using QSOrmProject;
using Vodovoz.Domain;
using NHibernate.Criterion;

namespace Vodovoz.Repository
{
	public static class NomenclatureRepository
	{
		public static Nomenclature GetDefaultBottle (IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature> (32);
		}

		public static QueryOver<Nomenclature> NomenclatureForProductMaterialsQuery ()
		{
			return QueryOver.Of<Nomenclature> ()
				.Where (n => n.Category.IsIn (Nomenclature.GetCategoriesForProductMaterial ()));
		}

		public static QueryOver<Nomenclature> NomenclatureForSaleQuery ()
		{
			return QueryOver.Of<Nomenclature> ()
				.Where (n => n.Category.IsIn (Nomenclature.GetCategoriesForSale ()));
		}

		/// <summary>
		/// Запрос номенклатур которые можно использовать на складе
		/// </summary>
		public static QueryOver<Nomenclature> NomenclatureOfGoodsOnlyQuery ()
		{
			return QueryOver.Of<Nomenclature> ()
				.Where (n => n.Category.IsIn (Nomenclature.GetCategoriesForGoods ()));
		}

		public static Nomenclature GetBottleDeposit (IUnitOfWork uow)
		{
			return uow.GetById<Nomenclature> (40);
		}

		public static QueryOver<Nomenclature> NomenclatureOfItemsForService ()
		{
			return QueryOver.Of<Nomenclature> ()
				.Where (n => n.Category == NomenclatureCategory.equipment);
		}

		public static QueryOver<Nomenclature> NomenclatureOfPartsForService ()
		{
			return QueryOver.Of<Nomenclature> ()
				.Where (n => n.Category == NomenclatureCategory.spare_parts);
		}

		public static QueryOver<Nomenclature> NomenclatureOfServices ()
		{
			return QueryOver.Of<Nomenclature> ()
				.Where (n => n.Category == NomenclatureCategory.service);
		}
	}
}

