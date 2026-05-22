[Setup]
AppName=Isegoria
AppVersion=1.0
DefaultDirName={pf}\Isegoria
DefaultGroupName=Isegoria
OutputBaseFilename=isegoria-setup
OutputDir=D:\311\isegoria\installer
Compression=lzma
SolidCompression=yes

[Files]
Source: "D:\311\isegoria\isegoria-wpf\bin\Release\net10.0-windows\win-x64\publish\*"; \
        DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\Isegoria"; Filename: "{app}\isegoria.exe"
Name: "{commondesktop}\Isegoria"; Filename: "{app}\isegoria.exe"

[Run]
Filename: "{app}\isegoria.exe"; Description: "Isegoria 실행"; Flags: nowait postinstall