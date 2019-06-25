using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Repositories
{
	public static class FuelRepository
	{
		public static FuelType GetDefaultFuel(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<FuelType>()
					  .Where(x => x.Name == "АИ-92")
					  .Take(1)
					  .SingleOrDefault();
		}
	}
}
