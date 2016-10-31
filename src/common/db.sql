--DROP TABLE [mon].[Log]
--DROP TABLE [mon].[Source]
--DROP TABLE [mon].[Instance]
--DROP TABLE [mon].[Settings]
--DROP TABLE [mon].[KeepAlive]
--DROP TABLE [mon].[EventQueue]
--DROP TABLE [mon].[HourStat]

CREATE SCHEMA mon

CREATE TABLE [mon].[Log](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[Received] [datetime] NOT NULL,
	[Level] [tinyint] NOT NULL,
	[Severity] [tinyint] NOT NULL,
	[InstanceID] [int] NOT NULL,
	[Format] [tinyint] NOT NULL,
	[Body] [nvarchar](max) NOT NULL,
	[Tags] [nvarchar](256) NOT NULL,
CONSTRAINT [PK_Log] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH 
(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)


CREATE TABLE [mon].[Source](
	[ID] [smallint] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](256) NULL,
CONSTRAINT [PK_Source] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH 
(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)


CREATE TABLE [mon].[Instance](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[SourceID] [smallint] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](256) NULL,
 CONSTRAINT [PK_Instance] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH 
(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)


CREATE TABLE [mon].[Settings](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH 
(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

INSERT INTO mon.Settings (Name, Value)
VAlUES ('NeedUpdate', '0')

INSERT INTO mon.Settings (Name, Value)
VAlUES ('OutcomingConnectionString', '[YOUR SERVICE BUS CONNECTION STRING]')

INSERT INTO mon.Settings (Name, Value)
VAlUES ('OutcomingQueue', '[SERVICE BUS QUEUE]')

INSERT INTO mon.Settings (Name, Value)
VAlUES ('DayDeepLog', '14')

INSERT INTO mon.Settings (Name, Value)
VAlUES ('DayDeepKeepAlive', '1')



CREATE TABLE [mon].[KeepAlive](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[Received] [datetime] NOT NULL,
	[InstanceID] [int] NOT NULL,
 CONSTRAINT [PK_KeepAlive] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH 
(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)



CREATE TABLE [mon].[EventQueue](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](256) NULL,
	[Type] [tinyint] NOT NULL,
	[ConnectionString] [nvarchar](256) NOT NULL,
	[QueueName] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_EventQueue] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH 
(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

INSERT INTO mon.EventQueue (Name, Type, ConnectionString, QueueName)
VAlUES ('EventsSourceAzureQueue', 1, '[YOUR SERVICE BUS CONNECTION STRING]', '[SERVICE BUS QUEUE]')



CREATE TABLE [mon].[HourStat](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Hour] [datetime] NOT NULL,
	[LastLogID] [bigint] NOT NULL,
	[LastKeepAliveID] [bigint] NOT NULL,
 CONSTRAINT [PK_HourStat] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)


CREATE VIEW [mon].[PrettyLogs] AS
 SELECT lg.ID, src.Name as Source, ins.Name as Instance, lg.Created, [Level] =
  CASE lg.Level
	WHEN 0 THEN 'SYSTEM'
	WHEN 10 THEN 'APPLICATION'
	WHEN 20 THEN 'LOGIC'
	WHEN 30 THEN 'SECURITY'
  END,
  Severity = 
  CASE lg.Severity
	WHEN 0 THEN 'FATAL'
	WHEN 10 THEN 'ERROR'
	WHEN 20 THEN 'WARNING'
	WHEN 30 THEN 'INFO'
	WHEN 40 THEN 'VERBOSE'
  END, 
  lg.Body
  FROM mon.Log lg
  JOIN mon.Instance ins on lg.InstanceID = ins.ID
  JOIN mon.Source src on src.ID = ins.SourceID
