using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.WageCalculation
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "параметры расчёта зарплаты",
			Nominative = "параметр расчёта зарплаты",
			Accusative = "параметра расчёта зарплаты",
			Genitive = "параметра расчёта зарплаты"
	)]
	[HistoryTrace]
	[EntityPermission]
	public abstract class WageParameter : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public abstract WageParameterTypes WageParameterType { get; set; }

		public virtual int Id { get; set; }

		public virtual string Title => $"{WageParameterType.GetEnumTitle()}";

		private DateTime startDate;
		[Display(Name = "Дата начала")]
		public virtual DateTime StartDate {
			get => startDate;
			set => SetField(ref startDate, value, () => StartDate);
		}

		private DateTime? endDate;
		[Display(Name = "Дата окончания")]
		public virtual DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value, () => EndDate);
		}

		private Employee employee;
		[Display(Name = "Сотрудник")]
		public virtual Employee Employee {
			get => employee;
			set => SetField(ref employee, value, () => Employee);
		}

		private WageParameterTargets wageParameterTarget;
		[Display(Name = "Назначение расчета")]
		public virtual WageParameterTargets WageParameterTarget {
			get => wageParameterTarget;
			set => SetField(ref wageParameterTarget, value, () => WageParameterTarget);
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			yield break;
		}

		#endregion IValidatableObject implementation
	}
}
