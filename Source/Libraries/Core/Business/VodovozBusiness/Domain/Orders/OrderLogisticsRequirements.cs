using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "требования к логистике для заказов",
		Nominative = "требование к логистике для заказа",
		Prepositional = "требовании в логистике для заказа",
		PrepositionalPlural = "требованиях в логистике для заказов")]
	[HistoryTrace]
	public class OrderLogisticsRequirements : LogisticsRequirements
	{
		private Order _order;
		[Display(Name = "Заказ")]
		public virtual Order Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		public override string Title => $"Требования к логистике для заказа";
	}
}
