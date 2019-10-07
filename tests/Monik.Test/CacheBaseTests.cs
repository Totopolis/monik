using Monik.Common;
using Monik.Service;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Monik.Test
{
    [TestFixture]
    public abstract class CacheBaseTests<TCache, TEntity>
        where TCache : CacheBase<TEntity>
        where TEntity : ICacheEntity, new()
    {
        protected Mock<IRepository> Repo;
        protected Mock<ISourceInstanceCache> SourceCache;
        protected Mock<IMonik> Monik;
        protected TCache Cache;

        public abstract TCache CreateCache(IRepository repository, ISourceInstanceCache cache, IMonik monik);
        public abstract ISetup<IRepository, long> SetupRepoMaxId(Mock<IRepository> r);
        public abstract ISetup<IRepository, List<TEntity>> SetupRepoLast(Mock<IRepository> r);
        public abstract ISetup<IRepository> SetupRepoWriteEntities(Mock<IRepository> r);

        [SetUp]
        public void SetUp()
        {
            Repo = new Mock<IRepository>();
            SourceCache = new Mock<ISourceInstanceCache>();
            Monik = new Mock<IMonik>();
            Cache = CreateCache(Repo.Object, SourceCache.Object, Monik.Object);
        }

        [Test]
        public void LastLogId_WhenCreate_Zero()
        {


            Assert.AreEqual(0, Cache.LastId);
        }

        [Test]
        public void Add_AddingLog_WillIncrementID()
        {
            var log = new TEntity();

            Cache.Add(log);

            Assert.AreEqual(1, Cache.LastId);
        }

        [Test]
        public void Add_AddingLog_WillSetLastIDToLogEntity()
        {
            var log = new TEntity {ID = 0};

            Cache.Add(log);

            Assert.AreEqual(Cache.LastId, log.ID);
        }

        [Test]
        public void OnStart_Run_LoadsLastID()
        {
            SetupRepoMaxId(Repo).Returns(999);
            SetupRepoLast(Repo).Returns(new List<TEntity>());

            Cache.OnStart();

            Assert.AreEqual(999, Cache.LastId);
        }

        [Test]
        public void Flush_OnRun_WillWritePendingLogs()
        {
            IEnumerable<TEntity> writtenValues = null;
            SetupRepoWriteEntities(Repo)
                .Callback((IEnumerable<TEntity> values) => writtenValues = values);
            var log = new TEntity();
            Cache.Add(log);

            Cache.Flush();

            Assert.IsTrue(new[] { log }.SequenceEqual(writtenValues));
        }

        [Test]
        public void Flush_AfterRun_WillClearPendingLogs()
        {
            var log = new TEntity();
            Cache.Add(log);

            Cache.Flush();

            Assert.AreEqual(0, Cache.PendingAmount);
        }

        [Test]
        public void PendingAmount_Empty_ReturnsZero()
        {

            var result = Cache.PendingAmount;

            Assert.AreEqual(0, result);
        }

        [Test]
        public void PendingAmount_AfterAdd_ReturnsOne()
        {
            var log = new TEntity();
            Cache.Add(log);

            var result = Cache.PendingAmount;

            Assert.AreEqual(1, result);
        }
    }
}
