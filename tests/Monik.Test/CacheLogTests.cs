using Monik.Common;
using Monik.Service;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using System.Collections.Generic;

namespace Monik.Test
{
    [TestFixture]
    public class CacheLogTests : CacheBaseTests<CacheLog, Log_>
    {
        public override CacheLog CreateCache(IRepository repository, ICacheSourceInstance cache, IMonik monik)
        {
            return new CacheLog(repository, cache, monik);
        }

        public override ISetup<IRepository, long> SetupRepoMaxId(Mock<IRepository> repo)
        {
            return repo.Setup(r => r.GetMaxLogId());
        }

        public override ISetup<IRepository, List<Log_>> SetupRepoLast(Mock<IRepository> repo)
        {
            return repo.Setup(r => r.GetLastLogs(It.IsAny<int>()));
        }

        public override ISetup<IRepository> SetupRepoWriteEntities(Mock<IRepository> repo)
        {
            return repo.Setup(r => r.WriteLogs(It.IsAny<IEnumerable<Log_>>()));
        }
    }
}
