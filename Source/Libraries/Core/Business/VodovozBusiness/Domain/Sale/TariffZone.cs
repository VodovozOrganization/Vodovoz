using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "тарифные зоны",
		Nominative = "тарифная зона")]
	[EntityPermission]
	[HistoryTrace]
	public class TariffZone : BusinessObjectBase<TariffZone>, IDomainObject, IValidatableObject
	{
		private string _name;
		private bool _isFastDeliveryAvailable;
		private TimeSpan _fastDeliveryTimeFrom;
		private TimeSpan _fastDeliveryTimeTo;

		public virtual int Id { get; set; }

		[Required(ErrorMessage = "Имя обязательно")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		public virtual bool IsFastDeliveryAvailable
		{
			get => _isFastDeliveryAvailable;
			set => SetField(ref _isFastDeliveryAvailable, value);
		}

		[Display(Name = "От часа")]
		public virtual TimeSpan FastDeliveryTimeFrom
		{
			get => _fastDeliveryTimeFrom;
			set => SetField(ref _fastDeliveryTimeFrom, value);
		}

		[Display(Name = "До часа")]
		public virtual TimeSpan FastDeliveryTimeTo
		{
			get => _fastDeliveryTimeTo;
			set => SetField(ref _fastDeliveryTimeTo, value);
		}

		public virtual bool IsFastDeliveryAvailableAtCurrentTime => IsFastDeliveryAvailable
			&& FastDeliveryTimeFrom <= DateTime.Now.TimeOfDay
			&& DateTime.Now.TimeOfDay <= FastDeliveryTimeTo;

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(IsFastDeliveryAvailable && FastDeliveryTimeFrom == FastDeliveryTimeTo)
			{
				yield return new ValidationResult("Неверно указан интервал для доставки за час.", new[] { nameof(FastDeliveryTimeTo) });
			}
		}
	}
}
