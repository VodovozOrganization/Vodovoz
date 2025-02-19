using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DocumentFormat.OpenXml.Wordprocessing;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Domain.Settings
{
	public abstract class PaymentTypeOrganizationSettings : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private Organization _organizationForOrder;
		
		public virtual int Id { get; set; }

		public virtual Organization OrganizationForOrder
		{
			get => _organizationForOrder;
			set => SetField(ref _organizationForOrder, value);
		}
		
		public abstract PaymentType PaymentType { get; }

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			string name = null;
			var appellativeAttribute = GetType().GetCustomAttribute<AppellativeAttribute>(true);

			if(appellativeAttribute != null)
			{
				name = appellativeAttribute.Nominative;
			}
			else
			{
				name = "Настройка организации по типу оплаты";
			}
			
			if(OrganizationForOrder is null)
			{
				yield return new ValidationResult(
					$"Организация у {name} должна быть заполнена", new[] { nameof(OrganizationForOrder) });
			}
		}
	}
}
