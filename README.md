# Install as a service

```
sc.exe create AudioMeterEvent binPath= "C:\Path\To\AudioMeterEvent.exe service foo" obj= "NT AUTHORITY\LocalService"
```