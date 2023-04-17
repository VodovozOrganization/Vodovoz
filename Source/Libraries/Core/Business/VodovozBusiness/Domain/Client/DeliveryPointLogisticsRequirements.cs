using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "требования к логистике для точек доставки",
		Nominative = "требование к логистике для точки доставки",
		Prepositional = "требовании к логистике для точки доставки",
		PrepositionalPlural = "требованиях к логистике для точек доставки")]
	[HistoryTrace]
	public class DeliveryPointLogisticsRequirements : LogisticsRequirements
	{
		private DeliveryPoint _deliveryPoint;
		[Display(Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint
		{
			get => _deliveryPoint;
			set => SetField(ref _deliveryPoint, value);
		}
		public override string Title => $"Требования к логистике для точки доставки";
	}
}
