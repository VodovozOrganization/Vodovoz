using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Logistic.FastDelivery
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "строка заказа истории экспресс-доставки",
		NominativePlural = "строки заказа истории экспресс-доставки")]
	[EntityPermission]
	public class FastDeliveryOrderItemHistory : PropertyChangedBase, IDomainObject
	{
		private Nomenclature _nomenclature;
		private decimal _count;
		private FastDeliveryAvailabilityHistory _fastDeliveryAvailabilityHistory;

		public virtual int Id { get; set; }

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Количество")]
		public virtual decimal Count
		{
			get => _count;
			set => SetField(ref _count, value);
		}

		[Display(Name = "История доступности экспресс-доставки")]
		public virtual FastDeliveryAvailabilityHistory FastDeliveryAvailabilityHistory
		{
			get => _fastDeliveryAvailabilityHistory;
			set => SetField(ref _fastDeliveryAvailabilityHistory, value);
		}
	}
}
