using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Mindbox;

namespace Mindbox.CounterpartiesAdmin
{
	internal sealed class ReportRow
	{
		public CounterpartyEntity Counterparty { get; set; }
		public MindboxCounterpartyState State { get; set; }
	}
}
