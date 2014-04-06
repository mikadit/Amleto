; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{3C37B532-D178-4B8D-9169-08AB76E045B1}
AppName=Amleto
AppVerName=Amleto 3.3.4
AppPublisher=Virtualcoder
AppPublisherURL=http://virtualcoder.co.uk/
AppSupportURL=http://virtualcoder.co.uk/
AppUpdatesURL=http://virtualcoder.co.uk/
DefaultDirName={pf}\Amleto
DefaultGroupName=Amleto
OutputBaseFilename=Amleto
Compression=lzma
SolidCompression=yes
ChangesAssociations=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "Amleto\bin\Release\Amleto.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "AmletoClient\bin\Release\AmletoClient.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "RemoteExecution\bin\Release\RemoteExecution.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Amleto\bin\Release\NLog.*"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Amleto Server"; Filename: "{app}\Amleto.exe"
Name: "{group}\Amleto Client"; Filename: "{app}\AmletoClient.exe"
Name: "{group}\{cm:UninstallProgram,Amleto}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\Amleto Server"; Filename: "{app}\Amleto.exe"; Tasks: desktopicon
Name: "{commondesktop}\Amelto Client"; Filename: "{app}\AmletoClient.exe"; Tasks: desktopicon

[Run]
