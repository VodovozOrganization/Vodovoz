using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "требования к логистике для контрагентов",
		Nominative = "требование к логистике для контрагента",
		Prepositional = "требовании к логистике для контрагента",
		PrepositionalPlural = "требованиях к логистике для контрагента")]
	[HistoryTrace]
	public class CounterpartyLogisticsRequirements : LogisticsRequirements
	{
		private Counterparty _counterparty;
		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}
		public override string Title => $"Требования к логистике для контрагента";
	}
}
