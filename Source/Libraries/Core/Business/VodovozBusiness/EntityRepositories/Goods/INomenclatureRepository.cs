using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.EntityRepositories.Nodes;
using Vodovoz.Nodes;

namespace Vodovoz.EntityRepositories.Goods
{
	public interface INomenclatureRepository
	{
		QueryOver<Nomenclature> NomenclatureForProductMaterialsQuery();
		QueryOver<Nomenclature> NomenclatureEquipmentsQuery();
		QueryOver<Nomenclature> NomenclatureForSaleQuery();
		QueryOver<Nomenclature> NomenclatureByCategory(NomenclatureCategory category);

		/// <summary>
		/// Запрос номенклатур которые можно использовать на складе
		/// </summary>
		QueryOver<Nomenclature> NomenclatureOfGoodsOnlyQuery();
		QueryOver<Nomenclature> NomenclatureOfGoodsWithoutEmptyBottlesQuery();
		QueryOver<Nomenclature> NomenclatureWaterOnlyQuery();
		QueryOver<Nomenclature> NomenclatureEquipOnlyQuery();
		Nomenclature GetDefaultBottleNomenclature(IUnitOfWork uow);

		/// <summary>
		/// Возвращает список номенклатур, которые зависят от передаваемой номенклатуры.
		/// </summary>
		/// <returns>Список зависимых номенклатур.</returns>
		/// <param name="uow">uow - Unit of work</param>
		/// <param name="influentialNomenclature">influentialNomenclature - вляющая номенклатура</param>
		IList<Nomenclature> GetDependedNomenclatures(IUnitOfWork uow, Nomenclature influentialNomenclature);
		QueryOver<Nomenclature> NomenclatureOfItemsForService();
		QueryOver<Nomenclature> NomenclatureOfPartsForService();
		QueryOver<Nomenclature> NomenclatureOfServices();
		IList<Nomenclature> GetNomenclatureOfDefectiveGoods(IUnitOfWork uow);
		string GetNextCode1c(IUnitOfWork uow);
		QueryOver<Nomenclature> NomenclatureInGroupsQuery(int[] groupsIds);
		Nomenclature GetNomenclatureToAddWithMaster(IUnitOfWork uow);
		Nomenclature GetForfeitNomenclature(IUnitOfWork uow);
		int[] GetSanitisationNomenclature(IUnitOfWork uow);

		#region Rent

		Nomenclature GetAvailableNonSerialEquipmentForRent(IUnitOfWork uow, EquipmentKind kind, IEnumerable<int> excludeNomenclatures);
		IList<NomenclatureForRentNode> GetAllNonSerialEquipmentForRent(IUnitOfWork uow, EquipmentKind kind);
		QueryOver<Nomenclature, Nomenclature> QueryAvailableNonSerialEquipmentForRent(EquipmentKind kind);

		#endregion
		
		IList<Nomenclature> GetNomenclatureWithPriceForMobileApp(IUnitOfWork uow, params MobileCatalog[] catalogs);

		/// <summary>
		/// Возврат словаря сертификатов для передаваемых номенклатур
		/// </summary>
		/// <returns>Словарь сертификатов</returns>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="nomenclatures">Список номенклатур</param>
		Dictionary<Nomenclature, IList<Certificate>> GetDictionaryWithCertificatesForNomenclatures(IUnitOfWork uow, Nomenclature[] nomenclatures);

		/// <summary>
		/// Возвращает Dictionary где: 
		/// key - id номенклатуры
		/// value - массив id картинок
		/// </summary>
		/// <returns>The nomenclature images identifiers.</returns>
		Dictionary<int, int[]> GetNomenclatureImagesIds(IUnitOfWork uow, params int[] nomenclatureIds);
		Nomenclature GetWaterSemiozerie(IUnitOfWork uow);
		Nomenclature GetWaterKislorodnaya(IUnitOfWork uow);
		Nomenclature GetWaterSnyatogorskaya(IUnitOfWork uow);
		Nomenclature GetWaterKislorodnayaDeluxe(IUnitOfWork uow);
		Nomenclature GetWaterStroika(IUnitOfWork uow);
		Nomenclature GetWaterRuchki(IUnitOfWork uow);
		/// <summary>
		/// Идентификатор для группы товаров, принадлежащей интернет-магазину
		/// </summary>
		int GetIdentifierOfOnlineShopGroup();
		decimal GetWaterPriceIncrement { get; }
		Nomenclature GetNomenclature(IUnitOfWork uow, int nomenclatureId);
		bool Has19LWater(IUnitOfWork uow, int[] nomenclaturesIds);
		IList<NomenclatureOnlineParametersNode> GetActiveNomenclaturesOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType);
		IList<NomenclatureOnlinePriceNode> GetNomenclaturesOnlinePricesByOnlineParameters(
			IUnitOfWork uow, IEnumerable<int> onlineParametersIds);
		IList<OnlineNomenclatureNode> GetNomenclaturesForSend(IUnitOfWork uow, GoodsOnlineParameterType parameterType);
		IEnumerable<INamedDomainObject> GetPromoSetsWithNomenclature(IUnitOfWork unitOfWork, int nomenclatureId, bool notArchive = true);
	}
}
