[Setup]
AppName=Visualizationizer1.0
AppVersion=1.0
AppId={{A unique GUID}}
DefaultDirName={pf}\Visualizationizer1.0
DefaultGroupName=Visualizationizer1.0
OutputDir=.
OutputBaseFilename=Visualizationizer1.0Installer
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Dirs]
Name: "{app}\SVGs"; Permissions: everyone-modify

[Files]
Source: "C:\Users\Zephyrus\source\repos\Visualizationizer\bin\Release\net6.0-windows\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Include icon file and README as well as SVGs folder and subfolders and .NET installer stuff
Source: "C:\Users\Zephyrus\source\repos\Visualizationizer\icon.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Zephyrus\source\repos\Visualizationizer\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Zephyrus\source\repos\Visualizationizer\bin\Release\net6.0-windows\win-x64\SVGs\*"; DestDir: "{app}\SVGs"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\Users\Zephyrus\source\repos\Visualizationizer\LibraryInstallers\x64\dotnet-runtime-6.0.24-win-x64.msi"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: NeedsDotNetRuntime

[Icons]
Name: "{commondesktop}\Visualizationizer1.0"; Filename: "{app}\Visualizationizer1.0.exe"; Tasks: desktopicon; IconFilename: "{app}\icon.ico"
Name: "{group}\Visualizationizer1.0"; Filename: "{app}\Visualizationizer1.0.exe"; IconFilename: "{app}\icon.ico"
Name: "{group}\Uninstall Visualizationizer1.0"; Filename: "{app}\Uninstall Visualizationizer1.0.exe"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop icon"; GroupDescription: "Additional tasks:"; Flags: unchecked

[Registry]
Root: HKCU; Subkey: "Software\Visualizationizer1.0"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: createvalueifdoesntexist

[Run]
Filename: "{app}\Visualizationizer1.0.exe"; Description: "{cm:LaunchProgram,Visualizationizer1.0}"; Flags: nowait postinstall skipifsilent
Filename: "notepad.exe"; Parameters: "{app}\README.md"; Description: "Read the README file"; Flags: postinstall skipifsilent; Check: ShouldShowReadme

[Code]
function GetDotNetRuntimeInstaller(): string;
begin
    Result := ExpandConstant('C:\Users\Zephyrus\source\repos\Visualizationizer\LibraryInstallers\dotnet-runtime-6.0.24-win-x64.msi')
end;

function GetArchitectureString: string;
begin
  if IsWin64 then
    Result := 'x64'
  else
    Result := 'x86';
end;

function NeedsDotNetRuntime(): Boolean;
begin
  // Check if .NET Runtime is installed or not
  Result := not RegKeyExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\dotnet\Setup\InstalledVersions\' + GetArchitectureString + '\sharedhost');
end;

function ShouldShowReadme: Boolean;
begin
  Result := (MsgBox('Congratulations!'#13#10 #13#10'Your Automated Software Installation is Complete!'#13#10 #13#10'If you know what you are doing, good for you, but for everyone else I suggest reading the README file.', mbInformation, MB_YESNO) = IDYES);
end;

var
  ErrorCode: Integer;

function InitializeSetup(): Boolean;
var
  DotNetInstallerPath: String;
begin
  DotNetInstallerPath := GetDotNetRuntimeInstaller;
  if NeedsDotNetRuntime then
  begin
    // Initialize ErrorCode
    ErrorCode := 0;
    // Execute the installer
    if not Exec(ExpandConstant(DotNetInstallerPath), '/quiet /norestart', '', SW_SHOW, ewWaitUntilTerminated, ErrorCode) then
    begin
      MsgBox('Installation of .NET 6.0 runtime failed with error code: ' + IntToStr(ErrorCode) + '. Setup will now exit.', mbError, MB_OK);
      Result := False;
      Exit;
    end;
  end;
  Result := True;
end;