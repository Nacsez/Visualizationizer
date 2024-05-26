; Visualizationizer1.0Installer.iss
[Setup]
AppName=Visualizationizer1.0
AppVersion=1.0
DefaultDirName={pf}\Visualizationizer1.0
DefaultGroupName=Visualizationizer1.0
OutputDir=.
OutputBaseFilename=Visualizationizer1.0Installer
Compression=lzma
SolidCompression=yes

[Files]
Source: "Bin\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Visualizationizer1.0"; Filename: "{app}\Visualizationizer1.0.exe"
Name: "{group}\Uninstall Visualizationizer1.0"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\Visualizationizer1,0.exe"; Description: "{cm:LaunchProgram,Visualizationizer}"; Flags: nowait postinstall skipifsilent