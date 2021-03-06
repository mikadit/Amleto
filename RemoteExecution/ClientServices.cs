using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Remoting.Channels.Tcp;
using Microsoft.Win32;
using NLog;
using RemoteExecution.Jobs;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Activation;
using System.IO;

namespace RemoteExecution
{
    public class ClientServices
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public delegate void StatusStringChange(string msg);

        public static event StatusStringChange MessageConsumer;

        private static bool _isWorking;

        public static Process CurrentRenderProcess { get; set; }
        public static ClientServices CurrentInstance { get; set; }
        public static List<string> OldMessages { get; set; }

        public static string ConfigName { get; set; }
        public static bool IsRunning { get; set; }
        public static ClientSettings Settings;

        private ServerServices _server;
        private Queue<Job> _jobs = new Queue<Job>();
        private Thread _jobConsumer;
        private Thread _jobPumper;
        private bool _setupToDo = true;
        private bool _readyToStart;
        private Stopwatch _setupTime = new Stopwatch();
        private TcpChannel _channel;
        private object _lock = new object();
       

        public ClientServices()
        {
            CurrentInstance = this;
        }

        static ClientServices()
        {
            OldMessages = new List<string>();
            ConfigName = "";
            IsRunning = true;
            CurrentInstance = null;
            CurrentRenderProcess = null;
            Settings = new ClientSettings();
        }

        public void StartService()
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
            Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator = "'";

            RestoreSettings();

            IsRunning = true;
            IsWorking = false;

            _setupToDo = true;
            _readyToStart = false;

            _setupTime.Reset();
            _setupTime.Start();

            if (_jobConsumer != null)
                _jobConsumer.Abort();
            _jobConsumer = new Thread(JobsExecuter);
            _jobConsumer.IsBackground = true;
            _jobConsumer.Start();

            if (_jobPumper != null)
                _jobPumper.Abort();
            _jobPumper = new Thread(JobsPump);
            _jobPumper.IsBackground = true;
            _jobPumper.Start();

            if (Settings.AutoServerFinder)
            {
                AddMessage(0, "Searching for Amleto server...");
                ThreadPool.QueueUserWorkItem(SearchServer);
            }
            else
            {
                AddMessage(0,
                           "Attempting to connect to server " + Settings.ServerHost + " on port " + Settings.ServerPort);
                ThreadPool.QueueUserWorkItem(ConnectToServer);
            }
        }

        /// <summary>
        /// Gets the configuration settings from the registry
        /// </summary>
        public static void RestoreSettings()
        {
            Settings = ClientSettings.LoadSettings();
        }

        /// <summary>
        /// Saves the configuration too the registry 
        /// </summary>
        public static void SaveSettings()
        {
            ClientSettings.SaveSettings(Settings);
        }


        public static void AddMessage(int icon, string msg)
        {
            string fullMsg = icon + "|" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "|" + msg;
            if (Settings.SaveToLog && Settings.LogFile != "")
            {
                try
                {
                    File.AppendAllText(Settings.LogFile, fullMsg + "\r\n");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error sending message: " + msg);
                }
            }

            while (OldMessages.Count > 3000)
                OldMessages.RemoveAt(0);

            OldMessages.Add(fullMsg);

            if (MessageConsumer != null)
            {
                foreach (Delegate d in MessageConsumer.GetInvocationList())
                {
                    try
                    {
                        d.DynamicInvoke(new object[] {fullMsg});
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Error sending message: " + fullMsg);
                    }
                }
            }
        }

        public static void SetRenderProcess(Process p)
        {
            CurrentRenderProcess = p;
        }

        public static void KillRenderProcess()
        {
            try
            {
                if (CurrentRenderProcess != null)
                    CurrentRenderProcess.Kill();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error killing render process");
            }

            try
            {
                CurrentRenderProcess = null;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error killing render process");
            }
        }

        public static void SetRenderProcessPriority(ProcessPriorityClass newPriority)
        {
            if (CurrentRenderProcess != null)
                CurrentRenderProcess.PriorityClass = newPriority;
            Settings.RenderPriority = newPriority;

            // Now set as default
            SaveSettings();
        }

        private void SearchServer(object obj)
        {
            BroadcastFinder finder = new BroadcastFinder();
            Settings.ServerPort = finder.Port;
            Settings.ServerHost = finder.Server;
            if (Settings.ServerHost == "")
            {
                AddMessage(1, "Amleto server has not been found.");
                AddMessage(1, "Check that your firewall allows access to port 61111 on the server");
                AddMessage(1, "and the server is currently running");
            }
            else
            {
                AddMessage(0, "Found Amleto server at " + Settings.ServerHost + " on port " + Settings.ServerPort);
            }

            AddMessage(0, "Attempting to connect to the Server.");
            ThreadPool.QueueUserWorkItem(ConnectToServer);
        }

        public bool IsWorking
        {
            get
            {
                lock (_lock)
                {
                    return _isWorking;
                }
            }

            set
            {
                lock (_lock)
                {
                    _isWorking = value;
                }
            }
        }

        private string GetIP()
        {
            IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        private void ConnectToServer(object obj)
        {
            if (Settings.ServerHost == "" || Settings.AutoServerFinder)
            {
                BroadcastFinder finder = new BroadcastFinder();
                Settings.ServerPort = finder.Port;
                Settings.ServerHost = finder.Server;
            }

            if (_channel == null)
            {
                _channel = new TcpChannel();
                ChannelServices.RegisterChannel(_channel, false);
            }

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    _server = (ServerServices)Activator.CreateInstance(typeof(ServerServices), null,
                                                 new object[]
                                                     {
                                                         new UrlAttribute("tcp://" + Settings.ServerHost + ":" +
                                                                          Settings.ServerPort)
                                                     });
                    if (_server.IsWorking())
                        break;
                    Thread.Sleep(100);
                }
                catch (Exception)
                {
                    _server = null;
                    // Don't log this error as this is expected if the server isn't runningB
                }
            }

            if (_server == null)
            {
                if (_setupToDo)
                    AddMessage(1, "Cannot connect to the server. Will retry in 5 seconds.");

                Thread.Sleep(5000);
                ThreadPool.QueueUserWorkItem(ConnectToServer);
            }
            else
            {
                lock (_lock)
                {
                    int bitsize = Environment.Is64BitOperatingSystem ? 64 : 32;
                    _server.RegisterClient(Environment.MachineName, GetIP(), Settings.RenderPriority, bitsize);
                }
                AddMessage(0, "Connected to the server " + Settings.ServerHost + " on port " + Settings.ServerPort + ".");
                try
                {
                    if (_setupToDo)
                        _server.SetCurrentJob("Starting up");
                }
                catch (Exception ex)
                {
                    _server = null;
                    if (_setupToDo)
                        AddMessage(1, "Cannot connect to the server. Will retry in 5 seconds.");
                    Thread.Sleep(5000);
                    ThreadPool.QueueUserWorkItem(ConnectToServer);
                    logger.Error(ex, "Error connecting to the server");
                }
            }
        }

        private bool AttemptServerConnect(string server, int port)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    _server = (ServerServices) Activator.CreateInstance(typeof (ServerServices), null,
                        new object[] {new UrlAttribute("tcp://" + server + ":" + port)});
                    if (_server.IsWorking())
                        return true;
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    _server = null;
                    logger.Error(ex, "Error connecting to the server");                    
                }
            }
            return false;
        }

        private void JobsPump()
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
            Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator = "'";

            while (IsRunning)
            {
                RetreiveJobs();
                Thread.Sleep(500);
            }
        }

        private void RetreiveJobs()
        {
            if (_server == null)
                return;
            try
            {
                if (_server.KeepAlive() == false)
                {
                    try
                    {
                        _server.Unregister();
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Error server.Unregister");
                    }

                    _server = null;
                    ThreadPool.QueueUserWorkItem(ConnectToServer);
                    return;
                }

                // Checks if we have any priority jobs to execute. If yes do it ASAP
                if (_server.HasPriorityJobs())
                {
                    List<Job> newJobs = _server.GetPriorityJobs();
                    foreach (Job j in newJobs)
                    {
                        j.SetServer(_server);
                        j.ExecuteJob(AddMessage, _jobs);
                    }
                }

                lock (_jobs)
                {
                    if (_setupToDo)
                        DoSetup();
                    else if (!_readyToStart && !_setupToDo && _jobs.Count == 0)
                    {
                        AddMessage(0, "Client ready.");
                        _setupTime.Stop();
                        AddMessage(0, "Startup and setup took " + _setupTime.Elapsed.TotalSeconds + " sec(s).");
                        _readyToStart = true;
                    }
                    else if (_jobs.Count > 0)
                        return;
                }

                if (IsWorking)
                    return;
                // Checks if we have new jobs to do
                if (_server.HasJobs())
                {
                    // If yes, lock the jobs queue and add the new one
                    lock (_jobs)
                    {
                        List<Job> newJobs = _server.GetJobs();
                        foreach (Job j in newJobs)
                            _jobs.Enqueue(j);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error retrieving jobs");
                try
                {
                    if (_server != null) _server.KeepAlive();
                }
                catch
                {
                    _server = null;
                    AddMessage(1, "Lost contact with Server");
                    ThreadPool.QueueUserWorkItem(ConnectToServer);
                }
            }
        }

        private void JobsExecuter()
        {
            bool needToSleep = true;

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
            Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator = "'";

            while (IsRunning)
            {
                if (needToSleep)
                    Thread.Sleep(100);
                needToSleep = false;

                Job jobToDo = null;
                lock (_jobs)
                {
                    if (_jobs.Count > 0)
                        jobToDo = _jobs.Dequeue();
                    else
                        needToSleep = true;
                }

                if (jobToDo != null)
                {
                    IsWorking = true;
                    try
                    {
                        jobToDo.SetServer(_server);
                        jobToDo.ExecuteJob(AddMessage, _jobs);

                        lock (_jobs)
                        {
                            // Checks if we have new jobs to do
                            if (_jobs.Count == 0 && _server.HasJobs())
                            {
                                // If yes, lock the jobs queue and add the new one
                                List<Job> newJobs = _server.GetJobs();
                                foreach (Job j in newJobs)
                                    _jobs.Enqueue(j);
                            }

                            if (_jobs.Count == 0)
                                IsWorking = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Error executing job");
                        IsWorking = false;
                    }
                }
            }
        }

        public static void ReDoSetup()
        {
            if (CurrentInstance._server.IsFirstClient())
                CurrentInstance._jobs.Enqueue(new SetupJob());
            else // Wait the first client...
                CurrentInstance._jobs.Enqueue(new WaitFirstJob());
        }

        private void DoSetup()
        {
            ConfigName = _server.GetConfigName();
            _setupToDo = false;

            if (_server.IsFirstClient())
                _jobs.Enqueue(new SetupJob());
            else // Wait the first client...
                _jobs.Enqueue(new WaitFirstJob());
        }

        public static void Shutdown()
        {
            IsRunning = false;
            KillRenderProcess();
            Thread.Sleep(600);
            SaveSettings();
            
            try
            {
                CurrentInstance._server.Unregister();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error shutting down");
            }
        }

        public static void ChangePriority()
        {
            if (CurrentInstance._server != null)
                CurrentInstance._server.ChangeClientPriority(Settings.RenderPriority);
        }

        public static string GetClientDir()
        {
            return Settings.ClientDir;
        }

        public static bool IsMainConfig(string filename)
        {
            string[] lines = File.ReadAllLines(filename);

            foreach (string line in lines)
            {
                if (line.StartsWith("CommandDirectory "))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
