using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Goods
{
	public interface INomenclatureFixedPriceRepository
	{
		IEnumerable<NomenclatureFixedPrice> GetEmployeesNomenclatureFixedPrices(IUnitOfWork uow);
		IEnumerable<NomenclatureFixedPrice> GetEmployeesNomenclatureFixedPricesByCounterpartyId(IUnitOfWork uow, int counterpartyId);
		IEnumerable<NomenclatureFixedPrice> GetAllFixedPricesFromEmployeeCounterparties(IUnitOfWork uow);
		IEnumerable<NomenclatureFixedPrice> GetAllFixedPricesFromEmployeeCounterpartiesDeliveryPoints(IUnitOfWork uow);
		IEnumerable<int> GetEmployeeCounterpartiesIds(IUnitOfWork uow);
		IEnumerable<int> GetEmployeeCounterpartiesDeliveryPointsIds(IUnitOfWork uow);
		IReadOnlyList<NomenclatureFixedPrice> GetFixedPricesFor19LWater(IUnitOfWork uow);
	}
}
