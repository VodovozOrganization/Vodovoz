using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Complaints
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "требования к логистике для контрагентов",
		Nominative = "требование к логистике для контрагента",
		Prepositional = "требовании к логистике для контрагента",
		PrepositionalPlural = "требованиях к логистике для контрагента")]
	[HistoryTrace]
	public class ComplaintLogisticsRequirements : LogisticsRequirementsBase
	{
		private Complaint _complaint;
		[Display(Name = "Контрагент")]
		public virtual Complaint Complaint
		{
			get => _complaint;
			set => SetField(ref _complaint, value);
		}
		public override string Title => $"Требования к логистике для контрагента";
	}
}
