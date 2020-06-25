using QS.DomainModel.UoW;

namespace Vodovoz.TempAdapters
{
    public interface IEntityDeleteWorker
    { 
        bool DeleteObject<TEntity>(int id, IUnitOfWork uow = null);
    }
}