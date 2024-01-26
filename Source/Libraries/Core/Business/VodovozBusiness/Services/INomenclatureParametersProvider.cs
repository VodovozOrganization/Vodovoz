using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Services
{
	public interface INomenclatureParametersProvider
	{
		int PaidDeliveryNomenclatureId { get; }
		int AdvancedPaymentNomenclatureId { get; }
		int Folder1cForOnlineStoreNomenclatures { get; }
		int MeasurementUnitForOnlineStoreNomenclatures { get; }
		int RootProductGroupForOnlineStoreNomenclatures { get; }
		int CurrentOnlineStoreId { get; }
		decimal GetWaterPriceIncrement { get; }
		string OnlineStoreExportFileUrl { get; }
		int FastDeliveryNomenclatureId { get; }
		int PromotionalNomenclatureGroupId { get; }
		int DailyCoolerRentNomenclatureId { get; }
		int[] PaidDeliveriesNomenclaturesIds();

		Nomenclature GetWaterSemiozerie(IUnitOfWork uow);
		Nomenclature GetWaterKislorodnaya(IUnitOfWork uow);
		Nomenclature GetWaterSnyatogorskaya(IUnitOfWork uow);
		Nomenclature GetWaterKislorodnayaDeluxe(IUnitOfWork uow);
		Nomenclature GetWaterStroika(IUnitOfWork uow);
		Nomenclature GetWaterRuchki(IUnitOfWork uow);
		Nomenclature GetDefaultBottleNomenclature(IUnitOfWork uow);
		Nomenclature GetNomenclatureToAddWithMaster(IUnitOfWork uow);
		int[] GetSanitisationNomenclature(IUnitOfWork uow);
		Nomenclature GetForfeitNomenclature(IUnitOfWork uow);

		int GetIdentifierOfOnlineShopGroup();
		Nomenclature GetFastDeliveryNomenclature(IUnitOfWork uow);
	}
}
