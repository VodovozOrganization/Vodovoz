using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class CarEventRepository : ICarEventRepository
	{
		public CarEvent GetCarEventById(IUnitOfWork uow, int id)
		{
			return uow.Session.QueryOver<CarEvent>()
					  .Where(x => x.Id == id)
					  .Take(1)
					  .SingleOrDefault();
		}
	}
}
