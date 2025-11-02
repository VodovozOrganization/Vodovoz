using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Counterparties;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	internal sealed class WaterPricesRepository : IWaterPricesRepository
	{
		public List<WaterPriceNode> GetWaterPrices(IUnitOfWork uow)
		{
			List<WaterPriceNode> result = new List<WaterPriceNode>();

			var waterPrintNom = GetPrintableWaterNomenclatures(uow);

			if(!waterPrintNom.Any())
			{
				return result;
			}

			//Цены
			WaterPriceNode nodeAlias = null;
			NomenclaturePrice nomenclaturePriceAlias = null;
			Nomenclature nomenclatureAlias = null;
			var resultPrices =
			uow.Session.QueryOver(() => nomenclaturePriceAlias)
			   .Left.JoinAlias(() => nomenclaturePriceAlias.Nomenclature, () => nomenclatureAlias)
			   .WhereRestrictionOn(() => nomenclatureAlias.Id).IsIn(waterPrintNom.Select(x => x.Id).ToArray())
			   .SelectList(list => list
						   .SelectGroup(() => nomenclaturePriceAlias.MinCount)
						   //Количество
						   .Select(
							   Projections.Cast(
								   NHibernateUtil.AnsiString,
								   Projections.Property(() => nomenclaturePriceAlias.MinCount))
							  ).WithAlias(() => nodeAlias.Count)

						   //Цена номенклатуры 1
						   .Select(Projections.Max(
							   Projections.Conditional(
								   Restrictions.Eq(Projections.Property(() => nomenclatureAlias.Id), waterPrintNom[0].Id),
								   Projections.Cast(
									   NHibernateUtil.AnsiString,
									   Projections.Property(() => nomenclaturePriceAlias.Price)),
								   Projections.Constant("", NHibernateUtil.AnsiString)))
							  ).WithAlias(() => nodeAlias.Water1)
						   //Id номенклатуры 1
						   .Select(() => waterPrintNom[0].Id).WithAlias(() => nodeAlias.Id1)

						   //Цена номенклатуры 2
						   .Select(Projections.Max(
							   waterPrintNom.Count() > 1
							   ? Projections.Conditional(
								   Restrictions.Eq(Projections.Property(() => nomenclatureAlias.Id), waterPrintNom[1].Id),
								   Projections.Cast(
									   NHibernateUtil.AnsiString,
									   Projections.Property(() => nomenclaturePriceAlias.Price)),
								   Projections.Constant("", NHibernateUtil.AnsiString))
							   : Projections.Constant("", NHibernateUtil.AnsiString))
							  ).WithAlias(() => nodeAlias.Water2)
						   //Id номенклатуры 2
						   .Select(() => waterPrintNom.Count() > 1 ? waterPrintNom[1].Id : 0).WithAlias(() => nodeAlias.Id2)

						   //Цена номенклатуры 3
						   .Select(Projections.Max(
							   waterPrintNom.Count() > 2
							   ? Projections.Conditional(
								   Restrictions.Eq(Projections.Property(() => nomenclatureAlias.Id), waterPrintNom[2].Id),
								   Projections.Cast(
									   NHibernateUtil.AnsiString,
									   Projections.Property(() => nomenclaturePriceAlias.Price)),
								   Projections.Constant("", NHibernateUtil.AnsiString))
							   : Projections.Constant("", NHibernateUtil.AnsiString))
							  ).WithAlias(() => nodeAlias.Water3)
						   //Id номенклатуры 3
						   .Select(() => waterPrintNom.Count() > 2 ? waterPrintNom[2].Id : 0).WithAlias(() => nodeAlias.Id3)

						   //Цена номенклатуры 4
						   .Select(Projections.Max(
							   waterPrintNom.Count() > 3
							   ? Projections.Conditional(
								   Restrictions.Eq(Projections.Property(() => nomenclatureAlias.Id), waterPrintNom[3].Id),
								   Projections.Cast(
									   NHibernateUtil.AnsiString,
									   Projections.Property(() => nomenclaturePriceAlias.Price)),
								   Projections.Constant("", NHibernateUtil.AnsiString))
							   : Projections.Constant("", NHibernateUtil.AnsiString))
							  ).WithAlias(() => nodeAlias.Water4)
						   //Id номенклатуры 4
						   .Select(() => waterPrintNom.Count() > 3 ? waterPrintNom[3].Id : 0).WithAlias(() => nodeAlias.Id4)

						   //Цена номенклатуры 5
						   .Select(Projections.Max(
							   waterPrintNom.Count() > 4
							   ? Projections.Conditional(
								   Restrictions.Eq(Projections.Property(() => nomenclatureAlias.Id), waterPrintNom[4].Id),
								   Projections.Cast(
									   NHibernateUtil.AnsiString,
									   Projections.Property(() => nomenclaturePriceAlias.Price)),
								   Projections.Constant("", NHibernateUtil.AnsiString))
							   : Projections.Constant("", NHibernateUtil.AnsiString))
							  ).WithAlias(() => nodeAlias.Water5)
						   //Id номенклатуры 5
						   .Select(() => waterPrintNom.Count() > 4 ? waterPrintNom[4].Id : 0).WithAlias(() => nodeAlias.Id5)
			   ).TransformUsing(Transformers.AliasToBean<WaterPriceNode>())
			   .List<WaterPriceNode>();

			foreach(var item in resultPrices)
			{
				result.Add(item);
			}

			return result;
		}

		public List<WaterPriceNode> GetWaterPricesHeader(IUnitOfWork uow)
		{
			List<WaterPriceNode> result = new List<WaterPriceNode>();

			var waterPrintNom = GetPrintableWaterNomenclatures(uow);

			//Шапка с названиями
			result.Add(new WaterPriceNode()
			{
				StringCount = "Бутыли 19 л",
				Water1 = waterPrintNom.Any() ? waterPrintNom[0].OfficialName : "",
				Water2 = waterPrintNom.Count() > 1 ? waterPrintNom[1].OfficialName : "",
				Water3 = waterPrintNom.Count() > 2 ? waterPrintNom[2].OfficialName : "",
				Water4 = waterPrintNom.Count() > 3 ? waterPrintNom[3].OfficialName : "",
				Water5 = waterPrintNom.Count() > 4 ? waterPrintNom[4].OfficialName : ""
			});

			return result;
		}

		private Nomenclature[] GetPrintableWaterNomenclatures(IUnitOfWork uow)
		{
			var waterNomenclatures = uow.Session.QueryOver<Nomenclature>()
										.Where(x => x.CanPrintPrice)
										.Where(x => !x.IsArchive)
										.Where(x => x.Category == NomenclatureCategory.water).List();

			return waterNomenclatures.Where(x => x.NomenclaturePrice.Any())
												  .OrderBy(x => x.NomenclaturePrice.Min(y => y.Price))
												  .Distinct()
												  .Take(5)
												  .ToArray();
		}

		public List<WaterPriceNode> GetCompleteWaterPriceTable(IUnitOfWork uow)
		{
			var completeTable = GetWaterPricesHeader(uow);
			completeTable.AddRange(GetWaterPrices(uow));

			return completeTable;
		}
	}
}
