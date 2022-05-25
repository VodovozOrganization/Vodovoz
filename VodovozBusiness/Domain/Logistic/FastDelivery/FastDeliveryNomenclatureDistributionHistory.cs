using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Logistic.FastDelivery
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		Nominative = "строка истории распределения номенклатур для экспресс-доставки",
		NominativePlural = "строки истории распределения номенклатур для экспресс-доставки")]
	public class FastDeliveryNomenclatureDistributionHistory : PropertyChangedBase, IDomainObject
	{
		private Nomenclature _nomenclature;
		private decimal _percent;
		private FastDeliveryAvailabilityHistory _fastDeliveryAvailabilityHistory;

		public virtual int Id { get; set; }

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Процент")]
		public virtual decimal Percent
		{
			get => _percent;
			set => SetField(ref _percent, value);
		}
		
		[Display(Name = "История доступности экспресс-доставки")]
		public virtual FastDeliveryAvailabilityHistory FastDeliveryAvailabilityHistory
		{
			get => _fastDeliveryAvailabilityHistory;
			set => SetField(ref _fastDeliveryAvailabilityHistory, value);
		}
	}
}
