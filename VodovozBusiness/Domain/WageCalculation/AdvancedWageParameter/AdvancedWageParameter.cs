using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.WageCalculation.AdvancedWageParameter
{
	[Appellative(
	Gender = GrammaticalGender.Masculine,
	NominativePlural = "дополнительные параметры расчёта зарплаты",
	Nominative = "дополнительный параметр расчёта зарплаты"
	)]
	public abstract class AdvancedWageParameter : PropertyChangedBase, IAdvancedWageParameter, IDomainObject
	{
		public int Id { get; set; }

		public AdvancedWageParameter parentParameter;
		public virtual AdvancedWageParameter ParentParameter {
			get => parentParameter;
			set { 
				SetField(ref parentParameter, value, () => ParentParameter);
				if(value != null)
					wageRateType = null;
			}
		}

		private WageRateTypes? wageRateType;
		[Display(Name = "Тип ставки для расчета зарплаты")]
		public virtual WageRateTypes? WageRateType { //Устанавливается только у корневого элемента в иерархии дополнительных параметров
			get => wageRateType;
			set {
				SetField(ref wageRateType, value);
				if(value != null)
					parentParameter = null;
			}
		}

		private decimal wage;
		[Display(Name = "Зарплата")]
		public virtual decimal Wage {
			get => wage;
			set => SetField(ref wage, value);
		}

		public abstract AdvancedWageParameterType AdvancedWageParameterType { get; }

		public abstract bool HasConflicWith(IAdvancedWageParameter advancedWageParameter);

		public abstract override string ToString();
	}
}
