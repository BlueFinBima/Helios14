## Full change log

### 1.4.2020.0315 Release
----------------------------
* Fix for some post installation file level strangeness.  #204
* Fix for a silent failure due to a problem with RWR Bezel
* Significant restructure of the installer
* HeliosDiag.cmd Data Privacy Notice

### 1.4.2020.0308 Release
----------------------------
* Regression fix for change in Lua 5.3 interpreter causing bindings to return integer #201
* Regression fix for rotary switch not working for the last position
* Installer change to make Iris and Keyboard Receiver delivery optional
* Improved diagnostics during initialisation
* New shortcuts for Control Center and Profile Editor to start with loglevel debug
* Removed Visual C++ Runtime dependency
* Control Center Version Check now checks GitHub and not gh-pages.  Also it has a better "Reminder" mechanism.  
	Version check added to "About" dropdown in Profile Editor #200

### 1.4.2020.0226 Release
----------------------------
#### Huge "Shout out" to derammo who was this release's primary contributor.  1.4.2020.0226 benefits from his many improvements and bugfixes.

* BMS F-16 - Altimeter fixes
	Added altimeter pressure reading
	fix for _airPressureDrum.Value only can accept 4 digits
	Added Altimeter Calibration reading from FlightData2
	Support barometric readings of altitude and pressure
	Use math.abs to get absolute value
	Read values from flightData2 to support barometric altitude and calibrated airpressure
* Keyboard Interface changes
	Bug relating to sending keys "RETURN" and "ENTER" fixed.  Both of these now send the ENTER key on
	the right of the main keyboard.  New "NUMPADENTER" sends the Enter key on the numeric keypad.
	This potentially needs profiles sending "RETURN" which previously activated the numeric pad enter
	to be changed to send "NUMPADENTER" instead #178
	New feature to force QWERTY keyboard keycodes to be sent to programs (such as BMS) which only
	understand keycodes and not keystrokes. #164
* A-10C
	fixed button command for MFCD brightness down
	Corrections to the values used by the interface for VHF AM Radio
* Bug fix for orphaned triggers and actions when there is a change in interface #173
* UDP traffic logging now only for loglevel Debug
* Fix for Reset monitors Bug where bindings don't move to the correct monitor #177 & #117
* Fix for CLR20r3 crash on release check when there is no internet connection #163
* Fix for crash on startup when Helios Settings file has been corrupted #106
* Fix for Text Display font size being incorrect when textbox is resized #146
* Fix for ArgumentOutOfRangeException on HeliosTextBox #158
* Installer changes to re-req the Visual C++ 2015-2019 Redistributable Runtime.  
	The installation is only attempted when using "Helios setup.exe".  14.24.28127.04 is the minimum level.
* Helios is now an x64-only offering
* HeliosDiag.cmd is delivered to aid the collection of diagnostic information
* Package Updates:
	* HidSharp 2.1
	* KeraLua 1.0.2.6
	* NLua 1.4.29 (including Lua 5.3)
* New Custom Drum and Custom Gauge controls added
* Fix for Reset Monitors: on reset from 5 to 3 screens, all controls lost #194

### 1.4.2019.1005 Release
----------------------------
* A-10C Interface
	* New devices for AN_ALR69V, TISL, DVADR and ACCELEROMETER
	* A-10C Interface completed, improvements in GuardedSwitch and CustomGauge, and new rotation option
	* Fixed:
		* "101", "IFFCC", "Ext Stores Jettison", fixed output buttom number.
		* All TISL values changed from IFFCC to the new device TISL.
		* Corrected the output buttons number of all IFF functions for the correct ones.
		* Added one missing position to Intercom, Transmitter Select Dial.

* M2000C Interface 
	* What’s new 
		* Improvement of guards on the Test and LG Panels: Made all 4 guards on this 2 panels working
		* Improvement of the Tacan Panel: add the 2 potentiometers and made them continuous and working
		* Improvement of VOR/ILS Panel: added the 2 potentiometers and made them continuous and working and made the Mode Selector working
		* Improvement of the HSI Panel: all the needles are working
	* Known issues 
  	  * Fuel Panel: the Fuel CrossFeed Switch doesn’t work, the two toggle switches don’t work (seem not to be used in DCS)
   	  * Master Caution lights Panel: still not working despite the modification with IndicatorPushButton. Seems the id is not the good one when clicking on the 2 indicators ?
          * HSI Panel: the potentiometer doesn’t work properly
          * ECM Box: the potentiometer is not working

* Controls:
	* Custom Gauge, chages to improve workflow and visuals
	* Guarded switch, added a new image for open state

* General:
	* Mousewheel support for some rotaries and pots in Control Center
	* Added a new rotation option on the editor: 180º Turn

* Interfaces DCS common:
	New DualRocker function, work like the normal rocker but this one return the actual arg value form DCS, too.

#### Bug Fixes:
* Fix for duplicate bindings #156 problem with autobindings

		
### 1.4.2019.0930 Release
----------------------------
* M2000C Interface 
	* Added the Fuel Panel with the 7 empty tanks indicators, the refueling indicator, the left and right neddle gauges and the JAUG and DETOT drums
	* Added the PPA Panel with the 8 indicators on the buttons, the 4 buttons and the 5 switches
	* Added the PCA Panel with the 17 indicators on the buttons, the 11 buttons, the 2 switches and the guard on the Jettison switch
	* Added the PCN	Panel with the 9 indicators, the 9 indicators buttons, the 10 keypad buttons and the Parameter selector working only from DCS to Helios
	* Added the TACAN Panel with the frequency display and the 2 switches (X/Y Mode and Mode Select) working only from DCS to Helios
	* Added the VOR/ILS Panel with the frequency display and the 2 switches (Power and Mode Select)	
	* Added the Engine Sensors Panel (still in beta) with the 2 needles and the drum
	* Added the HSI Panel (still in beta) with the drum, the 4 stop flags, the 2 needles, the green needle, the mode needle, the compass rose, the switch and the potentiometer
	* Added the ECM Box with the potentiometer and the 3 switches
	* Added the INS Panel with the 2 switches
	* Added the Test Panel with the 4 indicators, the 3 switches and the 3 guards on the switches
	* Improvement of the Engine Panel by adding the 2 guards
	* Improvement of the Landing Gear Panel by adding the guard for the gun safe switch  and the emergency landing gear lever
	* Improvement of the pictures of the cockpit to allow the new panels to work fine
	* Improvement of the Master Caution lights Panel, trying to make the 2 indicators clickable with the same solution as the PCN Panel
	* Miscellaneous: Added the Post Combustion Indicator, the 2 Fire Warning Indicators, the Demar Indicator, the 5 ] indicators, the AOA needle

* Profile Editor now polices only adding one UDP interface.
* AV-8B Interface Elements
	* Autobinding Master Arm Panel added
	* Autobinding Gear and Flaps panel added
	* Autobinding Gear Indicator Panel added
	* Autobinding H2O Panel added
	* Autobinding Threat Indicator panel added
	* Autobinding RWR control panel added
	* Autobinding Cockpit as a single control
	* Autobinding AV-8B Radios now use text displays saving viewport exports
	* Known issues: there has had to be some breaking changes, so profiles may not work completely as they once did, but all new items are autobinding so hopefully can be fixed easily.
* Small change to UFC font to allow AV-8B decimal point 

### 1.4.2019.0908 Release
----------------------------
* Adjustments to the method to identify the scripts directory
* Graphical improvements to AV-8B SMC, FQIS, ODU, EDP, UFC, MFD including support for automatically
	binding the controls to the AV-8B interface if it has been added.  
	* Note:  We've tried to limit any beakages, but there are certain to be some issues with existing
	       AV-8B profiles.
* AV-8B ODU and UFC text based displays - to avoid viewports for these devices
* Fix for problem with new Export.lua not sending data after the first set of sends
* Refactoring to move images and templates from Helios.dll into aircraft specific dlls.  Any profiles which make use
	of images for M2000C, AV-8B or F/A-18C embedded within Helios will have to change references 
	from 
		`{helios}\Images\aircraft\xxxx.png`
	to 
		`{M2000C}\Images\xxxx.png, {AV-8B}\Images\xxxx.png, {FA-18C}\Images\xxxx.png`
* New Autobinding Caution Panel for Mirage 2000C
* Updates to the Interfaces for A-10C, F/A-18C, AV-8B, and Mirage 2000C to improve integration with the different
	types of DCS World installations

### 1.4.2019.0901 Release
----------------------------
* Profile Editor now polices only adding only one UDP interface.
* AV-8B Autobinding Cockpit as a single control
* AV-8B Radios now use text displays saving viewport exports
* A-10C DLL added and images can be referenced by `{A-10C}/Images/xxxx.png`
	
#### Shadowman's M2000C Status as of 31st August 2019

	* Caution Panel:  What is working:  all the indicators; all the switches
	* Master Caution Lights Panel:  What is working: 2 red and yellow PANNE indicators
	* Start Engine Panel:  The 5 switches work
	* Landing Gear Panel: the 3 switches, the landing gear lever, the emergency jettison lever and all the indicators are working	
	* Other:  Testing is incomplete, but more of the switches should work.				
	
	* Known Issues
		* Master Caution Lights Panel: this indicator has to be clickable to aknowledge the audio alarm and light off the 2 indicators
		* Start Engine Panel: Temporarily removed the two red Guard Covers because we are not able to use the switches/button under
		* Landing Gear Panel: two red Guard Covers removed; the emergency landing gear lever : it seems there is nothing coming from DCS
			and need to work a little on the RotarySwitch in CompositoVisual.cs

### 1.4.2019.0823 Release
----------------------------
* Adjustments made to templates for F-16 gauges, parts, etc in toolbox 
* Empty panel template created for toolbox
* Helios exports.lua plays nicely when in a chain of exports.
* Breaking backwards compatibility on the Hornet UFC (*** you'll need to re-add the F/A-18C Export.lua interface ***) 
*	Bug fix for Hornet UFC font (now 1.2)
*	Bug fix for datastream encoding in non-Latin locales.
* Bug fix for 10 second delay on profile start introduced in 1.4.2019.0616
* Special Guest submission from Jabbers.... Control Center size independent of the Windows scaling value - Let us know what you think 

### 1.4.2019.0616 Release
----------------------------
* Fixed bug stopping both Profile Editor and Control Center not running at the same time
* Extended the 0611 change log.
* Changed the Helios Update process and version checking
* Reduced the nagging frequency when there is a new release available
* Fix for Control Center window positioning not working.

### 1.4.2019.0611 Release
----------------------------
* Original Helios User Guide refreshed and included
* GNU LESSER GENERAL PUBLIC LICENSE file added to the distribution
* Returned missing BMS F-16 images
* New control - Potentiometer with Translate and rotate. (Rasmaninoff)
* New custom gauge providing standard way to create a new suction gauge with only one needle (Capt Zeen)
* Rectangle Fill control used to create instruments like the Su25 Fuel bars and Mi-8 doppler bars (Capt Zeen)
* Kneeboard control added (Capt Zeen)
* Black Shark Interface changes(Capt Zeen) 
	- Corrected Device numbers: PPK, ILLUMINATION INTERFACE, SIGNAL_FLARE_DISPENSER, STBY_ADI,  PSHK7, ZMS_3, K041
	- Added ACCELEROMETER device 61
	- Added CLOCK device 29
	- modified DH/DT device to NAV_SYSTEMS
	- modified Abris power swith from 1 0 to 1.0 0.0
	- Added plafond switch
	- Added lightimg auxiliar panel switch
	- Swapped values in INU fixating method swith from 1 to 0
	- Change device to the Datalink Power switch to DATALAINK
	- Corrected Pitot AOA switch
	- Corrected Pitot Hat RAM switch
	- Swapped values in Engine rotor anti-ice switch from 1 to 0
	- Added Baro QFE Knob encoder
	- Added lighting back panel switch
	- added lighting NAVG switch
	- added lighting main panel switch
	- Added lighting ADI HSI switch
	- Added UV26 Messages display
	- Added PVI message digits
	- Added EKRAN messages displays
	- Modified and corrected the Magnetic variation function
	- Modified device on Accelerometer Reset button to ACCELEROMETER and BUTTON_1
	- Corrected AC/DC Inverter switch from 01.f to 0.1f
	- Corrected fire extinguisher work/off/test guard values to float
	- Corrected EEG Power Turbine Test guard to floats
	- Magnetic variatoin selection btn_1 is a rotary encoder now
	- Latitude Correction btn_1 is a rotary encoder now
	- Added test backup adi button
	- Added Backup ADI Longitudinal Deviation Bar
	- Added Backup ADI Lateral Deviation Bar
	- Added ambient temperature
	- Corrected UV26 power guard and Test guard buttons numbers
	- Corrected System Bit selector to rotaryswitch
* Configurable reflectivity for front Glass of IFEI (derammo)
* Control Center Preference added to stop secondary mouse events that some touch screens generate afrter a touch event (derammo)
* Update Phidgets to version Phidget21 2.1.9 Build date Feb 15 2018 (KiwiLostInMelb)
* Fixed Phidgits crashing on startup and closure (KiwiLostInMelb)
* Fixed Phidgit's to handle out of range acceleration and travel entries entered in the Profile Editor (KiwiLostInMelb)
* Added events for startup, connect aircraft and shutdown - used for re-centering real guages before shutdown to a known state - needed with Phidgits interface as it doesnt remember state across restarts - eliminates broken needle issues. (KiwiLostInMelb)
* Added Key Press Receiver program (Runs on PC which is running DCS) to issue key presses on DCS machine served from Control Center running on a different PC (KiwiLostInMelb)   
* Added ability to send key presses to a different PC using TCP on port 9088 (KiwiLostInMelb)
* The usual minor bug changes

### 1.4.2019.0317 Release
----------------------------
* TextDisplay Control Added
* Hornet UFC updated to remove the need for a viewport
* Hornet IFEI updated to remove the need for a viewport
* Auto-binding for some devices to their intended interface
* MS33558 Font incorporated (c) Derek Higgs 2000 which some sites claim is postcardware but I was unable to find an address / email
* Seven Segment font incorporated (free for personal use) http://www.kraftilab.com/portfolio/7-segment-display-font/
* Reimplement the version checking for Control Center using HTTPS
* Minor bug fixes
* Distributing Iris Screen Exporter for people who run Control Center remotely (it is in the Helios directory in My Documents)
* Code corrected to identify destination directory for DCS, Betas and Alphas 
* Stricter version numbering which breaks the installer's version checking so previous Helios needs to be uninstalled manually.

### 1.4.2018.1008 Release
----------------------------
* F/A-18C Interface
* DCS Generic interface added.  This is an enabler for complex aircraft profiles from authors such as Capt Zeen.
* Prototype "Simple" interfaces for the Mirage 2000 and Mi-8
* Guard Switch (without Toggle) added to tool box controls
* A-10 NMSP and UFC Toolbox contents moved into "A-10"
* Mi-8 ADI gauge added

### 1.4.2018.0924 Beta 3 Release
----------------------------
* AV-8B Interface
* MiG-21Bis Partial Interface by Cylution
* Profile Editor zoom funtionality restored
* Various Profile Editor bug fixes
* New file extension for profile layout

### 1.4 Un-released (Gadroc Open Source)
------------------------------------
* Switch to NuGet for dependency management
* Switch to open source license (GPLv3)
* Upgrade to AvalonDock 2
* Remove all traces of DRM
* Fix A-10C Master Caution Template
* Merge all "Gauge Packs" into main Helios assembly
* Upgrade Lua version

TODO
* Switch from Old Managed DirectX so SharpDX
* Move images out of assembly into filesystem
* Split Helios Assembly into Helios and HeliosUI (Move all dependencies on WPF to HeliosUI)
* Fix save/load crash bug on EOS servos
* Merge all DCS export.lua into one
* Fix value interpretation on lua expressions (Frank bug)
* Add exe scanning for CMSC, CMSP, Radio and PUI-800 (from forums)

### 1.3 Release - Build 191
-----------------------
* Fix crash when renaming analog inputs on EOS Devices
* Fix intermitant crash when trying to send data to EOS bus

### 1.3 Release - Build 190
-----------------------
* Attempt to fix always on top with DCS World 1.2.4

### 1.3 Release - Build 189
-----------------------
* Added "Autopilot - On Indicator" to BMS interface
* Fixed Phidgets Servo to properly configure and save servo type
* Update to EOS Bus v2
* Updated Phidgets.dlls
* Added Servo, Stepper and LED Output support to EOS bus
* Updated to properly detect DCS World 1.2.4
* Removed last references to SCSimulations (Install folder, Start menu folders)
* Removed last of license management code
* Fixed phidgets.dll in installer
* Fixed input calibaration save/load when output values are in reverse order
* Fixed setup for phidgets servo's using userdefined settings

### 1.2 Release - Build 176
-----------------------
* EOS Interface Enhancements
* Profile Editor: Added uncaught exception handling
* Control Center: Added uncaught exceptoin handling
* Increased EOS Direct scanning speed
* EOS Interface Editor: Added board active indicator

### 1.2 Release - Build 175
-----------------------
* Added EOS Interface Module

Changes
* Resolved bindings not showing up on Phidgets Servo Board

### 1.2 Release - Build 173
-----------------------
Changes
* Composite Controls (MFDs): Input bindings now load properly from a saved profile.

### 1.2 Release - Build 172
-----------------------
Changes
* A-10C Gauge Pack: Fixed colors on ADI ball
* A-10C Gauge Pack: Added Backup ADI
* Profile Editor: Moving controls up and down in the display order now reflects immediately
* General: Added ability to toggle "always on top" behavior on monitor properties
* Phidgets Servo Board: Fixed crash on exit if servo poperties where open
* Phidgets Servo Board: Fixed crash when servo was set outside it's limits.
* Profile Editor: Prevent names from having control characters added to them.
* Profile Editor: Add tool tip explaining validation errors.

Known Issues
* Old profiles may contain names with periods which cause bindings to be lost on save/load.
* DCS Black Shark Interace: VHF-1 & VHF-2 Power switches do not work as input bindings (bug in BlackShark 1.0.2 not Helios)

### 1.2 Release - Build 168
-----------------------
* Controls: Added Guarded Toggle Switches
* Profile Editor: Added ability for all controls to be rotated
* Profile Editor: Depricated Orientation setting on Toggle Switches (Existing profiles will load properly but new controls should use rotation)
* Profile Editor: Removed all "horizontal" control templates in favor of using rotation
* Profile Editor: Fixed crash when you create a zero width or height control
* Installer: Modified installer to check for both .Net 2.0 and Managed Direct X
* DCS Balck Shark Interface: Fixed EKRAN text extraction
* Profile Editor: Misc corrections in help text
* DCS FC2 Interface: Added support for 3rd Party scripts
* DCS Black Shark Interface: Fixed HSI rotary encoders (may require rebinding in your profile).
* DCS Black Shark Interface: Added fuel meter power switch
* DCS Black Shark Interface: Added Shkval Wiper Switch
* DCS Black Shark Interface: Added Anti-Collision Beacon switch
* DCS Black Shark Interface: Added SAI power switch
* DCS Black Shark Interface: Added rotor tip lights switch
* DCS Black Shark Interface: Added formation lights switch
* DCS Black Shark Interface: Reversed switch positin on laser range finder. (Require binding adjustment for existing profiles)
* DCS Black Shark Interface: Added fire extinghuiser controls
* DCS Black Shark Interface: Fixed AC/DC Inverter switch to be three way switch
* DCS Interfaces: Fixed guard on three way switches
* DCS Black Shark Interface: Added all indicators for PVI-800 Control Buttons
* DCS Black Shark Interface: Added LWS Power Switch, Self Test Button and ready lamp
* DCS Black Shark Interface: Added UV-26 power and self test switches
* DCS Black Shark Interface: Added VMS BIT Button
* DCS Black Shark Interface: Added PPK-800 System Prep panel switches
* DCS Black Shark Interface: Added INU Power/Heat Switches
* DCS Black Shark Interface: Added EKRAN/Hydraulics Power Switch
* DCS Black Shark Interface: Added Engine Power Indicator Export
* DCS Black Shark Interface: Added Engine/Gearbox Oil and Temperature data
* DCS Black Shark Interface: Added swing load auto / manual switch
* DCS Black Shark Interface: Added Gearbox oil reduction switch
* DCS Black Shark Interface: Added R-800 VHF-2 Test Button
* DCS Black Shark Interface: Added Magnentic Variation and Latitude Entry Controls
* DCS Black Shark Interface: Fixed Datalink ID and Master Mode Switches
* Falcon Interface: Added browse button for key file
* Falcon Interface: Fixed so callback list refreshes after changing keyfile
* Falcon Interface: Added BMS to falcon type selection
* Control Center: Removed Transparency from main window
* Control Center: Removed Transaprency from windows if background color is set.
* Profile Editor: Added ability to hide all controls by default
* Profile Editor: Toolbox Icons no longer respond to clicks.
* DCS A-10C Interface: Fixed ILS Volume and Khz frequency knob issues
* Profile Editor: Fixed arrow key movement bounds for rotated panels and controls
* Profile Editor: Fixed paste bug on checking bounds for rotated panels and controls
* Profile Editor: Removed annoying rectangle in profile explorer
* Four Way Hat Switch: Fixed hit boxes when switch is scaled.
* Profile Interface: Added action to launch an external application.
* Control Center: Added minize button, made power switch close application.
* Profile Editor: Fixed scaling of rotated panels and contorls
* Profile Editor: Fixed scaling of grouped items by corners other than bottom left
* Profile Editor: Fixed Profile Preview bug where it would mis-display monitor rectangles in some scenarios
* Profile Editor: Added reset monitors interface to allow easy switching of monitors including scaling
* Falcon Interface: Fixed light bit export for BMS 4.3.2
* DCS Interface (A-10C / Black Shark): Default axis/level items to increase acuracy of data.
* KA Gauge Pack: Fixed radar altimeter needle.
* Phidgets Interface: Added full support for Phidget Stepper controllers
* Phidgets Interface: Added beta support for Phidget Servo controllers (untested)
* Falcon Interface: Fixed nozzle position in BMS
* Falcon Interface: Fixed ftit output in BMS
* Falcon Interface: Fixed Indicated Altitude in BMS
* Control Center: Removed white border
* Profile Editor: Fixed controls not redrawing in editor
* Control Center: Added do not display again for Aero warning
* Profile Editor: Add working indicator to monitor reset
* DCS Intefaces (A-10C, KA-50, FC2): Removed log file from export.lua
* DCS A-10C Interface: Fixed UHF Radio Bindings
* DCS A-10C Interface: Added UHF Radio cover bindings
* DCS Black Shark Interface: Added support for Black Shark 2

### 1.1 Release - Build 146
-----------------------
* Fixes loading of profiles which have multiple DirectX interfaces of the same type

### 1.1 Release - Build 143
-----------------------
* DCS A-10C Interface: Fixed TACAN tens dial
* DCS A-10C Interface: Fixed Fire Bleed Air Detect button
* Profile Editor: Fixed crash bug when removing interface while it's open in the editor
* Profile Editor: Fixed error preventing multiple directx controllers with the same name from being available

### 1.1 Release - Build 142
-----------------------
* Profile Editor: Fixed Rotary Encoder to rotate when changing initial position
* Helios Control Center: Added addition log message at debug level
* DCS A-10C Interface: Fixed parsing error on non-english systems

### 1.1 Release - Build 140
-----------------------
* Profile Editor: Fixed bug where wrong data was saved in layout
* Profile Editor: Fixed crash on reload bug when invalid name is saved in layout
* DCS Interfaces: Updated Export lua to support broadcast addresses (untested)
* DCS A-10C Interface: Fixed ILS Window readout
* DCS A-10C Interface: Added IFF Reply and Test lamp 
* DCS A-10C Interface: Fixed Emergency Flood switch spelling (will require rebinding for this control)
* DCS Black Shark Interface: Fixed HSI range export
* DCS Black Shark Interface: Added PUI Station lamps to export
* Fixed bug on profile load preventing some profiles from reloading


### 1.1 Release - Build 133
-----------------------
* DCS Black Shark, A-10C, FC2 Interface: Added ability to remove Helios config
* DCS Black Shark, A-10C, FC2 Interface: Added ability manually set game path
* DCS Black Shark Interface: Added PUI-800 Station and Ammo displays
* Control Center: Added Donate Now button.
* Profile Editor: Added Donate Now menu item.
* Control Center: Added automatic version checking on startup.

### 1.1 Release - Build 132
-----------------------
* KA-50 Gauge Pack: Accelerometer
* KA-50 Gauge Pack: Radar Altimeter
* DCS Black Shark Interface: Added clock data triggers
* KA-50 Gauge Pack: Clock
* Fixed Hidden panel controls not properly redrawing when shown
* KA-50 Gauge Pack: EGT
* DCS A-10C Interface: Added PhantomMonitorFix
* Removed 10 minute time limit
* Fixed crash bug on cut & paste
* Added Phidgets Advanced LED board interface
* KA-50 Gauge Pack: ADI
* KA-50 Gauge Pack: Tachometer
* KA-50 Gauge Pack: Fuel Gauge
* DCS Black Shark Interface: Fixed tank fuel qty exports
* Phidgets Advanced LED: Fixed set brightness to automatically turn on led

### 1.1 Release - Build 124
-----------------------
* KA-50 Gauge Pack: Completed HSI
* DCS Black Shark Interface: Fixed HSI Commaned Course and heading exports
* KA-50 Gauge Pack: Blade Angle
* Fixed template manager causing Helios to crash
* Updated shortcut creation in installer
* Added KA-50 Gauge Pack to installer
* DCS Black Shark Interface: Added Commanded Altititude to Baro Altimeter output
* KA-50 Gauge Pack: Barometric Altimeter
* Added VC 2010 Runtime to installer
* Modified installer to not register Development version for file extensions
* KA-50 Gauge Pack: Completed Rotor RPM
* Added warning dialog for disabled Aero glass
* KA-50 Gauge Pack: Completed IAS

### 1.0 Release - Build 120
------------------------
* Resolved new installer issues causing crash on start for some machines
* Installer changes to use regular shortcuts
* Additional fixes for nested panels and TouchKit
* Resovled text scaling while zooming
* Resovled F16 MFD corrupting profiles
* Resolved touckit issues with hidden panels
* Resolved a cut & paste crash bug when cut controls had bindings to it's parent panel
* Fixed Falcon Allied Forces export bug which prevented data export
* Added RWR Bezel for A10
* Fixed SAI pitch inversion

### 1.0 Release - Build 113 - 3/22/2011
-----------------------------------
* Fix for installer error causing Profile Editor to crash

### 1.0 Release - Build 112 - 3/21/2011
-----------------------------------
* DCS A-10C Interface: Reversed SAI Bank direction
* Fixed TouchKit support broken in build 111

### 1.0 Release - Build 111 - 3/21/2011
-----------------------------------
* Optimized for fewer screen redraws
* Added configuration option to adjust number of updates per second from DCS based simulations.
* DCS A-10C Gauge Pack: Resovled stuck key issue with MFDs
* Added full multi-touch capability with mutli-touch capabile touch screeen you can press and hold mutliple buttons or switch more than one switch.
* Fixed panel rotation so it works on first profile start
* Changed the way rotary switches and encoders are scaled when not uniformly scaled
* DCS A-10C Interface: Added Auxiliary Landing Gear bindings
* DCS A-10C Interface: Added Seat Arm Handle bindings
* New installer
* Consolidated all gauge packs into main Helios install.
* Small performance improvment in packet parsing
* Changed rendering loop for better performance and less laggy needles
* DCS Black Shark Interface: Fixed duplicate control issues with ARK-22 Antenna/Mode switches
* DCS Black Shark Interface: Fixed duplicate control issues with PVI Datalink Power Switch and PVI Brightness axis
* Fixed Helios so it will return itself to the top most windows every 3 seconds.
* Fixed lua script values becoming strings instead of numeric - No really I think I really fixed it this time

### 1.0 Release - Build 103 - 3/9/2011
----------------------------------
* Fixed DCS switches to properly pass values as numbers instead of string for lua functions
* Added additional error trapping for DCS configuration
* Add additional error trapping around Add Interface dialog
* Disabled Add button on Add Interface dialog until an interface is selected
* DCS A-10C Interface: Fixed engine temperature to be more accurate
* DCS A-10C Interface: Fixed altimeter acurracy around rolling thousands altitude
* Digital Counters for gauges now properly roll digits so they are easier to read for multiples of ten being displayed
* DCS A-10C Interface: Added SAI pitch trim
* DCS Black Shark Interface: Fixed indicator push buttons to work correctly

### Experimental Add
* DCS Black Shark: Added UV-26 CMD Panel
* DCS Black Shark: Added PVI-800 Control Panel
* DCS Black Shark: Added PVTz-800 Data Link Panel
* DCS Black Shark: Added Autopilot panel
* DCS Black Shark: Added ADF ARK-22 panel
* DCS Black Shark: Added R828 Radio Panel
* DCS Black Shark: Added Signal Flare Panel
* DCS Black Shark: Added R-800 Radio Panel
* DCS Black Shark: Added Targeting Mode Control Panel
* DCS Black Shark: Added Engine Start-Up Panel
* DCS Black Shark: Added Intercom Panel
* DCS Black Shark: Added APU Panel
* DCS Black Shark: Added Electrical Panel
* DCS Black Shark: Added Fuel System Panel
* DCS Black Shark: Added Comm Power panel

### 1.0 Release Candidate 4 - 2/28/2011
-----------------------
Bug Fixes and Changes
* Fixed logging bugs causing crashes
* Changed default log level to Warning instead of Debug
* Added command line flags to disable touchkit integration (-t|--NoTouchKit)
* Added command line flags for log level (-l|--LogLevel [None | Error | Warning | Info | Debug])
* Fixed crash when clicking start when no profiles where available
* DCS A-10C Interface: Moved 3rd Party Script to end of Export.lua
* Added window triggers to all VHF Radio dials so you no longer have to format them in LUA Script
* Fixed DCS A-10C radio rotaries for final DCS A-10C release
* Fixed OpenFalcon crash when using shared textures
* Fixed background fill when running profile
* A-10C Gauge Pack: Fixed ADI Pitch Steering Bar and Glide Slope Indicator direction
* A-10C Gauge Pack: Fixed ADI Glide Scode Indicator scale
* A-10C Gauge Pack: Fixed Engine RPM needles
* Improved debug logging.
* DCS A-10C Interface: Added SAI value exports
* DCS A-10C, BS and FC2: Set export back to max of 30fps

### Experimental Adds
* DCS Black Shark: Added Magnetic Compass triggers/actions
* DCS Black Shark: Added Overhead Panel triggers/actions
* DCS Black Shark: Added Landing Gear Panel triggers/actions
* DCS Black Shark: Added Datalink panel triggers/actions
* DCS Black Shark: Added Laser Warning Receiver triggers/actions

### 1.0 Releaase Candidate 2 - 1/28/2011
* Settings and preferences are now store in My Documents Helios, so they will not be lost during upgrades
* Fixed Cut and Paste display order issues
* Fixed so you can't drag a control with negative top or left co-ordinates
* Fixed crash when openening second instance of Helios Control Center
* Fixed saving of panel rotation
* Fixed saving of monitor background fill
* Fixed reset monitors to not put panels outside monitor border
* DCS A-10C Interface: Added Secure Voice Panel, IFF Panel, Emergency Flight Control Panel, Intercom Panel
* DCS A-10C Interface: Emergency Flood switch is fixed
* DCS A-10C Interface: Fixed UHF Mode / Frequency Selector bindings
