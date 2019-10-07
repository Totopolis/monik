using Monik.Common;
using Monik.Service;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using System.Collections.Generic;

namespace Monik.Test
{
    [TestFixture]
    public class CacheKeepAliveTests : CacheBaseTests<CacheKeepAlive, KeepAlive_>
    {
        public override CacheKeepAlive CreateCache(IRepository repository, ICacheSourceInstance cache, IMonik monik)
        {
            return new CacheKeepAlive(repository, cache, monik);
        }

        public override ISetup<IRepository, long> SetupRepoMaxId(Mock<IRepository> repo)
        {
            return repo.Setup(r => r.GetMaxKeepAliveId());
        }

        public override ISetup<IRepository, List<KeepAlive_>> SetupRepoLast(Mock<IRepository> repo)
        {
            return repo.Setup(r => r.GetLastKeepAlive(It.IsAny<int>()));
        }

        public override ISetup<IRepository> SetupRepoWriteEntities(Mock<IRepository> repo)
        {
            return repo.Setup(r => r.WriteKeepAlives(It.IsAny<IEnumerable<KeepAlive_>>()));
        }
    }
}
