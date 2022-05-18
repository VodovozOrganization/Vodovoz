using QS.Project.Journal;
using System;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class DriverMessageJournalNode : JournalNodeBase
	{
		public DateTime CommentDate { get; set; }
		public string DriverName { get; set; }
		public string DriverPhone { get; set; }
		public int RouteListId { get; set; }
		public int OrderId { get; set; }
		public int BottlesReturn { get; set; }
		public int ActualBottlesReturn { get; set; }
		public int AddressBottlesDebt { get; set; }
		public string DriverComment { get; set; }
	}
}
