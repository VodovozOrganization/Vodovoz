using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSDocTemplates;
using QSOrmProject;
using Vodovoz.Domain.Client;
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
				StringCount = "Бутыли 19 л",
				Water1 = (waterPrintNom.Count() > 0 ? waterPrintNom[0].OfficialName : ""),
				Water2 = (waterPrintNom.Count() > 1 ? waterPrintNom[1].OfficialName : ""),
				Water3 = (waterPrintNom.Count() > 2 ? waterPrintNom[2].OfficialName : ""),
				Water4 = (waterPrintNom.Count() > 3 ? waterPrintNom[3].OfficialName : "")
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
												  .Take(4)
												  .ToArray();
		}

		public static List<WaterPriceNode> GetCompleteWaterPriceTable(IUnitOfWork uow)
		{
			var completeTable = GetWaterPricesHeader(uow);
			completeTable.AddRange(GetWaterPrices(uow));
			return completeTable;
		}
	
		public static WaterSalesAgreement FillWaterFixedPrices (IUnitOfWork UoW, WaterSalesAgreement agreement, List<WaterSalesAgreementFixedPrice> fixedPrices)
		{
			WaterSalesAgreement result = null;
			using(var uow = UnitOfWorkFactory.CreateForRoot<WaterSalesAgreement>(agreement.Id)) {
				foreach(var fixPrice in fixedPrices) {
					var existsPrice = uow.Root.FixedPrices.FirstOrDefault(x => x.Nomenclature == fixPrice.Nomenclature);
					if(existsPrice != null) {
						existsPrice.Price = fixPrice.Price;
					}else {
						uow.Root.AddFixedPrice(fixPrice.Nomenclature, fixPrice.Price);
					}
				}
				uow.Save();
				result = UoW.GetById<WaterSalesAgreement>(uow.Root.Id);
			}
			return result;
		}
	}

	public class WaterPriceNode : PatternField
	{
		[Display(Name = "Количество")]
		public string Count { get; set; }

		string stringCount = null;
		[Display(Name = "Количество строка")]
		public string StringCount {
			get {
				return stringCount ?? String.Format("от {0} шт.", Count);
			}
			set{
				stringCount = value;
			}
		}
		[Display(Name = "Вода1")]
		public string Water1 { get; set; }
		[Display(Name = "Идентификатор1")]
		public int Id1 { get; set; }
		[Display(Name = "Вода2")]
		public string Water2 { get; set; }
		[Display(Name = "Идентификатор2")]
		public int Id2 { get; set; }
		[Display(Name = "Вода3")]
		public string Water3 { get; set; }
		[Display(Name = "Идентификатор3")]
		public int Id3 { get; set; }
		[Display(Name = "Вода4")]
		public string Water4 { get; set; }
		[Display(Name = "Идентификатор4")]
		public int Id4 { get; set; }
	}
}
