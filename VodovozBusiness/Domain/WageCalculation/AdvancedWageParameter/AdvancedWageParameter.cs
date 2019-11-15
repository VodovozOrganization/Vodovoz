using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.WageCalculation.AdvancedWageParameters
{
	[Appellative(
	Gender = GrammaticalGender.Masculine,
	NominativePlural = "дополнительные параметры расчёта зарплаты",
	Nominative = "дополнительный параметр расчёта зарплаты"
	)]
	public abstract class AdvancedWageParameter : PropertyChangedBase, IAdvancedWageParameter, IDomainObject
	{
		public virtual int Id { get; set; }

		private AdvancedWageParameter parentParameter;
		public virtual AdvancedWageParameter ParentParameter {
			get => parentParameter;
			set { 
				SetField(ref parentParameter, value, () => ParentParameter);
				if(value != null)
					wageRate = null;
			}
		}

		private WageRate wageRate;
		[Display(Name = "Тип ставки для расчета зарплаты")]
		public virtual WageRate WageRate { //Устанавливается только у корневого элемента в иерархии дополнительных параметров
			get => wageRate;
			set {
				SetField(ref wageRate, value);
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

		private IList<AdvancedWageParameter> childParameters;
		[Display(Name = "Вложенные параметры")]
		public virtual IList<AdvancedWageParameter> ChildParameters {
			get => childParameters;
			set => SetField(ref childParameters, value);
		}


		public abstract AdvancedWageParameterType AdvancedWageParameterType { get; set; }

		IAdvancedWageParameter IAdvancedWageParameter.ParentParameter => ParentParameter;

		public abstract bool HasConflicWith(IAdvancedWageParameter advancedWageParameter);

		public abstract override string ToString();
	}
}
