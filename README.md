# Monik
Backend and client libraries to collect and process messages: logs, metrics (performance counters) and keep-alive statuses. 
System can operate with Azure Queue or RabbitMQ for messages exchange.

## Setup backend
1. Prepare Azure Service Bus namespace and queue
2. Prepare SQL Database and execute src/common/db.sql
3. Fill ServiceConfiguration files in MonikCloud project
4. Deploy service

## Client Use:
1. Add new nuget package source: https://www.myget.org/F/totopolis/
2. Install last package Monik.Client.Azure to your project
3. Sample:
```csharp
// Initialize (DI container use, tinyioc)
container.Register<IMonikSender>(new AzureSender("[Service Bus connection string]", "[Queue name]"));
container.Register<IMonikSettings>(new ClientSettings()
{
    SourceName = "[Source name]",
    InstanceName = "[Instance name]",
    AutoKeepAliveEnable = true
});
container.Register<IMonik, MonikClient>().AsSingleton();

// Old-school initialize
var sender = new AzureSender("[Service Bus connection string]", "[Queue name]");
M.Initialize(sender, "[Source name]", "[Source instance]", aAutoKeepAliveEnable: true);

// Send message
M.SecurityInfo("User John log in");
M.ApplicationError("Some error in application");
M.LogicInfo($"{processName} completed, processid={processId}");
```
## Methodology:
0. SecurityVerbose:
1. SecurityInfo: user XX log-in or log-out
2. SecurityWarning: user XX bad username or password
3. SecurityError: domain or service not accessible
4. ApplicationVerbose: debug and trace
5. ApplicationInfo: 
6. ApplicationWarning: service start or shutdown
7. ApplicationError: any exceptions
8. LogicVerbose:
9. LogicInfo: something calculated
10. LogicWarning: user request's bad params (for the service)
11. LogicError: logic violation
