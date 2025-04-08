# TimeSyncService

- `sc create InternetTimeSyncService binPath= "C:\Temp\ITS\TimeSyncService.exe" start= auto`
- `sc description InternetTimeSyncService "Synchronizes system time with internet time servers every 15 minutes"`
- `sc start InternetTimeSyncService`
