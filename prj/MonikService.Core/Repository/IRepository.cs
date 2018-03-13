﻿using System;
using System.Collections.Generic;

namespace Monik.Service
{
    public interface IRepository
    {
        List<Source> GetAllSources();
        List<Instance> GetAllInstances();
        List<Group> GetAllGroupsAndFill();

        void CreateNewSource(Source aSrc);
        void CreateNewInstance(Instance aIns);
        void AddInstanceToGroup(Instance aIns, Group aGroup);

        long GetMaxLogId();
        long GetMaxKeepAliveId();

        List<Log_> GetLastLogs(int aTop);
        List<KeepAlive_> GetLastKeepAlive(int aTop);

        long? GetLogThreshold(int aDayDeep);
        long? GetKeepAliveThreshold(int aDayDeep);

        int CleanUpLog(long aLastLog);
        int CleanUpKeepAlive(long aLastKeepAlive);

        void CreateHourStat(DateTime aHour, long aLastLogId, long aLastKeepAliveId);

        void CreateKeepAlive(KeepAlive_ aKeepAlive);
        void CreateLog(Log_ aLog);

        List<EventQueue> GetEventSources();

        List<MetricDescription> GetMetricDescriptions();
        void AddMetricValueStubs(int stubsCount, int metricId);
        int DeleteMetricValueStubs(int stubsCount, int metricId);

        List<MetricValue> GetAllMetricValues();
        List<MetricValue> GetMetricValues(int metricId);
    }
}