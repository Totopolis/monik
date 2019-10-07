# Monik
Backend and client libraries to collect and process messages: logs, metrics (performance counters) and keep-alive statuses. 
System can operate with Azure Queue or RabbitMQ for messages exchange.

## Setup backend
1. Prepare Azure Service Bus namespace and queue
2. Prepare SQL Database and execute src/db.sql
3. Fill ServiceConfiguration files in MonikCloud project
4. Deploy service

## Client Use
1. Add new nuget package source: https://www.myget.org/F/totopolis/
2. Install last package Monik.Client.Azure to your project
3. Sample:
```csharp
////////////////////////////////
// Initialize (DI container use)
////////////////////////////////
container.Register<IMonikSender>(new AzureSender("[Service Bus connection string]", "[Queue name]"));
container.Register<IMonikSettings>(new ClientSettings()
{
    SourceName = "[Source name]",
    InstanceName = "[Instance name]",
    AutoKeepAliveEnable = true
});
container.Register<IMonik, MonikClient>().AsSingleton();

var mon = container.Resolve<IMonik>();

mon.ApplicationInfo("Hello world!");
mon.Measure("Packets", AggregationType.Accumulator, 1);
mon.Measure("DbProcedureExecTime", AggregationType.Gauge, 0.27);

/////////////////
// Old-school way
/////////////////
var sender = new AzureSender("[Service Bus connection string]", "[Queue name]");
M.Initialize(sender, "[Source name]", "[Source instance]", aAutoKeepAliveEnable: true);

// Send message
M.SecurityInfo("User John log in");
M.ApplicationError("Some error in application");
M.LogicInfo($"{processName} completed, processid={processId}");
```
## Methodology
### Metrics
0. AggregationType.Accumulator: num of packets, executes, etc...
1. AggregationType.Gauge: time of any process (db or api method execute)
### Keep-alive
Method http://monikserver/keepalive-status can be used for Zabbix/Nagios monitoring
### Logs
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
## Auth
Some server methods require a token in the Authorization header
### Create JWT token
Here is an example of token creation process with `nodejs`:
Generate a secret key
```sh
node -e "console.log(require('crypto').randomBytes(256).toString('base64'));"
```
Put the key in the app.config and deploy with the new config
```xml
<!--Authentication Secret Key-->
<add key="AuthSecretKeyBase64" value="secret-key" />
```
Create a token with a subject `sub` and an expiration timestamp `exp`, sign it with the previously generated key
```sh
node -e "console.log(require('jsonwebtoken').sign({sub:'name', exp:1550102400}, Buffer.from('secret_key', 'base64')));"
```
### Use JWT token
Send the token with requests in the Authorization header
```sh
curl -X DELETE -H 'Authorization: Bearer <token>' -i http://monikserver/instances/1
```
