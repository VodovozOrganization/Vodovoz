using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "тарифные зоны",
		Nominative = "тарифная зона")]
	[EntityPermission]
	public class TariffZone : BusinessObjectBase<TariffZone>, IDomainObject
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
	}
}
