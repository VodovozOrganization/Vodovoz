using QS.DomainModel.UoW;
using QSOrmProject;

namespace Vodovoz.TempAdapters
{
    public class EntityDeleteWorker : IEntityDeleteWorker
    {
        public bool DeleteObject<TEntity>(int id, IUnitOfWork uow = null)
        {
            return OrmMain.DeleteObject<TEntity>(id, uow);
        }
    }
}