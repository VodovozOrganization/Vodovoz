using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Domain.Settings
{
	/// <summary>
	/// Базовый класс для настроек организации по типам оплат
	/// </summary>
	public abstract class PaymentTypeOrganizationSettings : PropertyChangedBase, IOrganizations, IDomainObject, IValidatableObject
	{
		private IObservableList<Organization> _organizations = new ObservableList<Organization>();
		
		public virtual int Id { get; set; }

		/// <summary>
		/// Список организаций
		/// </summary>
		public virtual IObservableList<Organization> Organizations
		{
			get => _organizations;
			set => SetField(ref _organizations, value);
		}
		
		/// <summary>
		/// Тип оплаты
		/// </summary>
		public abstract PaymentType PaymentType { get; }
		
		public override string ToString()
		{
			var appellativeAttribute = GetType().GetCustomAttribute<AppellativeAttribute>(true);
			return appellativeAttribute != null ? appellativeAttribute.Nominative : "Настройка организации по типу оплаты";
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!Organizations.Any())
			{
				yield return new ValidationResult(
					$"Хотя бы одна организация для {ToString()} должна быть заполнена",
					new[] { nameof(Organizations) });
			}
		}
	}
}
