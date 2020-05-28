copy Helios_ADI.svg ..\Helios.svg
convert -background none Helios_ADI.svg -resize 100x100 ..\Images\General\HeliosIcon.png
convert -background none Helios_ADI.svg -resize 256x256 -define icon:auto-resize="256,128,96,64,48,32,16" ..\Helios.ico

copy ..\Helios.ico "..\..\Helios Installer\Graphics\Helios.ico"
copy ..\Helios.ico "..\..\Keypress Receiver Installer\Graphics\Helios.ico"

convert -background none Helios_Tools.svg -resize 100x100 "..\..\Profile Editor\Icon.png"
convert -background none Helios_Tools.svg -resize 256x256 -define icon:auto-resize="256,128,96,64,48,32,16" "..\..\Profile Editor\ProfileEditor.ico"
convert -background none Helios_ADI.svg -resize 100x100 "..\..\Control Center\Icon.png"
convert -background none Helios_ADI.svg -resize 256x256 -define icon:auto-resize="256,128,96,64,48,32,16" "..\..\Control Center\ControlCenter.ico"
convert -background none Helios_Tiny.svg -resize 100x100 "..\Images\General\HeliosSimplifiedIcon.png"
