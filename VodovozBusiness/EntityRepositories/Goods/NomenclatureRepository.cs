using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.EntityRepositories.Goods
{
	public class NomenclatureRepository : INomenclatureRepository {
		
		private readonly INomenclatureParametersProvider nomenclatureParametersProvider;

		public NomenclatureRepository(INomenclatureParametersProvider nomenclatureParametersProvider) {
			this.nomenclatureParametersProvider = nomenclatureParametersProvider ?? 
				throw new ArgumentNullException(nameof(nomenclatureParametersProvider));
		}
		
		public QueryOver<Nomenclature> NomenclatureForProductMaterialsQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category.IsIn(Nomenclature.GetCategoriesForProductMaterial()))
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureEquipmentsQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.equipment)
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureForSaleQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category.IsIn(Nomenclature.GetCategoriesForSale()))
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureByCategory(NomenclatureCategory category)
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == category)
							.Where(n => !n.IsArchive);
		}

		/// <summary>
		/// Запрос номенклатур которые можно использовать на складе
		/// </summary>
		public QueryOver<Nomenclature> NomenclatureOfGoodsOnlyQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category.IsIn(Nomenclature.GetCategoriesForGoods()))
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureOfGoodsWithoutEmptyBottlesQuery()
		{
			return QueryOver.Of<Nomenclature>()
							.Where(n => n.Category.IsIn(Nomenclature.GetCategoriesForGoodsWithoutEmptyBottles()))
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureWaterOnlyQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.water)
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureEquipOnlyQuery()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.equipment)
							.Where(n => !n.IsArchive);
		}

		public Nomenclature GetDefaultBottle(IUnitOfWork uow)
		{
			var defaultBottleParameter = "default_bottle_nomenclature";
			if(!ParametersProvider.Instance.ContainsParameter(defaultBottleParameter))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура бутыли по умолчанию.");
			return uow.GetById<Nomenclature>(int.Parse(ParametersProvider.Instance.GetParameterValue(defaultBottleParameter)));
		}

		/// <summary>
		/// Возвращает список номенклатур, которые зависят от передаваемой номенклатуры.
		/// </summary>
		/// <returns>Список зависимых номенклатур.</returns>
		/// <param name="uow">uow - Unit of work</param>
		/// <param name="influentialNomenclature">influentialNomenclature - вляющая номенклатура</param>
		public IList<Nomenclature> GetDependedNomenclatures(IUnitOfWork uow, Nomenclature influentialNomenclature)
		{
			return uow.Session.QueryOver<Nomenclature>()
					  .Where(n => n.DependsOnNomenclature.Id == influentialNomenclature.Id)
					  .List();
		}

		public QueryOver<Nomenclature> NomenclatureOfItemsForService()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.equipment)
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureOfPartsForService()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.spare_parts)
							.Where(n => !n.IsArchive);
		}

		public QueryOver<Nomenclature> NomenclatureOfServices()
		{
			return QueryOver.Of<Nomenclature>()
				.Where(n => n.Category == NomenclatureCategory.service)
							.Where(n => !n.IsArchive);
		}

		public IList<Nomenclature> GetNomenclatureOfDefectiveGoods(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Nomenclature>()
				.Where(n => n.IsDefectiveBottle).List();
		}

		public string GetNextCode1c(IUnitOfWork uow)
		{
			var lastCode1c = uow.Query<Nomenclature>()
								.Where(n => n.Code1c.IsLike(Nomenclature.PrefixOfCode1c, MatchMode.Start))
								.OrderBy(n => n.Code1c).Desc
								.Select(n => n.Code1c)
								.Take(1)
								.SingleOrDefault<string>();
			int id = 0;
			if(!String.IsNullOrEmpty(lastCode1c)) {
				id = int.Parse(lastCode1c.Replace(Nomenclature.PrefixOfCode1c, ""));//Тут специально падаем в эксепшен если не смогли распарсить, подума 5 раз, пережде чем заменить на TryParse
			}
			id++;
			string format = new String('0', Nomenclature.LengthOfCode1c - Nomenclature.PrefixOfCode1c.Length);
			return Nomenclature.PrefixOfCode1c + id.ToString(format);
		}

		public QueryOver<Nomenclature> NomenclatureInGroupsQuery(int[] groupsIds)
		{
			return QueryOver.Of<Nomenclature>()
							.Where(n => n.ProductGroup.Id.IsIn(groupsIds));
		}

		public Nomenclature GetNomenclatureToAddWithMaster(IUnitOfWork uow)
		{
			var followingNomenclaure = "номенклатура_для_выезда_с_мастером";
			if(!ParametersProvider.Instance.ContainsParameter(followingNomenclaure))
				throw new InvalidProgramException("В параметрах базы не указана номенклатура \"номенклатура_для_выезда_с_мастером\" для добавления в заказ типа \"Выезд мастера\"");
			return uow.GetById<Nomenclature>(int.Parse(ParametersProvider.Instance.GetParameterValue(followingNomenclaure)));
		}

		public Nomenclature GetForfeitNomenclature(IUnitOfWork uow)
		{
			var forfeitNomenclatureStr = "forfeit_nomenclature_id";
			if(!ParametersProvider.Instance.ContainsParameter(forfeitNomenclatureStr))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура для \"Бутыль (Неустойка)\"");
			return uow.GetById<Nomenclature>(int.Parse(ParametersProvider.Instance.GetParameterValue(forfeitNomenclatureStr)));
		}

		public Nomenclature GetSanitisationNomenclature(IUnitOfWork uow)
		{
			var sanitisationNomenclature = "выезд_мастера_для_сан_обр";
			if(!ParametersProvider.Instance.ContainsParameter(sanitisationNomenclature))
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура для \"Выезд мастера для с\\о\"");
			return uow.GetById<Nomenclature>(int.Parse(ParametersProvider.Instance.GetParameterValue(sanitisationNomenclature)));
		}

		public IList<Nomenclature> GetNomenclatureWithPriceForMobileApp(IUnitOfWork uow, params MobileCatalog[] catalogs)
		{
			return uow.Session.QueryOver<Nomenclature>()
							  .Where(n => !n.IsArchive)
							  .Where(n => n.MobileCatalog.IsIn(catalogs))
							  .List();
		}

		/// <summary>
		/// Возврат словаря сертификатов для передаваемых номенклатур
		/// </summary>
		/// <returns>Словарь сертификатов</returns>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="nomenclatures">Список номенклатур</param>
		public Dictionary<Nomenclature, IList<Certificate>> GetDictionaryWithCertificatesForNomenclatures(IUnitOfWork uow, Nomenclature[] nomenclatures)
		{
			Dictionary<Nomenclature, IList<Certificate>> dict = new Dictionary<Nomenclature, IList<Certificate>>();
			foreach(var n in nomenclatures) {
				Nomenclature nomenclatureAlias = null;
				var certificates = uow.Session.QueryOver<Certificate>()
									   .Left.JoinAlias(c => c.Nomenclatures, () => nomenclatureAlias)
									   .Where(() => nomenclatureAlias.Id == n.Id)
									   .List()
									   ;
				if(certificates.Any()) {
					if(!dict.ContainsKey(n))
						dict.Add(n, certificates);
					else {
						foreach(Certificate certificate in certificates)
							dict[n].Add(certificate);
					}
				}
			}

			return dict;
		}

		/// <summary>
		/// Возвращает Dictionary где: 
		/// key - id номенклатуры
		/// value - массив id картинок
		/// </summary>
		/// <returns>The nomenclature images identifiers.</returns>
		public Dictionary<int, int[]> GetNomenclatureImagesIds(IUnitOfWork uow, params int[] nomenclatureIds)
		{
			return uow.Session.QueryOver<NomenclatureImage>()
						 .Where(n => n.Nomenclature.Id.IsIn(nomenclatureIds))
						 .SelectList(list => list
						 	.Select(i => i.Id)
							.Select(i => i.Nomenclature.Id)
						 )
						 .List<object[]>()
						 .GroupBy(x => (int)x[1])
						 .ToDictionary(g => g.Key, g => g.Select(x => (int)x[0]).ToArray());
		}

		#region Получение номенклатур воды

		public Nomenclature GetWaterSemiozerie(IUnitOfWork uow) => 
			nomenclatureParametersProvider.GetWaterSemiozerie(uow);

		public Nomenclature GetWaterKislorodnaya(IUnitOfWork uow) => 
			nomenclatureParametersProvider.GetWaterKislorodnaya(uow);

		public Nomenclature GetWaterSnyatogorskaya(IUnitOfWork uow) => 
			nomenclatureParametersProvider.GetWaterSnyatogorskaya(uow);

		public Nomenclature GetWaterKislorodnayaDeluxe(IUnitOfWork uow) => 
			nomenclatureParametersProvider.GetWaterKislorodnayaDeluxe(uow);

		public Nomenclature GetWaterStroika(IUnitOfWork uow) => 
			nomenclatureParametersProvider.GetWaterStroika(uow);

		public Nomenclature GetWaterRuchki(IUnitOfWork uow) => 
			nomenclatureParametersProvider.GetWaterRuchki(uow);

		#endregion

		public decimal GetWaterPriceIncrement => nomenclatureParametersProvider.GetWaterPriceIncrement;

		public int GetIdentifierOfOnlineShopGroup()
		{
			string parameterName = "код_группы_товаров_для_интерент-магазина";
			if(!ParametersProvider.Instance.ContainsParameter(parameterName) || !int.TryParse(ParametersProvider.Instance.GetParameterValue(parameterName), out int res))
				return 0;
			return res;
		}
	}
}