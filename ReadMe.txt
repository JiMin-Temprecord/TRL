ReadMe

The FTDI library only work with WinUSB

If the computer already has the Temprecord Drivers 

Zadig will have to be downloaded 
https://zadig.akeo.ie/

Open Zadig
Navigate to "Option -> List All Device" 
find "USB Reader"
Select "WinUSB" and Click "Reinstall WCID Driver"

This will conver the Temprecord Reader to a WinUSB. 

To revert back to Temprecord Reader to use COM port.
Navigate to "Device Manager -> Universal Serial Bus Device"
Right Click and Navigate to "Properties -> Driver " and Click Uninstall driver. 
Close "Device Manager" 
Unplug then re-plug the Reader. 