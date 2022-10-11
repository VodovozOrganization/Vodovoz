using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
		public virtual int Id { get; set; }

		public abstract string Title { get; }
		
		private WageParameterTypes wageParameterType;
		public virtual WageParameterTypes WageParameterType {
			get => wageParameterType;
			set => SetField(ref wageParameterType, value);
		}

		private DateTime startDate;
		[Display(Name = "Дата начала")]
		public virtual DateTime StartDate {
			get => startDate;
			set => SetField(ref startDate, value );
		}

		private DateTime? endDate;
		[Display(Name = "Дата окончания")]
		public virtual DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value );
		}

		private bool isStartedWageParameter;
		/// <summary>
		/// Расчет является стартовым.
		/// Устанавливается автоматически, в дальнейшем меняется на другой расчет.
		/// </summary>
		[Display(Name = "Стартовый расчет")]
		public virtual bool IsStartedWageParameter {
			get => isStartedWageParameter;
			set => SetField(ref isStartedWageParameter, value);
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			yield break;
		}

		#endregion IValidatableObject implementation
	}
}
