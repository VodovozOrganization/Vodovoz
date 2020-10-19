using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using NewNomenclatureRepository = Vodovoz.EntityRepositories.Goods.NomenclatureRepository;

namespace Vodovoz.Repositories
{
	[Obsolete("Используйте одноимённый класс из Vodovoz.EntityRepositories.Goods")]
	public static class NomenclatureRepository
	{
		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureForProductMaterialsQuery() => new NewNomenclatureRepository().NomenclatureForProductMaterialsQuery();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureEquipmentsQuery() => new NewNomenclatureRepository().NomenclatureEquipmentsQuery();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureForSaleQuery() => new NewNomenclatureRepository().NomenclatureForSaleQuery();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureByCategory(NomenclatureCategory category) => new NewNomenclatureRepository().NomenclatureByCategory(category);

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureOfGoodsOnlyQuery() => new NewNomenclatureRepository().NomenclatureOfGoodsOnlyQuery();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureOfGoodsWithoutEmptyBottlesQuery() => new NewNomenclatureRepository().NomenclatureOfGoodsWithoutEmptyBottlesQuery();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureWaterOnlyQuery() => new NewNomenclatureRepository().NomenclatureWaterOnlyQuery();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureEquipOnlyQuery() => new NewNomenclatureRepository().NomenclatureEquipOnlyQuery();

		[Obsolete]
		public static Nomenclature GetDefaultBottle(IUnitOfWork uow) => new NewNomenclatureRepository().GetDefaultBottle(uow);

		[Obsolete]
		public static IList<Nomenclature> GetDependedNomenclatures(IUnitOfWork uow, Nomenclature influentialNomenclature) => new NewNomenclatureRepository().GetDependedNomenclatures(uow, influentialNomenclature);

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureOfItemsForService() => new NewNomenclatureRepository().NomenclatureOfItemsForService();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureOfPartsForService() => new NewNomenclatureRepository().NomenclatureOfPartsForService();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureOfServices() => new NewNomenclatureRepository().NomenclatureOfServices();

		[Obsolete]
		public static IList<Nomenclature> NomenclatureOfDefectiveGoods(IUnitOfWork uow) => new NewNomenclatureRepository().GetNomenclatureOfDefectiveGoods(uow);

		[Obsolete]
		public static string GetNextCode1c(IUnitOfWork uow) => new NewNomenclatureRepository().GetNextCode1c(uow);

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureInGroupsQuery(int[] groupsIds) => new NewNomenclatureRepository().NomenclatureInGroupsQuery(groupsIds);

		[Obsolete]
		public static Nomenclature GetNomenclatureToAddWithMaster(IUnitOfWork uow) => new NewNomenclatureRepository().GetNomenclatureToAddWithMaster(uow);

		[Obsolete]
		public static Nomenclature GetForfeitNomenclature(IUnitOfWork uow) => new NewNomenclatureRepository().GetForfeitNomenclature(uow);

		[Obsolete]
		public static Nomenclature GetSanitisationNomenclature(IUnitOfWork uow) => new NewNomenclatureRepository().GetSanitisationNomenclature(uow);

		[Obsolete]
		public static IList<Nomenclature> GetNomenclatureWithPriceForMobileApp(IUnitOfWork uow, params MobileCatalog[] catalogs) => new NewNomenclatureRepository().GetNomenclatureWithPriceForMobileApp(uow, catalogs);

		[Obsolete]
		public static Dictionary<Nomenclature, IList<Certificate>> GetDictionaryWithCertificatesForNomenclatures(IUnitOfWork uow, Nomenclature[] nomenclatures) => new NewNomenclatureRepository().GetDictionaryWithCertificatesForNomenclatures(uow, nomenclatures);

		[Obsolete]
		public static Dictionary<int, int[]> GetNomenclatureImagesIds(IUnitOfWork uow, params int[] nomenclatureIds) => new NewNomenclatureRepository().GetNomenclatureImagesIds(uow, nomenclatureIds);

		[Obsolete]
		public static Nomenclature GetWaterSemiozerie(IUnitOfWork uow) => new NewNomenclatureRepository().GetWaterSemiozerie(uow);

		[Obsolete]
		public static Nomenclature GetWaterKislorodnaya(IUnitOfWork uow) => new NewNomenclatureRepository().GetWaterKislorodnaya(uow);

		[Obsolete]
		public static Nomenclature GetWaterSnyatogorskaya(IUnitOfWork uow) => new NewNomenclatureRepository().GetWaterSnyatogorskaya(uow);

		[Obsolete]
		public static Nomenclature GetWaterKislorodnayaDeluxe(IUnitOfWork uow) => new NewNomenclatureRepository().GetWaterKislorodnayaDeluxe(uow);

		[Obsolete]
		public static Nomenclature GetWaterStroika(IUnitOfWork uow) => new NewNomenclatureRepository().GetWaterStroika(uow);

		[Obsolete]
		public static Nomenclature GetWaterRuchki(IUnitOfWork uow) => new NewNomenclatureRepository().GetWaterRuchki(uow);
	}
}