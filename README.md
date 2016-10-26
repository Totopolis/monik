# Monik
Backend and client libraries to collect and process messages: logs, performance counters and keep-alive statuses. 

## Setup backend (Azure Cloud Service)
1. Prepare Service Bus namespace and queue
2. Prepare SQL Database and execute src/common/db.sql
3. Fill ServiceConfiguration files in MonikCloud project
4. Deploy service

## Client Use:
1. Add new nuget package source: https://www.myget.org/F/totopolis/
2. Install last package Monik.Client.Azure to your project
3. Sample:
```csharp
// Initialize
var azureSender = new AzureSender("[Service Bus connection string]", "[Queue name]");
M.Initialize(azureSender, "[Source name]", "[Source instance]");

// Send message
M.SecurityInfo("User John log in");
M.ApplicationError("Some error in application");
M.LogicInfo("{0} completed, processid={1}", "MyProcess", 1250);

// Enable auto Keep-Alive (per 30 sec)
M.MainInstance.AutoKeepAliveInterval = 30;
M.MainInstance.AutoKeepAlive = true;

// Profile time of your code
var th = TimingHelper.Create();
// some code1
th.EndAndLog("something1");

th.Begin();
// some code2
th.EndAndLog("something2");
```
## Methodology (todo):
1. SecurityInfo: user XX log-in or log-out
2. SecurityWarning: user XX bad password
3. SecurityError: domain not accessible
4. ApplicationInfo: 
5. ApplicationWarning: service start or shutdown
6. ApplicationError: any exceptions
7. LogicInfo: something calculated
8. LogicWarning: user request's bad params (for the service)
