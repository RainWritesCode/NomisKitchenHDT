#define AppName "Nomi's Kitchen"
#define AppShortName "NomisKitchenHDT"
#define AppVersion "1.0.2"
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
CloseApplications=yes
CloseApplicationsFilter=*.dll,*.exe
RestartApplications=no
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
Name: "hdt";    Description: "HDT plugin (APM overlay)";       Types: full hdt custom; Flags: fixed
Name: "apm";    Description: "APM provider (required for APM)"; Types: full hdt custom
Name: "numfix"; Description: "Disable abbreviation";           Types: full custom

[Files]
Source: "..\bin\Release\NomisKitchenHDT.dll"; DestDir: "{app}"; Components: hdt; Flags: ignoreversion

Source: "..\Resources\com.community.hs.NomiHatesAbbreviation.dll"; DestDir: "{code:GetHsDir}\BepInEx\plugins"; \
    Components: numfix; Flags: ignoreversion; \
    Check: HsDirIsValid

Source: "..\Resources\com.community.hs.NomisKitchenApm.dll"; DestDir: "{code:GetHsDir}\BepInEx\plugins"; \
    Components: apm; Flags: ignoreversion; \
    Check: HsDirIsValid

; BepInEx runtime installs automatically whenever a game-side feature (APM or
; number-fix) is selected and it is not already present. It is not an optional
; box the user can forget, because APM and number-fix cannot run without it.
Source: "BepInEx\*"; DestDir: "{code:GetHsDir}"; \
    Components: apm numfix; Flags: recursesubdirs createallsubdirs onlyifdoesntexist; \
    Check: NeedBepInEx

[Code]
var
  HsDirPage: TInputDirWizardPage;
  gNeedBepInEx: Boolean;

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

  SetArrayLength(candidates, 4);
  candidates[0] := ExpandConstant('{pf32}\Hearthstone');
  candidates[1] := ExpandConstant('{pf}\Hearthstone');
  candidates[2] := 'C:\Program Files (x86)\Hearthstone';
  candidates[3] := 'C:\Program Files\Hearthstone';

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
  Result := (d <> '') and FileExists(d + '\Hearthstone.exe') and not FileExists(d + '\BepInEx\core\BepInEx.dll');
end;

function NeedBepInEx(): Boolean;
begin
  Result := gNeedBepInEx;
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
  guess := ExpandConstant('{param:HSDIR|}');
  if guess = '' then guess := GuessHsDir();
  if guess <> '' then
    HsDirPage.Values[0] := guess;
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
  if PageID = HsDirPage.ID then begin
    if not (WizardIsComponentSelected('numfix') or WizardIsComponentSelected('apm')) then
      Result := True;
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  d: string;
  needsGame: Boolean;
begin
  Result := True;
  if CurPageID = HsDirPage.ID then begin
    d := HsDirPage.Values[0];
    needsGame := WizardIsComponentSelected('apm') or WizardIsComponentSelected('numfix');

    if (d <> '') and not FileExists(d + '\Hearthstone.exe') then begin
      if not WizardSilent() then MsgBox('No Hearthstone.exe in that folder. Pick the folder that contains Hearthstone.exe.', mbError, MB_OK);
      Result := False;
      exit;
    end;

    // APM and number-fix cannot work without a valid Hearthstone folder, so do
    // not let the user proceed with an empty path while those are selected.
    if needsGame and (d = '') then begin
      if not WizardSilent() then MsgBox('The APM overlay needs your Hearthstone folder to install its in-game component.' + #13#10 +
             'Pick the folder that contains Hearthstone.exe (usually in Program Files (x86)\Hearthstone).', mbError, MB_OK);
      Result := False;
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  d: string;
begin
  if CurStep = ssInstall then gNeedBepInEx := BepInExNotYetInstalled();
  if CurStep = ssPostInstall then begin
    if WizardIsComponentSelected('apm') then begin
      d := GetHsDir('');
      if (d <> '') and not FileExists(d + '\BepInEx\core\BepInEx.dll') then
        if not WizardSilent() then MsgBox('Warning: BepInEx does not appear to be installed in:' + #13#10 + d + #13#10 +
               'The APM overlay will show 0 until BepInEx is present. Re-run this installer and confirm the Hearthstone folder.',
               mbError, MB_OK);
    end;
  end;
end;

[Run]
Filename: "{app}\README.md"; Description: "Open README"; Flags: postinstall shellexec skipifsilent unchecked

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
