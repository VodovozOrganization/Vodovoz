using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "ответственные в рекламации",
		Nominative = "ответственный в рекламации",
		Prepositional = "ответственом в рекламации",
		PrepositionalPlural = "ответственных в рекламации")]
	[HistoryTrace]
	public class ComplaintGuiltyItem : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		private Complaint complaint;
		[Display(Name = "Рекламация")]
		public virtual Complaint Complaint {
			get => complaint;
			set => SetField(ref complaint, value, () => Complaint);
		}

		private Responsible _responsible;
		[Display(Name = "Ответственный")]
		public virtual Responsible Responsible {
			get => _responsible;
			set {
				if(SetField(ref _responsible, value, () => Responsible))
					OnGuiltyTypeChange?.Invoke();
			}
		}

		private Employee employee;
		[Display(Name = "Сотрудник")]
		public virtual Employee Employee {
			get => employee;
			set => SetField(ref employee, value, () => Employee);
		}

		private Subdivision subdivision;
		[Display(Name = "Подразделение")]
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value, () => Subdivision);
		}

		public virtual string Title
		{
			get 
			{
				if(Responsible == null)
				{
					return string.Format("Ответственный №{0} в рекламации №{1}", Id, Complaint?.Id);
				}

				if(Responsible.IsSubdivisionResponsible)
				{
					return $"Ответственный \"{Subdivision?.Name}\"";
				}

				if(Responsible.IsEmployeeResponsible)
				{
					return $"Ответственный сотрудник {Employee?.ShortName}";
				}
				
				return string.Format("Ответственный №{0} в рекламации №{1}", Id, Complaint?.Id);				
			}
		}

		public ComplaintGuiltyItem() { }

		public virtual Action OnGuiltyTypeChange { get; set; } = null;

		public virtual string GetGuiltySubdivisionOrEmployee => Subdivision?.Name ?? Employee?.ShortName;

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Responsible == null)
				yield return new ValidationResult(
					"Ответственная сторона не выбрана",
					new[] { this.GetPropertyName(o => o.Responsible) }
				);
			if(Responsible != null && Responsible.IsEmployeeResponsible && Employee == null)
				yield return new ValidationResult(
					"Укажите ответственного сотрудника",
					new[] { this.GetPropertyName(o => o.Employee) }
				);
			if(Responsible != null && Responsible.IsSubdivisionResponsible && Subdivision == null)
				yield return new ValidationResult(
					"Укажите ответственный отдел ВВ",
					new[] { this.GetPropertyName(o => o.Subdivision) }
				);
		}

		#endregion
	}
}
