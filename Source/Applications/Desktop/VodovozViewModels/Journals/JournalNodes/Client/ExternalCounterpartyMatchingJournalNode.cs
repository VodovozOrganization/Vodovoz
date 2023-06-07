using QS.Project.Journal;
using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Client
{
	public class ExternalCounterpartyMatchingJournalNode : JournalNodeBase
	{
		public int Id { get; set; }
		public string PhoneNumber { get; set; }
		public CounterpartyFrom CounterpartyFrom { get; set; }
		public DateTime Created { get; set; }
		public ExternalCounterpartyMatchingStatus Status { get; set; }
		public int? CounterpartyId { get; set; }
		public string CounterpartyName { get; set; }
	}
}
