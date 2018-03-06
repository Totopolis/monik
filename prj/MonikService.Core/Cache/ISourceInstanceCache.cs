using System.Collections.Generic;

namespace Monik.Service
{
    public interface ISourceInstanceCache : IObject
    {
        Instance CheckSourceAndInstance(string aSourceName, string aInstanceName);
        Source GetSourceByInstanceId(int aInstanceId);
        Instance GetInstanceById(int aInstanceId);
        List<Instance> GetAllInstances();
        List<Source> GetAllSources();
        List<Group> GetAllGroups();

        bool IsDefaultInstance(int aInstance);
        bool IsInstanceInGroup(int aInstanceId, short aGroupId);
    }
}