using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Parameters;
using NewNomenclatureRepository = Vodovoz.EntityRepositories.Goods.NomenclatureRepository;

namespace Vodovoz.Repositories
{
	[Obsolete("Используйте одноимённый класс из Vodovoz.EntityRepositories.Goods")]
	public static class NomenclatureRepository
	{
		private static NewNomenclatureRepository repository = new NewNomenclatureRepository(new NomenclatureParametersProvider());

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureForProductMaterialsQuery() => repository.NomenclatureForProductMaterialsQuery();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureEquipmentsQuery() => repository.NomenclatureEquipmentsQuery();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureForSaleQuery() => repository.NomenclatureForSaleQuery();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureByCategory(NomenclatureCategory category) => repository.NomenclatureByCategory(category);

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureOfGoodsOnlyQuery() => repository.NomenclatureOfGoodsOnlyQuery();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureOfGoodsWithoutEmptyBottlesQuery() => repository.NomenclatureOfGoodsWithoutEmptyBottlesQuery();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureWaterOnlyQuery() => repository.NomenclatureWaterOnlyQuery();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureEquipOnlyQuery() => repository.NomenclatureEquipOnlyQuery();

		[Obsolete]
		public static Nomenclature GetDefaultBottle(IUnitOfWork uow) => repository.GetDefaultBottle(uow);

		[Obsolete]
		public static IList<Nomenclature> GetDependedNomenclatures(IUnitOfWork uow, Nomenclature influentialNomenclature) => repository.GetDependedNomenclatures(uow, influentialNomenclature);

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureOfItemsForService() => repository.NomenclatureOfItemsForService();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureOfPartsForService() => repository.NomenclatureOfPartsForService();

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureOfServices() => repository.NomenclatureOfServices();

		[Obsolete]
		public static IList<Nomenclature> NomenclatureOfDefectiveGoods(IUnitOfWork uow) => repository.GetNomenclatureOfDefectiveGoods(uow);

		[Obsolete]
		public static string GetNextCode1c(IUnitOfWork uow) => repository.GetNextCode1c(uow);

		[Obsolete]
		public static QueryOver<Nomenclature> NomenclatureInGroupsQuery(int[] groupsIds) => repository.NomenclatureInGroupsQuery(groupsIds);

		[Obsolete]
		public static Nomenclature GetNomenclatureToAddWithMaster(IUnitOfWork uow) => repository.GetNomenclatureToAddWithMaster(uow);

		[Obsolete]
		public static Nomenclature GetForfeitNomenclature(IUnitOfWork uow) => repository.GetForfeitNomenclature(uow);

		[Obsolete]
		public static Nomenclature GetSanitisationNomenclature(IUnitOfWork uow) => repository.GetSanitisationNomenclature(uow);

		[Obsolete]
		public static IList<Nomenclature> GetNomenclatureWithPriceForMobileApp(IUnitOfWork uow, params MobileCatalog[] catalogs) => repository.GetNomenclatureWithPriceForMobileApp(uow, catalogs);

		[Obsolete]
		public static Dictionary<Nomenclature, IList<Certificate>> GetDictionaryWithCertificatesForNomenclatures(IUnitOfWork uow, Nomenclature[] nomenclatures) => repository.GetDictionaryWithCertificatesForNomenclatures(uow, nomenclatures);

		[Obsolete]
		public static Dictionary<int, int[]> GetNomenclatureImagesIds(IUnitOfWork uow, params int[] nomenclatureIds) => repository.GetNomenclatureImagesIds(uow, nomenclatureIds);

		[Obsolete]
		public static Nomenclature GetWaterSemiozerie(IUnitOfWork uow) => repository.GetWaterSemiozerie(uow);

		[Obsolete]
		public static Nomenclature GetWaterKislorodnaya(IUnitOfWork uow) => repository.GetWaterKislorodnaya(uow);

		[Obsolete]
		public static Nomenclature GetWaterSnyatogorskaya(IUnitOfWork uow) => repository.GetWaterSnyatogorskaya(uow);

		[Obsolete]
		public static Nomenclature GetWaterKislorodnayaDeluxe(IUnitOfWork uow) => repository.GetWaterKislorodnayaDeluxe(uow);

		[Obsolete]
		public static Nomenclature GetWaterStroika(IUnitOfWork uow) => repository.GetWaterStroika(uow);

		[Obsolete]
		public static Nomenclature GetWaterRuchki(IUnitOfWork uow) => repository.GetWaterRuchki(uow);
	}
}