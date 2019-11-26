\set ON_ERROR_STOP
\set ECHO all
BEGIN;
CREATE SCHEMA IF NOT EXISTS "mon";


CREATE TABLE "mon"."EventQueue"( 
	"ID" int NOT NULL,
	"Name" varchar(256) NOT NULL,
	"Description" varchar(256),
	"Type" smallint NOT NULL,
	"ConnectionString" varchar(256) NOT NULL,
	"QueueName" varchar(256) NOT NULL);

CREATE TABLE "mon"."Group"( 
	"ID" smallint NOT NULL,
	"Name" varchar(256) NOT NULL,
	"IsDefault" boolean NOT NULL,
	"Description" varchar(256));

CREATE TABLE "mon"."GroupInstance"( 
	"ID" int NOT NULL,
	"GroupID" smallint NOT NULL,
	"InstanceID" int NOT NULL);

CREATE TABLE "mon"."HourStat"( 
	"ID" int NOT NULL,
	"Hour" timestamp NOT NULL,
	"LastLogID" bigint NOT NULL,
	"LastKeepAliveID" bigint NOT NULL);

CREATE TABLE "mon"."Instance"( 
	"ID" int NOT NULL,
	"Created" timestamp NOT NULL,
	"SourceID" smallint NOT NULL,
	"Name" varchar(256) NOT NULL,
	"Description" varchar(256));

CREATE TABLE "mon"."KeepAlive"( 
	"ID" bigint NOT NULL,
	"Created" timestamp NOT NULL,
	"Received" timestamp NOT NULL,
	"InstanceID" int NOT NULL);

CREATE TABLE "mon"."Log"( 
	"ID" bigint NOT NULL,
	"Created" timestamp NOT NULL,
	"Received" timestamp NOT NULL,
	"Level" smallint NOT NULL,
	"Severity" smallint NOT NULL,
	"InstanceID" int NOT NULL,
	"Format" smallint NOT NULL,
	"Body" varchar NOT NULL,
	"Tags" varchar(256) NOT NULL);

CREATE TABLE "mon"."Measure"( 
	"ID" bigint NOT NULL,
	"Value" double precision NOT NULL);

CREATE TABLE "mon"."Metric"( 
	"ID" int NOT NULL,
	"Name" varchar(256) NOT NULL,
	"InstanceID" int NOT NULL,
	"Aggregation" int NOT NULL,
	"RangeHeadID" bigint NOT NULL,
	"RangeTailID" bigint NOT NULL,
	"ActualInterval" timestamp NOT NULL,
	"ActualID" bigint NOT NULL);

CREATE TABLE "mon"."Source"( 
	"ID" smallint NOT NULL,
	"Created" timestamp NOT NULL,
	"Name" varchar(256) NOT NULL,
	"Description" varchar(256),
	"DefaultGroupID" smallint);

COMMIT;
