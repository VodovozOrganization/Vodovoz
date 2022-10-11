using System.Collections.Generic;
using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface IWaterPricesRepository
	{
		List<WaterPriceNode> GetWaterPrices(IUnitOfWork uow);
		List<WaterPriceNode> GetWaterPricesHeader(IUnitOfWork uow);
		List<WaterPriceNode> GetCompleteWaterPriceTable(IUnitOfWork uow);
	}
}