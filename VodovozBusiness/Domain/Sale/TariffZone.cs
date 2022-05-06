using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "тарифные зоны",
		Nominative = "тарифная зона")]
	[EntityPermission]
	public class TariffZone : BusinessObjectBase<TariffZone>, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		private string name;
		[Required(ErrorMessage = "Имя обязательно")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value, () => Name);
		}

		private bool _isFastDeliveryAvailable;
		public virtual bool IsFastDeliveryAvailable
		{
			get => _isFastDeliveryAvailable;
			set => SetField(ref _isFastDeliveryAvailable, value);
		}

		private TimeSpan _fastDeliveryTimeFrom;
		[Display(Name = "От часа")]
		public virtual TimeSpan FastDeliveryTimeFrom
		{
			get => _fastDeliveryTimeFrom;
			set => SetField(ref _fastDeliveryTimeFrom, value, () => FastDeliveryTimeFrom);
		}

		private TimeSpan _fastDeliveryTimeTo;
		[Display(Name = "До часа")]
		public virtual TimeSpan FastDeliveryTimeTo
		{
			get => _fastDeliveryTimeTo;
			set => SetField(ref _fastDeliveryTimeTo, value, () => FastDeliveryTimeTo);
		}

		public virtual bool IsFastDeliveryAvailableAtCurrentTime
		{
			get
			{
				return IsFastDeliveryAvailable
				       && FastDeliveryTimeFrom <= DateTime.Now.TimeOfDay
				       && DateTime.Now.TimeOfDay <= FastDeliveryTimeTo;
			}
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(IsFastDeliveryAvailable
				&& FastDeliveryTimeFrom == FastDeliveryTimeTo)
			{
				yield return new ValidationResult("Неверно указан интервал для доставки за час.", new[] { nameof(FastDeliveryTimeTo) });
			}
		}
	}
}
