using System.Collections.Generic;
using System.Threading;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain
{
    public interface INomenclatureFixedPriceController
	{
        bool ContainsFixedPrice(Counterparty counterparty, Nomenclature nomenclature, decimal bottlesCount);
        bool ContainsFixedPrice(DeliveryPoint deliveryPoint, Nomenclature nomenclature, decimal bottlesCount);

		void AddFixedPrice(
			DeliveryPoint deliveryPoint,
			Nomenclature nomenclature,
			decimal fixedPrice = 0,
			int minCount = 0);

		void AddFixedPrice(
			Counterparty counterparty,
			Nomenclature nomenclature,
			decimal fixedPrice = 0,
			int minCount = 0);

		void UpdateFixedPrice(NomenclatureFixedPrice nomenclatureFixedPrice, decimal fixedPrice = 0, int minCount = 0);
		void DeleteFixedPrice(DeliveryPoint deliveryPoint, NomenclatureFixedPrice nomenclatureFixedPrice);
		void DeleteFixedPrice(Counterparty counterparty, NomenclatureFixedPrice nomenclatureFixedPrice);
		void DeleteAllFixedPricesFromCounterpartyAndDeliveryPoints(Counterparty counterparty);
		void AddEmployeeFixedPricesToCounterpartyAndDeliveryPoints(
			Counterparty counterparty, IEnumerable<NomenclatureFixedPrice> employeeFixedPrices);
		void UpdateAllEmployeeFixedPrices(
			IUnitOfWork uow,
			IEnumerable<NomenclatureFixedPrice> updatedEmployeeFixedPrices,
			IEnumerable<NomenclatureFixedPrice> deletedEmployeeFixedPrices,
			CancellationToken cancellationToken);
		IEnumerable<NomenclatureFixedPrice> GetEmployeesNomenclatureFixedPrices(IUnitOfWork uow);
	}
}
