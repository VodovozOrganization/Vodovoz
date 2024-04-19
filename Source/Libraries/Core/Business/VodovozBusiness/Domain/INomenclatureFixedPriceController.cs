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
			IUnitOfWork uow,
			DeliveryPoint deliveryPoint,
			Nomenclature nomenclature,
			decimal fixedPrice = 0,
			int minCount = 0);

		void AddFixedPrice(
			IUnitOfWork uow,
			Counterparty counterparty,
			Nomenclature nomenclature,
			decimal fixedPrice = 0,
			int minCount = 0);

		void UpdateFixedPrice(NomenclatureFixedPrice nomenclatureFixedPrice, decimal fixedPrice = 0, int minCount = 0);
		void DeleteFixedPrice(DeliveryPoint deliveryPoint, NomenclatureFixedPrice nomenclatureFixedPrice);
		void DeleteFixedPrice(Counterparty counterparty, NomenclatureFixedPrice nomenclatureFixedPrice);
	}
}
