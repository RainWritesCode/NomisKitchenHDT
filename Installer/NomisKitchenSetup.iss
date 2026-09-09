#define AppName "Nomi's Kitchen"
#define AppShortName "NomisKitchenHDT"
#define AppVersion "1.0.0"
#define AppPublisher "RainWritesCode"
#define AppURL "https://github.com/RainWritesCode/NomisKitchenHDT"

[Setup]
AppId={{5A7C8F1B-4E2A-4D3F-9B1A-3C5E9F7A2D18}}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}/issues
AppUpdatesURL={#AppURL}/releases
DefaultDirName={userappdata}\HearthstoneDeckTracker\Plugins\{#AppShortName}
DisableDirPage=yes
DisableProgramGroupPage=yes
DisableWelcomePage=no
DisableReadyPage=no
DisableReadyMemo=no
DisableFinishedPage=no
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog commandline
OutputDir=..\bin\Installer
OutputBaseFilename=NomisKitchenSetup-v{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
WizardResizable=no
AllowNoIcons=yes
UninstallDisplayName={#AppName}
UninstallDisplayIcon={app}\{#AppShortName}.dll

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Types]
Name: "full";    Description: "Full install"
Name: "hdt";     Description: "HDT plugin only"
Name: "custom";  Description: "Custom install";  Flags: iscustom

[Components]
Name: "hdt";     Description: "HDT plugin (APM overlay)";        Types: full hdt custom; Flags: fixed
Name: "bepinex"; Description: "BepInEx runtime (if missing)";    Types: full custom
Name: "numfix";  Description: "Disable abbreviation";            Types: full custom

[Files]
Source: "..\bin\Release\NomisKitchenHDT.dll"; DestDir: "{app}"; Components: hdt; Flags: ignoreversion

Source: "..\Resources\com.community.hs.NomiHatesAbbreviation.dll"; DestDir: "{code:GetHsDir}\BepInEx\plugins"; \
    Components: numfix; Flags: ignoreversion external skipifsourcedoesntexist; \
    Check: HsDirIsValid

Source: "BepInEx\*"; DestDir: "{code:GetHsDir}"; \
    Components: bepinex; Flags: recursesubdirs createallsubdirs onlyifdoesntexist; \
    Check: BepInExNotYetInstalled

[Code]
var
  HsDirPage: TInputDirWizardPage;

function GetHsDir(Param: string): string;
begin
  if Assigned(HsDirPage) and (HsDirPage.Values[0] <> '') then
    Result := HsDirPage.Values[0]
  else
    Result := '';
end;

function TryReadHsRegistry(): string;
var
  s: string;
begin
  Result := '';
  if RegQueryStringValue(HKLM32, 'SOFTWARE\Blizzard Entertainment\Hearthstone', 'InstallLocation', s) then begin
    Result := s;
    exit;
  end;
  if RegQueryStringValue(HKLM64, 'SOFTWARE\Blizzard Entertainment\Hearthstone', 'InstallLocation', s) then begin
    Result := s;
    exit;
  end;
  if RegQueryStringValue(HKLM32, 'SOFTWARE\Blizzard Entertainment\Hearthstone', 'InstallPath', s) then begin
    Result := s;
    exit;
  end;
end;

function GuessHsDir(): string;
var
  candidates: TArrayOfString;
  i: Integer;
begin
  Result := TryReadHsRegistry();
  if Result <> '' then exit;

  SetArrayLength(candidates, 5);
  candidates[0] := ExpandConstant('{pf32}\Hearthstone');
  candidates[1] := ExpandConstant('{pf}\Hearthstone');
  candidates[2] := 'C:\Program Files (x86)\Hearthstone';
  candidates[3] := 'C:\Program Files\Hearthstone';
  candidates[4] := 'I:\Hearthstone';

  for i := 0 to GetArrayLength(candidates) - 1 do
    if FileExists(candidates[i] + '\Hearthstone.exe') then begin
      Result := candidates[i];
      exit;
    end;
end;

function HsDirIsValid(): Boolean;
var
  d: string;
begin
  d := GetHsDir('');
  Result := (d <> '') and FileExists(d + '\Hearthstone.exe');
end;

function BepInExNotYetInstalled(): Boolean;
var
  d: string;
begin
  d := GetHsDir('');
  Result := (d <> '') and FileExists(d + '\Hearthstone.exe') and not FileExists(d + '\winhttp.dll');
end;

procedure InitializeWizard();
var
  guess: string;
begin
  HsDirPage := CreateInputDirPage(
    wpSelectComponents,
    'Hearthstone folder',
    'Where is Hearthstone installed?',
    'Change this if the detected path is wrong. Leave blank to skip Hearthstone parts. The HDT plugin still installs.',
    False,
    ''
  );
  HsDirPage.Add('');
  guess := GuessHsDir();
  if guess <> '' then
    HsDirPage.Values[0] := guess;
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
  if PageID = HsDirPage.ID then begin
    if not (WizardIsComponentSelected('numfix') or WizardIsComponentSelected('bepinex')) then
      Result := True;
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  d: string;
begin
  Result := True;
  if CurPageID = HsDirPage.ID then begin
    d := HsDirPage.Values[0];
    if d = '' then exit;
    if not FileExists(d + '\Hearthstone.exe') then begin
      MsgBox('No Hearthstone.exe in that folder. Pick the right one or clear the box to skip.', mbError, MB_OK);
      Result := False;
    end;
  end;
end;

[Run]
Filename: "{app}\README.md"; Description: "Open README"; Flags: postinstall shellexec skipifsilent unchecked

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
