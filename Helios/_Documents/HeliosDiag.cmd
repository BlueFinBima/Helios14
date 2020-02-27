@rem ******************************************************************************************
@rem ******************************************************************************************
@rem ***                                                                                    ***
@rem ***  This script saves away basic interface that might be needed to diagnose Helios    ***
@rem ***  problems.                                                                         ***
@rem ***                                                                                    ***
@rem ***  Different Windows Locales name "Saved Games" in local languages so there is an    ***
@rem ***  environment variable to name it.  There is another variable to indicate the type  ***
@rem ***  of DCS installation                                                               ***
@rem ***                                                                                    ***
@rem ***  Valid parameters are -language|-lang, -userdir, -dcstype                          ***
@rem ***                                                                                    ***
@rem ******************************************************************************************
@rem ******************************************************************************************
echo off
Echo Valid parameters are -language|-lang, -userdir, -dcstype
@rem set the defaults
set heliossavedgames=Saved Games
set heliosuserdir=%userprofile%
set heliosDCSType=DCS.openbeta
:loop
echo arg 1 - %1
IF NOT [%1]==[] (
    IF [%1]==[-language] (
        set heliossavedgames=%2
        if [%2] == [en] (set heliossavedgames=Saved Games)
        if [%2] == [es] (set heliossavedgames=Juegos guardados)
        SHIFT
    )
    IF [%1]==[-lang] (
        set heliossavedgames=%2
        if [%2] == [en] (set heliossavedgames=Saved Games)
        if [%2] == [es] (set heliossavedgames=Juegos guardados)
        SHIFT
    )
    IF [%1]==[-userdir] (
        SET heliosuserdir=%2
        SHIFT
    )
    IF [%1]==[-dcstype] (
        SET heliosDCSType=%2
        SHIFT
    )
    SHIFT
    GOTO :loop
)
set heliosDiag=%userprofile%
@echo >"%heliosDiag%\Documents\Helios\heliosDiags.txt" V1
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Diagnostics for Helios
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * %date% - %time%
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * for user %heliosuserdir%
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
@echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt"  
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Environment Variables                             * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
set >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" 
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * UDP Ports -needs elevation to get creating process* * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
netstat >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" 2>&1 -an -p UDP -b -o
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
netstat >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" 2>&1 -an -p UDP
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * DCS Scripts Directory Listing                     * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Dir >>"%heliosDiag%\Documents\Helios\heliosDiags.txt"  %heliosuserdir%
Dir >>"%heliosDiag%\Documents\Helios\heliosDiags.txt"  "%heliosuserdir%\%heliossavedgames%\%heliosDCSType%\scripts"  /s
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Export.Lua                                        * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
type >>"%heliosDiag%\Documents\Helios\heliosDiags.txt"  "%heliosuserdir%\%heliossavedgames%\%heliosDCSType%\scripts\Export.lua"
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * DCS log File                                      * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
type >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" "%heliosuserdir%\%heliossavedgames%\%heliosDCSType%\logs\dcs.log"
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Helios log File                                   * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
type >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" "%heliosuserdir%\%heliossavedgames%\%heliosDCSType%\logs\Helios.log"
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Helios Directory                                  * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
dir >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" "%heliosuserdir%\Documents\Helios\" /s
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Helios Settings File                              * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
type >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" "%heliosuserdir%\Documents\Helios\HeliosSettings.xml"
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Helios Control Center log File                    * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
type >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" "%heliosuserdir%\Documents\Helios\ControlCenter.log"
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Helios Profile Editor log File                    * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
type >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" "%heliosuserdir%\Documents\Helios\ProfileEditor.log"
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Helios Control Center log previous File           * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
type >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" "%heliosuserdir%\Documents\Helios\ControlCenter.log.bak"
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Helios Profile Editor log Previous File           * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
type >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" "%heliosuserdir%\Documents\Helios\ProfileEditor.log.bak"
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * End Diags                                         * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
set userdir=C:\Users\Default
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Diagnostics for Helios
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * %date% - %time%
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * for user %heliosuserdir%
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
@echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt"  
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * DCS Scripts Directory Listing                     * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Dir >>"%heliosDiag%\Documents\Helios\heliosDiags.txt"  %heliosuserdir%
Dir >>"%heliosDiag%\Documents\Helios\heliosDiags.txt"  "%heliosuserdir%\%heliossavedgames%\%heliosDCSType%\scripts"  /s
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Export.Lua                                        * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
type >>"%heliosDiag%\Documents\Helios\heliosDiags.txt"  "%heliosuserdir%\%heliossavedgames%\%heliosDCSType%\scripts\Export.lua"
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * DCS log File                                      * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
type >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" "%heliosuserdir%\%heliossavedgames%\%heliosDCSType%\logs\dcs.log"
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Helios log File                                   * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
type >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" "%heliosuserdir%\%heliossavedgames%\%heliosDCSType%\logs\Helios.log"
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Helios Directory                                  * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
dir >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" "%heliosuserdir%\Documents\Helios\" /s
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Helios Control Center log File                    * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
type >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" "%heliosuserdir%\Documents\Helios\ControlCenter.log"
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * Helios Profile Editor log File                    * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
type >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" "%heliosuserdir%\Documents\Helios\ProfileEditor.log"
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * End Diags                                         * * *
echo >>"%heliosDiag%\Documents\Helios\heliosDiags.txt" * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
