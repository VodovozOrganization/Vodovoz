using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.WageCalculation
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "параметры расчёта зарплаты без зарплаты",
			Nominative = "параметр расчёта зарплаты без зарплаты",
			Accusative = "параметра расчёта зарплаты без зарплаты",
			Genitive = "параметра расчёта зарплаты без зарплаты"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class ManualWageParameterItem : WageParameterItem
	{
		public override WageParameterItemTypes WageParameterItemType {
			get { return WageParameterItemTypes.Manual; }
			set { }
		}

		public override string Title => $"{WageParameterItemType.GetEnumTitle()}";
	}
}
