using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.WageCalculation
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "параметры расчёта зарплаты по старым ставкам",
			Nominative = "параметр расчёта зарплаты по старым ставкам",
			Accusative = "параметра расчёта зарплаты по старым ставкам",
			Genitive = "параметра расчёта зарплаты по старым ставкам"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class OldRatesWageParameterItem : WageParameterItem
	{
		public override WageParameterItemTypes WageParameterItemType {
			get { return WageParameterItemTypes.OldRates; }
			set { }
		}

		public override string Title => $"{WageParameterItemType.GetEnumTitle()}";

		static OldRatesWageParameterItem()
		{
			CreateMercenariesWageRates();
			CreateOurWageRates();
		}

		public OldRatesWageParameterItem()
		{
		}

		private static List<WageRateNode> wageRatesMercenaries;
		private static List<WageRateNode> wageRatesOur;

		private static void CreateMercenariesWageRates()
		{
			const decimal bottle19 = 15;
			const decimal bottle19WOForw = 20;
			const decimal forwarderBottle19 = 5;
			const decimal address = 52;
			wageRatesMercenaries = new List<WageRateNode> {
				new WageRateNode(new DateTime(2019, 05, 14), WageRateTypes.PhoneCompensation, 2, 2, 0),
				new WageRateNode(new DateTime(2017, 09, 18), WageRateTypes.Bottle19L, 10, 15, forwarderBottle19),
				new WageRateNode(WageRateTypes.Bottle19L, bottle19, bottle19WOForw, forwarderBottle19),
				new WageRateNode(WageRateTypes.Bottle19LInBigOrder, 7, 9, 4),
				new WageRateNode(WageRateTypes.EmptyBottle19L, 5, 5, 5),
				new WageRateNode(WageRateTypes.EmptyBottle19LInBigOrder, 1, 1, 1),
				new WageRateNode(WageRateTypes.MinBottlesQtyInBigOrder, 100, 100, 100),
				new WageRateNode(WageRateTypes.Bottle6L, 2, 3, 1),
				new WageRateNode(new DateTime(2017, 09, 18), WageRateTypes.PackOfBottles600ml, 10, 15, 5),
				new WageRateNode(WageRateTypes.PackOfBottles600ml, 15, 20, 5),
				new WageRateNode(WageRateTypes.Equipment, 20, 30, 10),
				new WageRateNode(new DateTime(2019, 05, 14), WageRateTypes.Address, 50, 50, 0),
				new WageRateNode(WageRateTypes.Address, address, address, 0),
				//Расчитывается по формуле: Адрес + (Бутыль19л * 2), для экспедитора: (Бутыль19л * 2)
				new WageRateNode(WageRateTypes.ContractCancelation, address + (bottle19 * 2), address + (bottle19WOForw * 2), forwarderBottle19 * 2)
			};
		}

		private static void CreateOurWageRates()
		{
			const decimal bottle19 = 10;
			const decimal bottle19WOForw = 10;
			const decimal forwarderBottle19 = 5;
			const decimal address = 32;
			wageRatesOur = new List<WageRateNode> {
				new WageRateNode(new DateTime(2019, 05, 14), WageRateTypes.PhoneCompensation, 2, 2, 0),
				new WageRateNode(new DateTime(2017, 08, 08), WageRateTypes.Bottle19L, 7, 7, forwarderBottle19),
				new WageRateNode(WageRateTypes.Bottle19L, bottle19, bottle19WOForw, forwarderBottle19),
				new WageRateNode(WageRateTypes.Bottle19LInBigOrder, 4, 4, 4),
				new WageRateNode(WageRateTypes.EmptyBottle19L, 5, 5, 5),
				new WageRateNode(WageRateTypes.EmptyBottle19LInBigOrder, 1, 1, 1),
				new WageRateNode(WageRateTypes.MinBottlesQtyInBigOrder, 100, 100, 100),
				new WageRateNode(WageRateTypes.Bottle6L, 1.4m, 1.4m, 1),
				new WageRateNode(new DateTime(2017, 08, 08), WageRateTypes.PackOfBottles600ml, 7, 7, 5),
				new WageRateNode(WageRateTypes.PackOfBottles600ml, 10, 10, 5),
				new WageRateNode(WageRateTypes.Equipment, 14, 14, 10),
				new WageRateNode(new DateTime(2019, 05, 14), WageRateTypes.Address, 30, 30, 0),
				new WageRateNode(WageRateTypes.Address, address, address, 0),
				//Расчитывается по формуле: Адрес + (Бутыль19л * 2), для экспедитора: (Бутыль19л * 2)
				new WageRateNode(WageRateTypes.ContractCancelation, address + (bottle19 * 2), address + (bottle19WOForw * 2), forwarderBottle19 * 2)
			};
		}

		public virtual WageRate GetRateForMercenaries(DateTime date, WageRateTypes wageRateType)
		{
			return GetActualRate(wageRatesMercenaries, date, wageRateType);
		}

		public virtual WageRate GetRateForOurs(DateTime date, WageRateTypes wageRateType)
		{
			return GetActualRate(wageRatesOur, date, wageRateType);
		}

		private WageRate GetActualRate(List<WageRateNode> rateNodes, DateTime date, WageRateTypes wageRateType)
		{
			var lastestRate = rateNodes.Where(x => x.Rate.WageRateType == wageRateType)
				.First(x => x.Date == null);

			var actualRate = (rateNodes
				.Where(x => x.Rate.WageRateType == wageRateType)
				.Where(x => x.Date != null)
				.Where(x => x.Date > date)
				.OrderBy(x => x.Date)
				.FirstOrDefault() ?? lastestRate).Rate;

			return actualRate;
		}

		private class WageRateNode
		{
			public WageRateNode(DateTime date, WageRateTypes wageRateType, decimal forDriverWithForwarder, decimal forDriverWithoutForwarder, decimal forForwarder)
			{
				Date = date;
				Rate = new WageRate(wageRateType, forDriverWithForwarder, forDriverWithoutForwarder, forForwarder);
			}

			public WageRateNode(WageRateTypes wageRateType, decimal forDriverWithForwarder, decimal forDriverWithoutForwarder, decimal forForwarder)
			{
				Date = null;
				Rate = new WageRate(wageRateType, forDriverWithForwarder, forDriverWithoutForwarder, forForwarder);
			}

			public DateTime? Date { get; set; }
			public WageRate Rate { get; set; }
		}
	}
}