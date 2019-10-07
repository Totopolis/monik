using System;
using System.Collections.Generic;
using System.Linq;
using Monik.Common;

namespace Monik.Service
{
    public class CacheSourceInstance : ICacheSourceInstance
    {
        private static string GetSourceInstanceKey(string sourceName, string instanceName)
        {
            return $"{sourceName}*{instanceName}";
        }

        private readonly IRepository _repository;
        private readonly IMonik _monik;

        private readonly Dictionary<string, Source> _sources;
        private readonly Dictionary<short, Source> _sourceMap;

        private readonly Dictionary<int, Instance> _instanceMap;
        private readonly Dictionary<string, Instance> _sourceInstanceMap;

        private Dictionary<short, Group> _groups;
        private List<int> _defaultInstances; // from default group

        public event Action<IEnumerable<int>> RemoveMetrics;

        public CacheSourceInstance(IRepository repository, IMonik monik)
        {
            _repository = repository;
            _monik = monik;

            _sources = new Dictionary<string, Source>();
            _sourceMap = new Dictionary<short, Source>();
            _instanceMap = new Dictionary<int, Instance>();
            _sourceInstanceMap = new Dictionary<string, Instance>();

            _monik.ApplicationVerbose("CacheSourceInstance created");
        }

        public void OnStart()
        {
            // 1. Load all sources in memory
            var sources = _repository.GetAllSources();
            foreach (var src in sources)
                if (!_sources.ContainsKey(src.Name))
                {
                    _sourceMap.Add(src.ID, src);
                    _sources.Add(src.Name, src);
                }
                else
                    _monik.ApplicationError($"Database contains more than one same source name: {src.Name}");

            // 2. Load all instances in memory
            var instances = _repository.GetAllInstances();
            foreach (var ins in instances)
                if (_sourceMap.ContainsKey(ins.SourceID))
                {
                    Source src = _sourceMap[ins.SourceID];
                    string key = GetSourceInstanceKey(src.Name, ins.Name);

                    if (!_sourceInstanceMap.ContainsKey(key))
                    {
                        ins.SourceRef(src);
                        _instanceMap.Add(ins.ID, ins);
                        _sourceInstanceMap.Add(key, ins);
                    }
                    else
                        _monik.ApplicationError(
                            $"Database contains more than one the same instance name '{ins.Name}' for the source '{src.Name}'");
                }
                else
                    _monik.ApplicationError($"Database doesnt contains source(id={ins.SourceID}) for the instance '{ins.Name}'");

            // 3. Load all groups in memory
            var groups = _repository.GetAllGroupsAndFill();
            _defaultInstances = new List<int>();
            _groups = new Dictionary<short, Group>();

            foreach (var it in groups)
            {
                if (it.IsDefault)
                    foreach (var it2 in it.Instances)
                        if (!_defaultInstances.Contains(it2))
                            _defaultInstances.Add(it2);

                _groups.Add(it.ID, it);
            }

            _monik.ApplicationVerbose("CacheSourceInstance started");
        }

        public void OnStop()
        {
        }

        public void RemoveInstance(int id)
        {
            _instanceMap.TryGetValue(id, out var instance);

            // clear cache
            _defaultInstances.Remove(id);
            _instanceMap.Remove(id);
            foreach (var item in _sourceInstanceMap.Where(kvp => kvp.Value.ID == id).ToList())
                _sourceInstanceMap.Remove(item.Key);
            foreach (var kvp in _groups)
                kvp.Value.Instances.Remove(id);

            // repository
            _repository.RemoveInstance(id);

            // cleanup metrics
            if (instance != null)
                RemoveMetrics?.Invoke(instance.Metrics.Values.Select(metObj => metObj.Dto.ID).ToList());
        }

        public void RemoveSource(short id)
        {
            // clear cache
            _sourceMap.Remove(id);
            foreach (var item in _sources.Where(kvp => kvp.Value.ID == id).ToList())
                _sources.Remove(item.Key);

            // repository
            _repository.RemoveSource(id);

            // cleanup instances
            foreach (var item in _sourceInstanceMap.Where(kvp => kvp.Value.SourceID == id).ToList())
                RemoveInstance(item.Value.ID);
        }

        public bool IsDefaultInstance(int instance)
        {
            return _defaultInstances.Contains(instance);
        }

        public bool IsInstanceInGroup(int instanceId, short groupId)
        {
            if (!_groups.ContainsKey(groupId))
                return false;

            return _groups[groupId].Instances.Contains(instanceId);
        }

        public void AddInstanceToGroup(int instanceId, short groupId)
        {
            if (!_groups.ContainsKey(groupId) ||
                _groups[groupId].Instances.Contains(instanceId))
                return;

            if (_groups[groupId].IsDefault)
                _defaultInstances.Add(instanceId);

            _groups[groupId].Instances.Add(instanceId);
            _repository.AddInstanceToGroup(instanceId, groupId);
        }

        public bool RemoveInstanceFromGroup(int instanceId, short groupId)
        {
            if (!_groups.ContainsKey(groupId) ||
                !_groups[groupId].Instances.Contains(instanceId))
                return false;

            if (IsDefaultInstance(instanceId))
                _defaultInstances.Remove(instanceId);

            _groups[groupId].Instances.Remove(instanceId);
            _repository.RemoveInstanceFromGroup(instanceId, groupId);
            return true;
        }

        public Group CreateGroup(Group_ group)
        {
            var newId = _repository.CreateGroup(group);

            var newGroup = new Group
            {
                ID = newId,
                Name = group.Name,
                Description = group.Description,
                IsDefault = group.IsDefault,
                Instances = new List<int>()
            };

            _groups.Add(newGroup.ID, newGroup);
            return newGroup;
        }

        public bool RemoveGroup(short groupId)
        {
            if (!_groups.ContainsKey(groupId))
                return false;

            if (_groups[groupId].IsDefault)
                foreach (var ins in _groups[groupId].Instances)
                    _defaultInstances.Remove(ins);

            _groups.Remove(groupId);
            _repository.RemoveGroup(groupId);
            return true;
        }

        public Source GetSourceByInstanceId(int instanceId)
        {
            lock (this)
            {
                return _instanceMap.ContainsKey(instanceId) ? _instanceMap[instanceId].SourceRef() : null;
            }
        }

        public Instance GetInstanceById(int instanceId)
        {
            lock (this)
            {
                return _instanceMap.ContainsKey(instanceId) ? _instanceMap[instanceId] : null;
            }
        }

        public List<Instance> GetAllInstances()
        {
            return _instanceMap.Values.ToList();
        }

        public List<Source> GetAllSources()
        {
            return _sources.Values.ToList();
        }

        public List<Group> GetAllGroups()
        {
            return _groups.Values.ToList();
        }

        public Instance CheckSourceAndInstance(string sourceName, string instanceName)
        {
            string key = GetSourceInstanceKey(sourceName, instanceName);

            lock (this)
            {
                if (_sourceInstanceMap.ContainsKey(key))
                    return _sourceInstanceMap[key];

                Source src;

                if (!_sources.ContainsKey(sourceName))
                {
                    src = new Source() { Name = sourceName, Created = DateTime.UtcNow, DefaultGroupID = null };
                    _repository.CreateNewSource(src);

                    _sources.Add(sourceName, src);
                    _sourceMap.Add(src.ID, src);
                }
                else
                    src = _sources[sourceName];

                Instance ins = new Instance() { Name = instanceName, Created = DateTime.UtcNow, SourceID = src.ID };
                _repository.CreateNewInstance(ins);

                ins.SourceRef(src);
                _instanceMap.Add(ins.ID, ins);

                _sourceInstanceMap.Add(key, ins);

                if (src.DefaultGroupID.HasValue && _groups.ContainsKey(src.DefaultGroupID.Value))
                {
                    var gr = _groups[src.DefaultGroupID.Value];
                    gr.Instances.Add(ins.ID);
                    _repository.AddInstanceToGroup(ins.ID, gr.ID);
                }

                return ins;
            } // TODO: optimize lock
        }

    } //end of class
}
