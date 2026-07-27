#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif

#define MyAppName "Codex Lid Awake"
#define MyAppExeName "CodexLidAwake.exe"

[Setup]
AppId={{9A67F50B-A12A-4FE2-A458-7D491BD3D6C2}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=FrankJing420
AppPublisherURL=https://github.com/FrankJing420/codex-lid-awake
AppSupportURL=https://github.com/FrankJing420/codex-lid-awake/issues
DefaultDirName={autopf}\Codex Lid Awake
DefaultGroupName=Codex Lid Awake
UninstallDisplayIcon={app}\{#MyAppExeName}
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\..\release
OutputBaseFilename=CodexLidAwakeSetup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupLogging=yes
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Codex Lid Awake"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Codex Lid Awake"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "快捷方式："; Flags: unchecked

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "启动 Codex Lid Awake"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{app}\{#MyAppExeName}"; Parameters: "--restore"; Flags: runhidden waituntilterminated skipifdoesntexist; RunOnceId: "RestoreLidSleep"
