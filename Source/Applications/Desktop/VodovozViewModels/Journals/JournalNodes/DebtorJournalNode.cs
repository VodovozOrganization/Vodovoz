using QS.Project.Journal;
using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalNodes
{
	public class DebtorJournalNode : JournalEntityNodeBase<Order>
	{
		public override string Title => ClientName;

		public int AddressId { get; set; }

		public string AddressName { get; set; }

		public int ClientId { get; set; }

		public string ClientName { get; set; }

		public PersonType OPF { get; set; }

		public int DebtByAddress { get; set; }

		public int DebtByClient { get; set; }

		public int Reserve { get; set; }

		public int? TaskId { get; set; }

		public DateTime? LastOrderDate { get; set; }

		public int? LastOrderBottles { get; set; }

		public string IsResidueExist { get; set; } = "нет";

		public int CountOfDeliveryPoint { get; set; }

		public string Address => String.IsNullOrWhiteSpace(AddressName) ? "Самовывоз" : AddressName;

		public string Phones { get; set; }

		public string Emails { get; set; }
		
		public decimal? FixPrice { get; set; }
	}
}
