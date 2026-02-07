#define MyAppName "Visualizationizer 1.1"
#define MyAppVersion "1.1.0"
#define MyAppExeName "Visualizationizer1.1.exe"
#define MyAppInstallDirName "Visualizationizer 1.1"
#define MyOutputBaseFilename "Visualizationizer1.1Installer"
#define MyPublishDir "..\bin\Release\net6.0-windows\win-x64\publish"

[Setup]
AppId={{B9A84866-4E2C-470E-AF26-93CC153C2FAE}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppInstallDirName}
DefaultGroupName={#MyAppInstallDirName}
OutputDir=.
OutputBaseFilename={#MyOutputBaseFilename}
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern

[Dirs]
Name: "{app}\SVGs"; Permissions: everyone-modify

[Files]
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\Icon.ico"; DestDir: "{app}"; DestName: "icon.ico"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{commondesktop}\{#MyAppInstallDirName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; IconFilename: "{app}\icon.ico"
Name: "{group}\{#MyAppInstallDirName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\icon.ico"
Name: "{group}\Uninstall {#MyAppInstallDirName}"; Filename: "{uninstallexe}"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop icon"; GroupDescription: "Additional tasks:"; Flags: unchecked

[Registry]
Root: HKLM; Subkey: "Software\Visualizationizer\1.1"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: createvalueifdoesntexist

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent
Filename: "notepad.exe"; Parameters: "{app}\README.md"; Description: "Read the README file"; Flags: postinstall skipifsilent
