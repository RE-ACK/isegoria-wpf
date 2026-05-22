[Setup]
AppName=Isegoria
AppVersion=1.0
DefaultDirName={pf}\Isegoria
DefaultGroupName=Isegoria
OutputBaseFilename=isegoria-setup
Compression=lzma
SolidCompression=yes

[Files]
Source: "D:\311\isegoria\isegoria-wpf\bin\Release\net10.0-windows\win-x64\publish\*"; \
        DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\Isegoria"; Filename: "{app}\isegoria-wpf.exe"
Name: "{commondesktop}\Isegoria"; Filename: "{app}\isegoria-wpf.exe"

[Run]
Filename: "{app}\isegoria-wpf.exe"; Description: "Isegoria 실행"; Flags: nowait postinstall