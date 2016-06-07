using QSOrmProject;
using NHibernate.Criterion;
using QSSupportLib;
using System;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Repository
{
	public static class NomenclatureRepository
	{

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
			var bottleDepositParameter = "bottleDeposit_id";
			if (!MainSupport.BaseParameters.All.ContainsKey (bottleDepositParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура залога за бутыли.");
			return uow.GetById<Nomenclature> (int.Parse(MainSupport.BaseParameters.All [bottleDepositParameter]));
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

