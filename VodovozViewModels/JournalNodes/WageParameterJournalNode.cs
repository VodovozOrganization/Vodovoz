using System;
using Gamma.Utilities;
using QS.Project.Journal;
using QS.Utilities;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.JournalNodes
{
	public class WageParameterJournalNode : JournalEntityNodeBase<WageParameter>
	{
		public WageParameterJournalNode() { }

		public override string Title {
			get {
				var title = WageCalcType.GetEnumTitle();
				switch(WageCalcType) {
					case WageCalculationType.normal:
					case WageCalculationType.withoutPayment:
					case WageCalculationType.percentageForService:
						return title;
					case WageCalculationType.percentage:
						return string.Format("{0} - {1}%", title, WageCalcRate);
					case WageCalculationType.fixedRoute:
					case WageCalculationType.fixedDay:
						return string.Format("{0} - {1}", title, WageCalcRate.ToShortCurrencyString());
					case WageCalculationType.salesPlan:
						return string.Format(
							"{0} (продажа - {1} бут., забор - {2} бут.)",
							title,
							QuantityOfFullBottlesToSell,
							QuantityOfEmptyBottlesToTake
						);
					default:
						return string.Format("Неизвестный параметр №{0}", Id);
				}
			}
		}

		public string IsArchiveString => IsArchive ? "Да" : "Нет";
		public string WageCalcTypeString => WageCalcType.GetEnumTitle();
		public string WageCalcRateString {
			get {
				switch(WageCalcType) {
					case WageCalculationType.percentage:
						return string.Format("{0}%", WageCalcRate);
					case WageCalculationType.fixedRoute:
					case WageCalculationType.fixedDay:
						return WageCalcRate.ToShortCurrencyString();
					default:
						return string.Empty;
				}
			}
		}

		public string Quantities {
			get {
				if(WageCalcType == WageCalculationType.salesPlan)
					return string.Format(
						"продажа - {0} бут., забор - {1} бут.",
						QuantityOfFullBottlesToSell,
						QuantityOfEmptyBottlesToTake
					);
				return string.Empty;
			}
		}

		public string RowColor => IsArchive ? "grey" : "black";

		public WageCalculationType WageCalcType { get; set; }
		public decimal WageCalcRate { get; set; }
		public int QuantityOfFullBottlesToSell { get; set; }
		public int QuantityOfEmptyBottlesToTake { get; set; }
		public bool IsArchive { get; set; }
	}
}
