copy >..\postbuild.txt 2>&1 "Keypress Receiver Setup.msi" "Helios Keypress Receiver Setup.msi"
copy >>..\postbuild.txt 2>&1 setup.exe "Helios Keypress Receiver Setup.exe"
del >>..\postbuild.txt 2>&1 "Keypress Receiver Setup.msi" /q
del >>..\postbuild.txt 2>&1 "setup.exe" /q