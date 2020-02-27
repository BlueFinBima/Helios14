# Notes for Creating a Helios Build

## Check Dependencies

Fire up NuGet Manager and check the installed dependencies

There should be no need to download a new vc_redistx64.exe, however ensure that the ClickOnce Bootstrap file that is in 

helios\Helios Installer\ClickOnce Bootstrap Files\VC_redist.64.ClickOnce.Package.zip

is unpacked to 

C:\Program Files (x86)\Microsoft SDKs\ClickOnce Bootstrapper\Packages\vcredist_x64

## Building

Change Buildtype to "Release" and Target to x64
Perform a "Build" -> "Clean Solution"
Change the assembly dates on the projects
	This can be done with a "Replace All" for the previous version and the new one, but make sure you check the results from a 
		"Find All" in the solution first because it will include the ChangeLog.txt which should not have the date changed!
Change the publish versions on the editor and center - not sure where these are stored, so they need to be done manually 

! Not sure this is needed anymore !  Change the StatusBlock.Text in About.xaml.cs in both Helios and PE
Review and change ContributionBlock.text in About.xaml.cs in both Helios and PE
Review and make necessary changes to SetProjectReleaseMessage() in CC's MainWindow.xaml.cs
Update the "Helios.Read.Me.txt" file in the Helios project's root folder.
Update the "ChangeLog.md" file in the Helios project's root folder.

Commit the Dev branch

Perform Build-> Build Solution - and check that there were no failures

Create a zip file to include Helios Installer.msi and Helios Setup.exe from the Helios Installer directory

Run WinRAR Test on the archive

Create branch for the release (optional)

Create a new release in GitHub from the committed Dev branch, and attach a zip file containing the release of the code that
you're about to release.  Include the info from the most recent section of the ChangeLog.md

In the config manager within the new branch, tick the Installer to cause it to build.

Hide any interfaces by commenting out their [HeliosInterface("Helios.F14B", "DCS F-14B", typeof(F14BInterfaceEditor), typeof(UniqueHeliosInterfaceFactory))] eg F-14B 


 * * *  Sign with HeliosKeyPair password =

## Post Build Activities

In VS, do a CTRL-O and open Helios Setup.exe.  Delete the two existing icons, and RMB to add resource for a new icon and import 
helios\Helios Installer\Graphics\helios_Ks7_icon.ico

If the install dialogues need to be changed, then this is done in the properties of the Helios Installer project with F4 (RMB properties does not show these).

### Orca Actions

Open the .MSI with Orca and 
	There are a few things which the post build SQL cannot alter for some reason.  
	Replace All (exact match) in Orca for 1.4.2020 to 1.4.2020.mmdd 

	Check the Welcome Text for the installer.  You can then correct any welcome text issues in "Control" eg  "WelcomeText".  Copy the original text into a text editor, save 	it as a file and then "Import Text File"

		
Note: The Assembly file version information is saved as a set of integers for each portion of the numbering system, with the result that
		any leading zeros will always be lost!
		
		The file version stored at https://bluefinbima.github.io/Helios/HeliosCurrentVersion.xml is of the form:
		
		<HeliosVersion>
			<CurrentVersion>1.4.20190317</CurrentVersion>
			<DownloadUrl>
			Https://www.digitalcombatsimulator.com/en/files/3302014/
			</DownloadUrl>
		</HeliosVersion>
		ie without a decimal point between the last 8 digits.

		The file version stored at https://bluefinbima.github.io/Helios/HeliosCurrentVersionV2.xml is of the form:
		
		<HeliosVersion>
			<CurrentVersion>1.4.2019.0317</CurrentVersion>
			<DownloadUrl>
			Https://www.digitalcombatsimulator.com/en/files/3302014/
			</DownloadUrl>
		</HeliosVersion>
		ie with a decimal point between the last 4.4 digits.


## Information for the Download Site 

Helios is a virtual cockpit simulator system for aircraft in the DCS World.  With Helios, you can create virtual cockpits, which allow you to increase your immersion in your favourite combat aircraft.  Helios profiles can be created to allow you to simulate switches, knobs, gauges and more complex instruments which can then be mapped into DCS to give you a much improved combat pilot experience.  Many people use a touch screen monitor with their virtual cockpits.   It is also possible to run the Helios cockpit on a remote PC.

Helios was originally created by Craig "Gadroc" Courtney.  Gadroc donated his code to the open source community and this code is currently delivered out of the BlueFinBima fork on Github. 

=======================

This release features more many bug fixes (including some for BMS) and new controls.

Full change history is at https://github.com/BlueFinBima/Helios/blob/Dev/README.md

In addition to the functionality that many know and love, this release contains an interface for the AV-8B and a partial interface for the MiG-21, the latter being contributed by "Cylution".  Additionally there is now an interface for the F/A-18C Hornet, and simple interfaces for the Mirage 2000C as well as the Mi-8.  The Changelog.md distributed with the release describes the changes to Helios.

The rate of growth in the number of aircraft in DCS means that unless there are many more contributors to the Helios project, there will not be an interface for every aircraft type, however this release includes a generic aircraft interface that is designed to allow profile designers such as the remarkable Capt Zeen ( http://www.captzeen.com/helios/index.asp ) to more easily develop profiles for new aircraft.

A (short) basic overview of Helios can be viewed on https://www.youtube.com/watch?v=78to_NENQT8
and a video tutorial demonstrating Installing Helios for beginners is here https://www.youtube.com/watch?v=8n1cL1Szgmg

Tutorials about creating profiles can be viewed https://www.youtube.com/results?search_query=dcs+helios+tutorial and Gadroc's site at http://www.gadrocsworkshop.com/downloads/ still exists which contains profiles, older information and files relating to Helios.

The BlueFinBima fork of Helios development effort is aided & abetted by derammo; CaptZeen; KiwiLostInMelb; damien022; Will Hartsell; Cylution; Rachmaninoff; yzfanimal; Jabbers; Phar71; BeamRider

https://forums.eagle.ru/showthread.php?p=3637870 can be used for communications about this release of Helios.



====================================================================
Update my EDForums Signature

## Post Ship 

Create a new entry in the ChangeLog.md file so that new features can be added into this file.
Clean up and GH Issues that have been resolved in the shipped code. 
