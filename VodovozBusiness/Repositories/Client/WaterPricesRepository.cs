using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSDocTemplates;
using QSOrmProject;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Repositories.Client
{
	public static class WaterPricesRepository
	{
		public static List<WaterPriceNode> GetWaterPrices(IUnitOfWork uow)
		{
			List<WaterPriceNode> result = new List<WaterPriceNode>();

			var waterPrintNom = GetPrintableWaterNomenclatures(uow);
			if(!waterPrintNom.Any()) {
				return result;
			}

			//Цены
			WaterPriceNode nodeAlias = null;
			NomenclaturePrice nomenclaturePriceAlias = null;
			Nomenclature nomenclatureAlias = null;
			var resultPrices =
			uow.Session.QueryOver<NomenclaturePrice>(() => nomenclaturePriceAlias)
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
						  ).TransformUsing(Transformers.AliasToBean<WaterPriceNode>())
			   .List<WaterPriceNode>();
			foreach(var item in resultPrices) {
				result.Add(item);
			}
			return result;
		}

		public static List<WaterPriceNode> GetWaterPricesHeader(IUnitOfWork uow)
		{
			List<WaterPriceNode> result = new List<WaterPriceNode>();

			var waterPrintNom = GetPrintableWaterNomenclatures(uow);

			//Шапка с названиями
			result.Add(new WaterPriceNode() {
				Count = "Бутыли 19 л",
				Water1 = (waterPrintNom.Count() > 0 ? waterPrintNom[0].OfficialName : ""),
				Water2 = (waterPrintNom.Count() > 1 ? waterPrintNom[1].OfficialName : ""),
				Water3 = (waterPrintNom.Count() > 2 ? waterPrintNom[2].OfficialName : "")
			});

			return result;
		}

		private static Nomenclature[] GetPrintableWaterNomenclatures(IUnitOfWork uow)
		{
			var waterNomenclatures = uow.Session.QueryOver<Nomenclature>()
										.Where(x => x.CanPrintPrice)
										.Where(x => !x.IsArchive)
										.Where(x => x.Category == NomenclatureCategory.water).List();
			return waterNomenclatures.Where(x => x.NomenclaturePrice.Any())
												  .OrderBy(x => x.NomenclaturePrice.Min(y => y.Price))
												  .Distinct()
												  .Take(3)
												  .ToArray();
		}
	}




	public class WaterPriceNode : PatternField
	{
		[Display(Name = "Количество")]
		public string Count { get; set; }
		[Display(Name = "Вода1")]
		public string Water1 { get; set; }
		[Display(Name = "Вода2")]
		public string Water2 { get; set; }
		[Display(Name = "Вода3")]
		public string Water3 { get; set; }
	}
}
