CREATE TABLE [mon].[Log](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[Received] [datetime] NOT NULL,
	[Level] [tinyint] NOT NULL,
	[Severity] [tinyint] NOT NULL,
	[SourceID] [smallint] NOT NULL,
	[InstanceID] [int] NULL,
	[Format] [tinyint] NULL,
	[Body] [nvarchar](max) NULL,
	[Tags] [nvarchar](256) NULL,
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
	[Name] [nchar](256) NOT NULL,
	[Description] [nchar](256) NULL,
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
VAlUES ('IncomingConnectionString', '[YOUR SERVICE BUS CONNECTION STRING]')

INSERT INTO mon.Settings (Name, Value)
VAlUES ('IncomingQueue', '[SERVICE BUS QUEUE]')

INSERT INTO mon.Settings (Name, Value)
VAlUES ('OutcomingConnectionString', '[YOUR SERVICE BUS CONNECTION STRING]')

INSERT INTO mon.Settings (Name, Value)
VAlUES ('OutcomingQueue', '[SERVICE BUS QUEUE]')

CREATE TABLE [mon].[KeepAlive](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Created] [datetime] NOT NULL,
	[Received] [datetime] NOT NULL,
	[SourceID] [smallint] NOT NULL,
	[InstanceID] [int] NULL,
 CONSTRAINT [PK_KeepAlive] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH 
(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)