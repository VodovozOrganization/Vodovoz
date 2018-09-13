using QSOrmProject;
using NHibernate.Criterion;
using QSSupportLib;
using System;
using Vodovoz.Domain.Goods;
using System.Collections.Generic;

namespace Vodovoz.Repository
{
	public static class NomenclatureRepository
	{

		public static QueryOver<Nomenclature> NomenclatureForProductMaterialsQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category.IsIn(Nomenclature.GetCategoriesForProductMaterial()))
							.Where(n => !n.IsArchive);
		}

		public static QueryOver<Nomenclature> NomenclatureEquipmentsQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.equipment)
							.Where(n => !n.IsArchive);
		}

		public static QueryOver<Nomenclature> NomenclatureForSaleQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category.IsIn(Nomenclature.GetCategoriesForSale()))
							.Where(n => !n.IsArchive);
		}

		public static QueryOver<Nomenclature> NomenclatureByCategory(NomenclatureCategory category)
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == category)
							.Where(n => !n.IsArchive);
		}

		/// <summary>
		/// Запрос номенклатур которые можно использовать на складе
		/// </summary>
		public static QueryOver<Nomenclature> NomenclatureOfGoodsOnlyQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category.IsIn(Nomenclature.GetCategoriesForGoods()))
							.Where(n => !n.IsArchive);
		}

		public static QueryOver<Nomenclature> NomenclatureOfGoodsWithoutEmptyBottlesQuery()
		{
			return QueryOver.Of<Nomenclature>()
				            .Where(n => n.Category.IsIn(Nomenclature.GetCategoriesForGoodsWithoutEmptyBottles()))
							.Where(n => !n.IsArchive);
		}

		public static QueryOver<Nomenclature> NomenclatureWaterOnlyQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.water 
				       || n.Category == NomenclatureCategory.disposableBottleWater)
							.Where(n => !n.IsArchive);
		}

		public static QueryOver<Nomenclature> NomenclatureEquipOnlyQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.equipment)
							.Where(n => !n.IsArchive);
		}

		public static Nomenclature GetBottleDeposit(IUnitOfWork uow)
		{
			var bottleDepositParameter = "bottleDeposit_id";
			if(!MainSupport.BaseParameters.All.ContainsKey(bottleDepositParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура залога за бутыли.");
			return uow.GetById<Nomenclature>(int.Parse(MainSupport.BaseParameters.All[bottleDepositParameter]));
		}

		public static Nomenclature GetDefaultBottle(IUnitOfWork uow)
		{
			var defaultBottleParameter = "default_bottle_nomenclature";
			if(!MainSupport.BaseParameters.All.ContainsKey(defaultBottleParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура бутыли по умолчанию.");
			return uow.GetById<Nomenclature>(int.Parse(MainSupport.BaseParameters.All[defaultBottleParameter]));
		}

		/// <summary>
		/// Возвращает список номенклатур, которые зависят от передаваемой номенклатуры.
		/// </summary>
		/// <returns>Список зависимых номенклатур.</returns>
		/// <param name="uow">uow - Unit of work</param>
		/// <param name="influentialNomenclature">influentialNomenclature - вляющая номенклатура</param>
		public static IList<Nomenclature> GetDependedNomenclatures(IUnitOfWork uow, Nomenclature influentialNomenclature)
		{
			return uow.Session.QueryOver<Nomenclature>()
					  .Where(n => n.DependsOnNomenclature.Id == influentialNomenclature.Id)
					  .List();
		}

		public static QueryOver<Nomenclature> NomenclatureOfItemsForService()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.equipment)
							.Where(n => !n.IsArchive);
		}

		public static QueryOver<Nomenclature> NomenclatureOfPartsForService()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.spare_parts)
							.Where(n => !n.IsArchive);
		}

		public static QueryOver<Nomenclature> NomenclatureOfServices()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.service)
							.Where(n => !n.IsArchive);
		}

		public static IList<Nomenclature> NomenclatureOfDefectiveGoods(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Nomenclature>()
				.Where(n => n.IsDefectiveBottle).List();
		}

		public static string GetNextCode1c(IUnitOfWork uow)
		{
			var lastCode1c = uow.Query<Nomenclature>()
								.Where(n => n.Code1c.IsLike(Nomenclature.PrefixOfCode1c, MatchMode.Start))
								.OrderBy(n => n.Code1c).Desc
			                    .Select(n => n.Code1c)
			                    .Take(1)
			                    .SingleOrDefault<string>();
			int id = 0;
			if(!String.IsNullOrEmpty(lastCode1c))
			{
				id = int.Parse(lastCode1c.Replace(Nomenclature.PrefixOfCode1c, ""));//Тут специально падаем в эксепшен если не смогли распарсить, подума 5 раз, пережде чем заменить на TryParse
			}
			id++;
			string format = new String('0', Nomenclature.LengthOfCode1c - Nomenclature.PrefixOfCode1c.Length);
			return Nomenclature.PrefixOfCode1c + id.ToString(format);
		}

		public static QueryOver<Nomenclature> NomenclatureInGroupsQuery(int[] groupsIds)
		{
			return QueryOver.Of<Nomenclature>()
				            .Where(n => n.ProductGroup.Id.IsIn(groupsIds));
		}

		public static Nomenclature GetNomenclatureToAddWithMaster(IUnitOfWork uow)
		{
			var followingNomenclaure = "номенклатура_для_выезда_с_мастером";
			if(!MainSupport.BaseParameters.All.ContainsKey(followingNomenclaure))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура для добавления в заказа типа \"Выезд мастера\"");
			return uow.GetById<Nomenclature>(int.Parse(MainSupport.BaseParameters.All[followingNomenclaure]));
		}

		public static Nomenclature GetForfeitNomenclature(IUnitOfWork uow)
		{
			var forfeitNomenclatureStr = "forfeit_nomenclature_id";
			if(!MainSupport.BaseParameters.All.ContainsKey(forfeitNomenclatureStr))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура для \"Бутыль (Неустойка)\"");
			return uow.GetById<Nomenclature>(int.Parse(MainSupport.BaseParameters.All[forfeitNomenclatureStr]));
		}

		public static Nomenclature GetSanitisationNomenclature(IUnitOfWork uow)
		{
			var sanitisationNomenclature = "выезд_мастера_для_сан_обр";
			if(!MainSupport.BaseParameters.All.ContainsKey(sanitisationNomenclature))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура для \"Выезд мастера для с\\о\"");
			return uow.GetById<Nomenclature>(int.Parse(MainSupport.BaseParameters.All[sanitisationNomenclature]));
		}

		public static IList<Nomenclature> GetNomenclatureWithPriceForMobileApp(IUnitOfWork uow, params MobileCatalog[] catalogs)
		{
			return uow.Session.QueryOver<Nomenclature>()
			   		  .Where(n => n.MobileCatalog.IsIn(catalogs))
			   		  .List();
		}

		#region Получение номенклатур воды

		public static Nomenclature GetWaterSemiozerie(IUnitOfWork uow)
		{
			var bottleDepositParameter = "nomenclature_semiozerie_id";
			if(!MainSupport.BaseParameters.All.ContainsKey(bottleDepositParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура воды Семиозерье");
			return uow.GetById<Nomenclature>(int.Parse(MainSupport.BaseParameters.All[bottleDepositParameter]));
		}

		public static Nomenclature GetWaterKislorodnaya(IUnitOfWork uow)
		{
			var bottleDepositParameter = "nomenclature_kislorodnaya_id";
			if(!MainSupport.BaseParameters.All.ContainsKey(bottleDepositParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура воды Кислородная");
			return uow.GetById<Nomenclature>(int.Parse(MainSupport.BaseParameters.All[bottleDepositParameter]));
		}

		public static Nomenclature GetWaterSnyatogorskaya(IUnitOfWork uow)
		{
			var bottleDepositParameter = "nomenclature_snyatogorskaya_id";
			if(!MainSupport.BaseParameters.All.ContainsKey(bottleDepositParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура воды Снятогорская");
			return uow.GetById<Nomenclature>(int.Parse(MainSupport.BaseParameters.All[bottleDepositParameter]));
		}

		public static Nomenclature GetWaterStroika(IUnitOfWork uow)
		{
			var bottleDepositParameter = "nomenclature_stroika_id";
			if(!MainSupport.BaseParameters.All.ContainsKey(bottleDepositParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура воды Стройка");
			return uow.GetById<Nomenclature>(int.Parse(MainSupport.BaseParameters.All[bottleDepositParameter]));
		}

		public static Nomenclature GetWaterRuchki(IUnitOfWork uow)
		{
			var bottleDepositParameter = "nomenclature_ruchki_id";
			if(!MainSupport.BaseParameters.All.ContainsKey(bottleDepositParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура воды С ручками");
			return uow.GetById<Nomenclature>(int.Parse(MainSupport.BaseParameters.All[bottleDepositParameter]));
		}

		#endregion
	}
}