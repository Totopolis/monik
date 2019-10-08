namespace Monik.Service
{
    public interface ICacheBase<in TEntity> : IObject where TEntity : ICacheEntity
    {
        void Flush();
        void Add(TEntity entity);
        int PendingAmount { get; }
        long LastId { get; }
    }
}