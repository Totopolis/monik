using System;
using System.Collections.Generic;

namespace Monik.Service
{
    public interface ICacheSourceInstance : IObject
    {
        Instance CheckSourceAndInstance(string sourceName, string instanceName);
        Source GetSourceByInstanceId(int instanceId);
        Instance GetInstanceById(int instanceId);

        List<Instance> GetAllInstances();
        List<Source> GetAllSources();
        List<Group> GetAllGroups();

        void RemoveSource(short id);
        void RemoveInstance(int id);
        event Action<IEnumerable<int>> RemoveMetrics; 

        bool IsDefaultInstance(int instance);
        bool IsInstanceInGroup(int instanceId, short groupId);

        Group CreateGroup(Group_ group);
        bool RemoveGroup(short groupId);
        void AddInstanceToGroup(int instanceId, short groupId);
        bool RemoveInstanceFromGroup(int instanceId, short groupId);
    }
}