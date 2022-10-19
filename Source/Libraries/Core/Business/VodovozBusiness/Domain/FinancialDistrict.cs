using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "финансовые районы",
        Nominative = "финансовый район")]
    [EntityPermission]
    [HistoryTrace]
    public class FinancialDistrict: PropertyChangedBase, IDomainObject, IValidatableObject, ICloneable
    {
        #region Свойства
		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название района")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		private Geometry border;
		[Display(Name = "Граница")]
		public virtual Geometry Border {
			get => border;
			set => SetField(ref border, value);
		}
		
		private Organization organization;
		[Display(Name = "Организация")]
		public virtual Organization Organization {
			get => organization;
			set => SetField(ref organization, value);
		}
		
		private FinancialDistrictsSet financialDistrictsSet;
		[Display(Name = "Версия финансовых районов")]
		public virtual FinancialDistrictsSet FinancialDistrictsSet {
			get => financialDistrictsSet;
			set => SetField(ref financialDistrictsSet, value);
		}
		
		private FinancialDistrict copyOf;
		[Display(Name = "Копия финансового района")]
		public virtual FinancialDistrict CopyOf {
			get => copyOf;
			set => SetField(ref copyOf, value);
		}
		
		#endregion
		
		#region IValidatableObject implementation
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrWhiteSpace(Name)) {
				yield return new ValidationResult(
					"Необходимо заполнить имя района",
					new[] { nameof(this.Name) }
				);
			}
			if(Organization == null) {
				yield return new ValidationResult(
					$"Для района \"{Name}\" необходимо указать организацию",
					new[] { nameof(this.Organization) }
				);
			}
			if(Border == null) {
				yield return new ValidationResult(
					$"Для района \"{Name}\"Необходимо нарисовать границы на карте",
					new[] { nameof(this.Border) }
				);
			}
		}
		
		#endregion
		
		#region ICloneable implementation

		public virtual object Clone()
		{
			var newDistrict = new FinancialDistrict {
				Name = Name,
				Border = Border?.Copy(),
				Organization = Organization,
			};

			return newDistrict;
		}

		#endregion
    }
}
