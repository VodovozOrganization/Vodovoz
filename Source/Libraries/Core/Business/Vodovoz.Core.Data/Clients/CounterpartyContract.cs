using System;
using Vodovoz.Core.Data.Organizations;

namespace Vodovoz.Core.Data.Clients
{
	public class CounterpartyContract
	{
		public int Id { get; set; }
		public Organization Organization { get; set; }
		public string Number { get; set; }
		public DateTime IssueDate { get; set; }
	}
}
