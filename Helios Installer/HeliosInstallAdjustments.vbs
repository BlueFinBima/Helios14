' This script actions the changes needed to the MSI package.
Option Explicit
Dim argNum, argCount:argCount = Wscript.Arguments.Count
if argCount = 0  then
   Wscript.Echo "Helios Installer Post Build changes requires an argument which is the" &_
   vbLf & " msi file to have the releases changed, and also set certain flags." &_
   vbLf & " No argument was passed into the HeliosInstallAdjustments.vbs file." &_
   vbLf & " SELECT queries will display the rows of the result list specified in the query"
	Wscript.Quit 1
else
   dim msiPackage:msiPackage = Wscript.Arguments(0)
   Wscript.Echo "Starting Post Build Script to correct: " & msiPackage
   Dim sNewRel,sOldRel,sUpgradeRel
   sUpgradeRel = "1.4." & Year(Date)
   dim reloffset:reloffset = cint(inputbox("Offset in days for the release number." & vbcrlf & sUpgradeRel & ".xxxx minus this offset","Release Offset","0"))
   sOldRel = sUpgradeRel & "." & Month(Date-reloffset) & RIGHT("0" & day(date-reloffset),2)
   sNewRel = sUpgradeRel & "." & RIGHT("0" & Month(Date-reloffset),2) & RIGHT("0" & day(date-reloffset),2)
   WScript.Echo "Current Dir:     " & CreateObject("WScript.Shell").CurrentDirectory
   Wscript.Echo "Upgrade Release: " & sUpgradeRel
   Wscript.Echo "Old Release:     " & sOldRel
   Wscript.Echo "New Release:     " & sNewRel
   Dim oShell 
   Set oShell = Wscript.CreateObject("WScript.Shell")
   oShell.Run "..\WiRunSQL.vbs """ & msiPackage & """  ""INSERT INTO Property(Property, Value) VALUES ('DISABLEADVTSHORTCUTS', '1')""",0,true
   oShell.Run "..\WiRunSQL.vbs """ & msiPackage & """  ""UPDATE Property SET Value = '" & sNewRel & "' WHERE Property = 'ProductVersion'""",0,true
   oShell.Run "..\WiRunSQL.vbs """ & msiPackage & """  ""UPDATE File SET Version = '" & sNewRel & "' WHERE Version = '" & sOldRel & "'""",0,true
   oShell.Run "..\WiRunSQL.vbs """ & msiPackage & """  ""UPDATE MsiAssemblyName SET Value = '" & sNewRel & "' WHERE Value = '" & sOldRel & "'""",0,true
   ' This next one is almost certain to fail, probably because it is attempting to update the primary key.
   'oShell.Run "..\WiRunSQL.vbs """ & msiPackage & """  ""INSERT INTO Upgrade(UpgradeCode, VersionMin, VersionMax,Language, Attribute,ActionProperty) VALUES ('{589D8667-3ED9-478B-8F67-A56E4FADBC63}', '1.4.2018','1.4.20199999','En',256,'NEWERPRODUCTFOUND')""",0,true
   'oShell.Run "..\WiRunSQL.vbs """ & msiPackage & """  ""UPDATE Upgrade SET VersionMin = '" & sNewRel & "' WHERE UpgradeCode = '{589D8667-3ED9-478B-8F67-A56E4FADBC63}'""",0,true
   Set oShell = Nothing 
end if
Wscript.Quit 0



 

