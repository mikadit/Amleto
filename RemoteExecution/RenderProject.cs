using System;
using System.Collections.Generic;
using RemoteExecution.Jobs;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Mail;
using System.Net;
using System.Diagnostics;
using System.Xml.Serialization;

namespace RemoteExecution
{
    [Serializable]
    [XmlRoot("RenderProject")]
    public class RenderProject
    {
        [XmlIgnore]
        public static int NextProjectId;

        [XmlElement("ContentDir")]
        public string ContentDir = "";
        [XmlElement("SceneFile")]
        public string SceneFile = "";
        [XmlElement("OutputDir")]
        public string OutputDir = "";
        [XmlElement("Prefix")]
        public string Prefix = "img_";
        [XmlElement("AlphaPrefix")]
        public string AlphaPrefix = "img_";
        [XmlElement("StartFrame")]
        public int StartFrame = 1;
        [XmlElement("EndFrame")]
        public int EndFrame = 60;
        [XmlElement("FrameStep")]
        public int FrameSteps = 1;
        [XmlElement("ProjectId")]
        public int ProjectId = -1;
		[XmlElement("OverrideSettings")]
		public bool OverrideSettings;
        [XmlElement("OverwriteFrames")]
        public bool OverwriteFrames;
        [XmlElement("FileNameFormat")]
        public int FileNameFormat;
        [XmlIgnore]
        public static string[] FileNameFormats ={ "{0}\\{1}{2:000}.", "{0}\\{1}{2:000}{3}", "{0}\\{1}{2:0000}.", "{0}\\{1}{2:0000}{3}", "{0}\\{1}_{2:000}.", "{0}\\{1}_{2:000}{3}", "{0}\\{1}_{2:0000}.", "{0}\\{1}_{2:0000}{3}" };
        [XmlElement("ImageFormat")]
        public int ImageFormat;
        [XmlElement("AlphaImageFormat")]
        public int AlphaImageFormat;
        [XmlElement("SaveAlpha")] 
        public bool SaveAlpha;
        [XmlIgnore]
        private List<RenderJob> _jobs = new List<RenderJob>();
        [XmlIgnore]
        public int StartJobs;
        [XmlIgnore]
        public TreeNode Node;
        [XmlElement("Antialias")]
        public int Antialias;
        [XmlElement("ReconFilter")]
        public int ReconFilter;
        [XmlElement("Width")]
        public int Width = 640;
        [XmlElement("Heigth")]
        public int Height = 480;
        [XmlElement("AspectRatio")]
        public double Aspect = 1.0;
        [XmlElement("RenderEffect")]
        public int RenderEffect;
        [XmlElement("RenderMode")]
        public int RenderMode;
        [XmlElement("RecursionLimit")]
        public int RecursionLimit = 16;
        [XmlElement("RenderLine")]
        public int RenderLine = 1;
        [XmlElement("AdaptiveSampling")]
        public int AdaptiveSampling;
        [XmlElement("AdaptiveThreashold")]
        public double AdaptiveThreshold = 0.1;
        [XmlElement("FilterType")]
        public int FilterType;
        [XmlElement("EnhanceAA")]
        public int EnhanceAA;
        [XmlElement("AntialiasLevel")]
        public int AntialiasLevel = -1;
        [XmlIgnore]
        public string Owner = "";
        [XmlIgnore]
        public bool Paused;
        [XmlElement("Block")]
        public int Block = 1;
        [XmlElement("ConfigNumber")]
        public int Config;
        [XmlArray("MasterStripped")]
        [XmlArrayItem("Stripped")]
        public List<string> StrippedMaster = new List<string>();
        [XmlElement("Slices")]
        public int Slices = 1;
        [XmlElement("Overlap")]
        public double Overlap = 5;
        [XmlElement("Camera")]
        public int Camera;
        [XmlElement("SamplingPattern")]
        public int SamplingPattern;
        [XmlElement("CameraAntialias")]
        public int CameraAntialias;
        [XmlElement("Radiosity")]
        public int Radiosity;
        [XmlElement("RadiosityType")]
        public int RadiosityType;
        [XmlElement("InterpolatedGI")]
        public int InterpolatedGI;
        [XmlElement("BackDropTransparency")]
        public int BackdropTranspGI;
        [XmlElement("CachedGI")]
        public int CachedGI;
        [XmlElement("VolumetricGI")]
        public int VolumetricGI;
        [XmlElement("UseAmbientGI")]
        public int UseAmbientGI;
        [XmlElement("DirectionalGI")]
        public int DirectionalGI;
        [XmlElement("IntensityGI")]
        public double IntensityGI;
        [XmlElement("ToleranceGI")]
        public double ToleranceGI;
        [XmlElement("RayGI")]
        public int RayGI;
        [XmlElement("MinEvalGI")]
        public double MinEvalGI;
        [XmlElement("MinPixelEvalGI")]
        public double MinPixelGI;
        [XmlElement("IndirectGI")]
        public int IndirectGI;
        [XmlElement("EmailNotify")]
        public bool EmailNotify;
        [XmlElement("EmailTo")]
        public string EmailTo = "";
        [XmlElement("EmailSubjectOk")]
        public string EmailSubjectOk = "";
        [XmlElement("EmailSubjectNok")]
        public string EmailSubjectNotOk = "";
        [XmlElement("EmailLog")]
        public bool EmailContainLog = true;
        [XmlIgnore]
        public bool IsFinished;

        [XmlIgnore]
        public List<FinishedFrame> RenderedFrames = new List<FinishedFrame>();

        [XmlIgnore]
        public string EmailFrom = "amleto@yourdomain.com";
        [XmlIgnore]
        public string SmtpServer = "mail.yourdomain.com";
        [XmlIgnore]
        public string SmtpUsername = "";
        [XmlIgnore]
        public string SmtpPassword = "";
        [XmlIgnore]
        public int SmtpPort = 25;
        [XmlIgnore]
        public string SmtpLogin = "";

        [XmlIgnore]
        public string Log = "";

		[XmlIgnore]
		public DateTime StartTime;
		[XmlIgnore]
		public bool StartTimeSet;

        private void CopyJobParams(RenderJob job)
        {
            job.Antialias = Antialias;
            job.ReconFilter = ReconFilter;
            job.Width = Width;
            job.Height = Height;
            job.Aspect = Aspect;
            job.RenderEffect = RenderEffect;
            job.RenderMode = RenderMode;
            job.RecursionLimit = RecursionLimit;
            job.RenderLine = RenderLine;
            job.AdaptiveSampling = AdaptiveSampling;
            job.AdaptiveThreshold = AdaptiveThreshold;
            job.FilterType = FilterType;
            job.EnhanceAA = EnhanceAA;
            job.AntialiasLevel = AntialiasLevel;
            job.SliceNumber = 0;
            job.TotSlices = Slices;
            job.Overlap = Overlap;
            job.Camera = Camera;
            job.SamplingPattern = SamplingPattern;
            job.CameraAntialias = CameraAntialias;
            job.Radiosity = Radiosity;
            job.RadiosityType = RadiosityType;
            job.InterpolatedGI = InterpolatedGI;
            job.InterpolatedGI = InterpolatedGI;
            job.CachedGI = CachedGI;
            job.VolumetricGI = VolumetricGI;
            job.UseAmbientGI = UseAmbientGI;
            job.DirectionalGI = DirectionalGI;
            job.IntensityGI = IntensityGI;
            job.ToleranceGI = ToleranceGI;
            job.RayGI = RayGI;
            job.MinEvalGI = MinEvalGI;
            job.MinPixelGI = MinPixelGI;
            job.IndirectGI = IndirectGI;
            job.SaveAlpha = SaveAlpha;
			job.OverrideSettings = OverrideSettings;
            job.OverwriteFrames = OverwriteFrames;

            foreach (string s in StrippedMaster)
                job.StrippedMaster.Add(s);
        }

        public void GenerateRenderJobs()
        {
            if (Log == "")
            {
                Log += "*******************************************************\n";
                Log += "Project: " + SceneFile + "\n";
                Log += "Id: " + ProjectId + "\n";
                Log += "Sent by: " + Owner + "\n";
                Log += "Sent on: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\n";
                Log += "*******************************************************\n";
            }
            else
            {
                Log += "*******************************************************\n";
                Log += "Project modified on: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\n";
                Log += "*******************************************************\n";
            }
            Node = new TreeNode(SceneFile.Substring(SceneFile.LastIndexOf('\\') + 1));

            if (!ContentDir.EndsWith("\\"))
                ContentDir += "\\";

            _jobs.Clear();
			
			if (Slices > 1) for (int i = 0; i < Slices; i++)
            {
                RenderJob job = new RenderJob(SceneFile.Substring(ContentDir.Length), StartFrame, StartFrame, 1, 0, ServerServices.Configs[Config].ImageFormats[ImageFormat], SaveAlpha, ServerServices.Configs[Config].ImageFormats[AlphaImageFormat]);
                //job.timeSpent = new Stopwatch();
                CopyJobParams(job);
                job.SliceNumber = i;

                _jobs.Add(job);
            }
            else 
				for (int i = StartFrame; i <= EndFrame; i += FrameSteps)
				{
                    // If the overwrite flag isn't set
                    // Check if the frame (and alpha) to render already exists in the output folder
                    string imgfmt = ServerServices.Configs[Config].ImageFormats[ImageFormat];
                    string ext = imgfmt.Substring(imgfmt.IndexOf('(') + 1, (imgfmt.IndexOf(')') - imgfmt.IndexOf('(')) - 1);
                    string fname = string.Format(FileNameFormats[FileNameFormat], OutputDir, Prefix, i, ext);
                    bool filesExist;
                    if (SaveAlpha)
                    {
                        string aimgfmt = ServerServices.Configs[Config].ImageFormats[AlphaImageFormat];
                        string aext = aimgfmt.Substring(aimgfmt.IndexOf('(') + 1, (aimgfmt.IndexOf(')') - aimgfmt.IndexOf('(')) - 1);
                        string aname = string.Format(FileNameFormats[FileNameFormat], OutputDir, AlphaPrefix, i, aext);
                        filesExist = File.Exists(fname) && File.Exists(aname);
                    }
                    else
                        filesExist = File.Exists(fname);

                    // If overwrite flag is set or if one or both the needed files are not in the output folder
                    if (OverwriteFrames || !filesExist)
					{
						RenderJob job = new RenderJob(SceneFile.Substring(ContentDir.Length), i, i, 1, 0,
							ServerServices.Configs[Config].ImageFormats[ImageFormat],
							SaveAlpha, ServerServices.Configs[Config].ImageFormats[AlphaImageFormat]);
						CopyJobParams(job);
						job.SliceNumber = 0;
						_jobs.Add(job);
					}
				}
            StartJobs = _jobs.Count;
        }

        public List<Job> GetContentJobs()
        {
            return GetContentJobs(ContentDir);
        }

        protected List<Job> GetContentJobs(string path)
        {
            List<Job> res = new List<Job>();

            if (!ContentDir.EndsWith("\\"))
                ContentDir += "\\";

            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                if(d.Name == "." || d.Name == "..")
                    continue;
                res.AddRange(GetContentJobs(d.FullName));
            }
            foreach (FileInfo f in dir.GetFiles())
            {
                res.Add(new DownloadJob(f.FullName, f.FullName.Substring(ContentDir.Length), f.Length, f.LastWriteTimeUtc));
            }
            return res;
        }

        public Job GetRenderJob(int clientId,int instance)
        {
            for (int i = 0; i < _jobs.Count; i++)
            {
                if (_jobs[i].Sent == false)
                {
                    _jobs[i].ClientId = clientId;
                    _jobs[i].Sent = true;
                    _jobs[i].Instance = instance;
                    _jobs[i].StartFrame = _jobs[i].EndFrame;
                    if (_jobs[i].TimeSpent == null)
                        _jobs[i].TimeSpent = new Stopwatch();
                    _jobs[i].TimeSpent.Reset();
                    _jobs[i].TimeSpent.Start();
                    if (Slices < 2 && FrameSteps < 2)
                    {
                        for (int j = 1; j < Block && (j + i) < _jobs.Count; j++)
                        {

                            // Check if the next frame is not in sequence. This can occur when re-rendering 
                            // a projects with existing frames in the output directory
                            if (_jobs[i + j].StartFrame != _jobs[i + j - 1].StartFrame + FrameSteps)
                                break;

                            if (_jobs[i + j].Sent)
                                break;
                            _jobs[i + j].Sent = true;
                            _jobs[i + j].ClientId = clientId;
                            _jobs[i + j].Instance = instance;
                            _jobs[i].EndFrame = _jobs[i + j].StartFrame;
                        }
                    }
                    return _jobs[i];
                }
            }
            return null;
        }

        public bool HasFreeJobs()
        {
        	foreach (RenderJob t in _jobs)
        		if (t.Sent == false)
        			return true;
        	return false;
        }

    	public string FinishFrame(int frame, int sliceNumber, string node)
        {
            for (int i = 0; i < _jobs.Count; i++)
            {
                if (_jobs[i].StartFrame == frame && _jobs[i].SliceNumber == sliceNumber)
                {
                    if (_jobs[i].TimeSpent != null && _jobs[i].TimeSpent.IsRunning)
                    {
                        _jobs[i].TimeSpent.Stop();
                        if(Slices > 1)
                            Log += DateTime.Now.ToLongTimeString() + " Rendered job frame " + frame + " slice " + sliceNumber + "  took " + _jobs[i].TimeSpent.Elapsed.ToString() + "\n";
                        else
                            Log += DateTime.Now.ToLongTimeString() + " Rendered job frame(s) starting at frame " + frame + " to " + _jobs[i].EndFrame + " by node " + node + " took " + _jobs[i].TimeSpent.Elapsed.ToString() + "\n";
                    }
                    _jobs.RemoveAt(i);
                    break;
                }
            }

            if (Slices > 1 && _jobs.Count == 0) // We finished all the slices!
            {
                string sImageFormat = ServerServices.Configs[Config].ImageFormats[ImageFormat];
                string ext = sImageFormat.Substring(sImageFormat.IndexOf('(') + 1, (sImageFormat.IndexOf(')') - sImageFormat.IndexOf('(')) - 1);
                string fname;
                Bitmap fullFrame = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
                BitmapData dst = fullFrame.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
				for (int i = 0; i < Slices; i++)
				{
					fname = string.Format(FileNameFormats[FileNameFormat], OutputDir, "slice_" + i + "_" + Prefix, StartFrame, ext);

					double realTopLine = (double) i*(double) Height/(double) Slices;
					double sliceHeight = (double) 1*(double) Height/(double) Slices;

					double topLine = realTopLine - (double) Height*Overlap/200.0;
					if (Overlap > 0)
						sliceHeight += (double) Height*Overlap/100.0;
					else
						sliceHeight += 1;
					if (topLine < 0.0)
					{
						sliceHeight -= topLine;
						topLine = 0;
					}
					if (topLine + sliceHeight >= Height)
						sliceHeight = (int) (Height - topLine);

					try
					{
						Bitmap part = new Bitmap(fname);
						BitmapData src = part.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly,
						                               PixelFormat.Format32bppArgb);
						unsafe
						{
							uint* ptrSrc = (uint*) (src.Scan0);
							uint* ptrDst = (uint*) (dst.Scan0);
							for (int j = (int) topLine; j <= (int) (topLine + Math.Ceiling(sliceHeight)); j++)
							{
								if (j < 0 || j >= Height)
									continue;
								if (j < realTopLine)
								{
									double mult = 1.0 - Math.Abs(realTopLine - j)/((double) Height*Overlap/200.0);
									double other = (1.0 - mult);
									for (int k = 0; k < Width; k++)
									{
										uint[] s = new uint[4];
										uint[] d = new uint[4];

										s[0] = (ptrSrc[j*Width + k] & 0xFF000000) >> 24;
										s[1] = (ptrSrc[j*Width + k] & 0x00FF0000) >> 16;
										s[2] = (ptrSrc[j*Width + k] & 0x0000FF00) >> 8;
										s[3] = ptrSrc[j*Width + k] & 0x000000FF;

										d[0] = (ptrDst[j*Width + k] & 0xFF000000) >> 24;
										d[1] = (ptrDst[j*Width + k] & 0x00FF0000) >> 16;
										d[2] = (ptrDst[j*Width + k] & 0x0000FF00) >> 8;
										d[3] = ptrDst[j*Width + k] & 0x000000FF;

										d[0] = (uint) ((double) s[0]*mult + (double) d[0]*other);
										d[1] = (uint) ((double) s[1]*mult + (double) d[1]*other);
										d[2] = (uint) ((double) s[2]*mult + (double) d[2]*other);
										d[3] = (uint) ((double) s[3]*mult + (double) d[3]*other);

										ptrDst[j*Width + k] = (d[0] << 24) | (d[1] << 16) | (d[2] << 8) | d[3];

										//ptrDst[j * width + k] = (uint)((double)ptrSrc[j * width + k] * mult + (double)ptrDst[j * width + k] * other);
									}
								}
								else
									for (int k = 0; k < Width; k++)
										ptrDst[j*Width + k] = ptrSrc[j*Width + k];
							}
						}
						part.UnlockBits(src);
						part.Dispose();
					}
					catch (Exception ex)
					{
						Debug.WriteLine("Error joining the split images: " + ex);
					}
				}
            	fname = string.Format(FileNameFormats[FileNameFormat], OutputDir, Prefix, StartFrame, ext, sliceNumber);
                fullFrame.UnlockBits(dst);
                fullFrame.Save(fname);
                fullFrame.Dispose();
                Log += DateTime.Now.ToLongTimeString() + " Full frame " + StartFrame + " reconstructed.\n";
                RenderedFrames.Add(new FinishedFrame(SceneId,fname));

                return fname;
            }
            
            return null;
        }

        public void ReleaseFrame(int frame, int sliceNumber, string node)
        {
            for (int i = 0; i < _jobs.Count; i++)
            {
                if (_jobs[i].StartFrame == frame && _jobs[i].SliceNumber == sliceNumber)
                {
                    Log += DateTime.Now.ToLongTimeString() + " Frame " + frame + " lost by node " + node;
                    _jobs[i].Retrials++;
                    if (_jobs[i].Retrials > 3)
                        _jobs.RemoveAt(i);
                    else
                    {
                        _jobs[i].ClientId = -1;
                        _jobs[i].Sent = false;
                    }
                    return;
                }
            }
        }

        public void ReleaseClientAllFrames(int clientId,string node)
        {
        	Log += DateTime.Now.ToLongTimeString() + " Lost client " + node + " all frames reset.";
        	foreach (RenderJob t in _jobs)
        	{
        		if (t.ClientId == clientId)
        		{
        			t.ClientId = -1;
        			t.Sent = false;
        			t.EndFrame = t.StartFrame;
        		}
        	}
        }

    	public int NbRemainingJobs()
        {
            return _jobs.Count;
        }

        public string SaveImage(int frame, int sliceNumber, byte[] img)
        {
            string sImageFormat = ServerServices.Configs[Config].ImageFormats[ImageFormat];
            string ext = sImageFormat.Substring(sImageFormat.IndexOf('(')+1, (sImageFormat.IndexOf(')') - sImageFormat.IndexOf('('))-1);
            string myPrefix = Prefix;
            if (Slices > 1)
                myPrefix = "slice_" + sliceNumber + "_" + Prefix;
            string fname = string.Format(FileNameFormats[FileNameFormat], OutputDir, myPrefix, frame, ext);

            File.WriteAllBytes(fname, img);

            RenderedFrames.Add(new FinishedFrame(SceneId,fname));          
            return fname;
        }

        public string SaveImageAlpha(int frame, int sliceNumber, byte[] img)
        {
            string sImageFormat = ServerServices.Configs[Config].ImageFormats[AlphaImageFormat];
            string ext = sImageFormat.Substring(sImageFormat.IndexOf('(') + 1, (sImageFormat.IndexOf(')') - sImageFormat.IndexOf('(')) - 1);
            string myPrefix = AlphaPrefix;
            if (Slices > 1)
                myPrefix = "slice_" + sliceNumber + "_" + AlphaPrefix;
            string fname = string.Format(FileNameFormats[FileNameFormat], OutputDir, myPrefix, frame, ext);

            File.WriteAllBytes(fname, img);

            RenderedFrames.Add(new FinishedFrame(SceneId, fname));
            return fname;
        }

        public string SceneId
        {
            get
            {
                return SceneFile.Substring(SceneFile.LastIndexOf('\\') + 1) + " (" + ProjectId + ")";
            }
        }

        public void RemoveAllJobs()
        {
            _jobs.Clear();
        }

        public void CloseLogs()
        {
            IsFinished = true;

            Log += "*******************************************************\n";
            Log += "Project finished on: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\n";
            Log += "*******************************************************\n";

            if (EmailNotify)
            {
                MailMessage mail = new MailMessage(new MailAddress(EmailFrom), new MailAddress(EmailTo));
                mail.Subject = EmailSubjectOk;

                if (EmailContainLog)
                    mail.Body = Log;
                else
                    mail.Body = "Project finished.";

                SmtpClient smtp = new SmtpClient(SmtpServer);
                if (SmtpUsername != "")
                    smtp.Credentials = new NetworkCredential(SmtpUsername, SmtpPassword);
                smtp.Send(mail);
                mail.Dispose();
            }
        }

        public bool Save(string filename)
        {
            bool success;

            try
            {
                XmlSerializer s = new XmlSerializer(typeof(RenderProject));
                TextWriter w = new StreamWriter(filename);
                s.Serialize(w, this);
                w.WriteLine("");
                w.WriteLine("<!-- Project Definition - Amleto 3.1 -->");
                w.WriteLine("<!-- (c) 2007 - Nodalideas.com -->");
                w.Close();
                success = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while saving: " + ex);
                success = false;
            }
            return success;
        }

        public RenderProject Load(string filename)
        {
            RenderProject project = new RenderProject();
            
            try
            {
                XmlSerializer s = new XmlSerializer(typeof(RenderProject));
                TextReader r = new StreamReader(filename);
                project = (RenderProject)s.Deserialize(r);
                r.Close();
            }
            catch (Exception ex)
            {
				Debug.WriteLine("Error while loading: " + ex);
			}
            return project;
        }
    }
}