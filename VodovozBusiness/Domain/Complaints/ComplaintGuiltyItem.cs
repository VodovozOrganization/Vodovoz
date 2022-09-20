using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
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

		private ComplaintGuiltyTypes? guiltyType;
		[Display(Name = "Ответственный")]
		public virtual ComplaintGuiltyTypes? GuiltyType {
			get => guiltyType;
			set {
				if(SetField(ref guiltyType, value, () => GuiltyType))
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

		public virtual string Title {
			get {
				if(!GuiltyType.HasValue)
					return string.Format("Ответственный №{0} в рекламации №{1}", Id, Complaint?.Id);
				switch(GuiltyType.Value) {
					case ComplaintGuiltyTypes.None:
					case ComplaintGuiltyTypes.Client:
					case ComplaintGuiltyTypes.Depreciation:
					case ComplaintGuiltyTypes.Supplier:
						return $"Ответственный \"{GuiltyType.GetEnumTitle()}\"";
					case ComplaintGuiltyTypes.Subdivision:
						return $"Ответственный \"{Subdivision?.Name}\"";
					case ComplaintGuiltyTypes.Employee:
						return $"Ответственный сотрудник {Employee?.ShortName}";
					default:
						return string.Format("Ответственный №{0} в рекламации №{1}", Id, Complaint?.Id);
				}
			}
		}

		public ComplaintGuiltyItem() { }

		public virtual Action OnGuiltyTypeChange { get; set; } = null;

		public virtual string GetGuiltySubdivisionOrEmployee => Subdivision?.Name ?? Employee?.ShortName;

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(GuiltyType == null)
				yield return new ValidationResult(
					"Ответственная сторона не выбрана",
					new[] { this.GetPropertyName(o => o.GuiltyType) }
				);
			if(GuiltyType == ComplaintGuiltyTypes.Employee && Employee == null)
				yield return new ValidationResult(
					"Укажите ответственного сотрудника",
					new[] { this.GetPropertyName(o => o.Employee) }
				);
			if(GuiltyType == ComplaintGuiltyTypes.Subdivision && Subdivision == null)
				yield return new ValidationResult(
					"Укажите ответственный отдел ВВ",
					new[] { this.GetPropertyName(o => o.Subdivision) }
				);
		}

		#endregion
	}
}
