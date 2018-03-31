using System;
using System.Collections.Generic;
using System.Linq;
using Monik.Client;

namespace Monik.Service
{
    public class SourceInstanceCache : ISourceInstanceCache
    {
        private readonly IRepository _repository;
        private readonly IClientControl _control;

        private readonly Dictionary<string, Source> _sources;
        private readonly Dictionary<short, Source> _sourceMap;

        private readonly Dictionary<int, Instance> _instanceMap;
        private readonly Dictionary<string, Instance> _sourceInstanceMap;

        private Dictionary<short, Group> _groups;
        private List<int> _defaultInstances; // from default group

        public SourceInstanceCache(IRepository aRepository, IClientControl aControl)
        {
            _repository = aRepository;
            _control = aControl;

            _sources = new Dictionary<string, Source>();
            _sourceMap = new Dictionary<short, Source>();
            _instanceMap = new Dictionary<int, Instance>();
            _sourceInstanceMap = new Dictionary<string, Instance>();

            _control.ApplicationVerbose("SourceInstanceCache created");
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
                    _control.ApplicationError($"Database contains more than one same source name: {src.Name}");

            // 2. Load all instances in memory
            var instances = _repository.GetAllInstances();
            foreach (var ins in instances)
                if (_sourceMap.ContainsKey(ins.SourceID))
                {
                    Source src = _sourceMap[ins.SourceID];
                    string key = $"{src.Name}*{ins.Name}";

                    if (!_sourceInstanceMap.ContainsKey(key))
                    {
                        ins.SourceRef(src);
                        _instanceMap.Add(ins.ID, ins);
                        _sourceInstanceMap.Add(key, ins);
                    }
                    else
                        _control.ApplicationError(
                            $"Database contains more than one the same instance name '{ins.Name}' for the source '{src.Name}'");
                }
                else
                    _control.ApplicationError($"Database doesnt contains source(id={ins.SourceID}) for the instance '{ins.Name}'");

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

            _control.ApplicationVerbose("SourceInstanceCache started");
        }

        public void OnStop()
        {
        }

        public bool IsDefaultInstance(int aInstance)
        {
            return _defaultInstances.Contains(aInstance);
        }

        public bool IsInstanceInGroup(int aInstanceId, short aGroupId)
        {
            if (!_groups.ContainsKey(aGroupId))
                return false;

            return _groups[aGroupId].Instances.Contains(aInstanceId);
        }

        public Source GetSourceByInstanceId(int aInstanceId)
        {
            lock (this)
            {
                return _instanceMap.ContainsKey(aInstanceId) ? _instanceMap[aInstanceId].SourceRef() : null;
            }
        }

        public Instance GetInstanceById(int aInstanceId)
        {
            lock (this)
            {
                return _instanceMap.ContainsKey(aInstanceId) ? _instanceMap[aInstanceId] : null;
            }
        }

        public List<Instance> GetAllInstances()
        {
            return _instanceMap.Values.ToList();
        }

        public Instance CheckSourceAndInstance(string aSourceName, string aInstanceName)
        {
            string key = $"{aSourceName}*{aInstanceName}";

            lock (this)
            {
                if (_sourceInstanceMap.ContainsKey(key))
                    return _sourceInstanceMap[key];

                Source src;

                if (!_sources.ContainsKey(aSourceName))
                {
                    src = new Source() { Name = aSourceName, Created = DateTime.UtcNow, DefaultGroupID = null };
                    _repository.CreateNewSource(src);

                    _sources.Add(aSourceName, src);
                    _sourceMap.Add(src.ID, src);
                }
                else
                    src = _sources[aSourceName];

                Instance ins = new Instance() { Name = aInstanceName, Created = DateTime.UtcNow, SourceID = src.ID };
                _repository.CreateNewInstance(ins);

                ins.SourceRef(src);
                _instanceMap.Add(ins.ID, ins);

                _sourceInstanceMap.Add(key, ins);

                if (src.DefaultGroupID.HasValue && _groups.ContainsKey(src.DefaultGroupID.Value))
                {
                    var gr = _groups[src.DefaultGroupID.Value];
                    gr.Instances.Add(ins.ID);
                    _repository.AddInstanceToGroup(ins, gr);
                }

                return ins;
            } // TODO: optimize lock
        }

    } //end of class
}
