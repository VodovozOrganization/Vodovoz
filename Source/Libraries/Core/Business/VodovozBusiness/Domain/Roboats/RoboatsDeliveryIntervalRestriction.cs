using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Roboats
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "графики доставки",
		Nominative = "график доставки")]
	public class RoboatsDeliveryIntervalRestriction : PropertyChangedBase, IDomainObject
	{
		private DeliverySchedule _deliverySchedule;
		private int _beforeAcceptOrderHour;

		public virtual int Id { get; set; }

		[Display(Name = "График доставки")]
		public virtual DeliverySchedule DeliverySchedule
		{
			get => _deliverySchedule;
			set => SetField(ref _deliverySchedule, value);
		}

		[Display(Name = "До какого часа необходимо принять заказ")]
		public virtual int BeforeAcceptOrderHour
		{
			get => _beforeAcceptOrderHour;
			set => SetField(ref _beforeAcceptOrderHour, value);
		}
	}
}
