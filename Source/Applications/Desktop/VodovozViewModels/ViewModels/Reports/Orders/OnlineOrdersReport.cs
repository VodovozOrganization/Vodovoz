using System;
using System.Collections.Generic;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;

namespace Vodovoz.ViewModels.ViewModels.Reports.Orders
{
	public class OnlineOrdersReport
	{
		private readonly IEnumerable<OnlineOrdersJournalNode> _onlineOrdersJournalNodes;
		
		public OnlineOrdersReport(
			DateTime createDateFrom,
			DateTime createDateTo,
			IEnumerable<OnlineOrdersJournalNode> onlineOrdersJournalNodes
			)
		{
			_onlineOrdersJournalNodes = onlineOrdersJournalNodes ?? new List<OnlineOrdersJournalNode>();
			
			CreateDateFrom = createDateFrom;
			CreateDateTo = createDateTo;
			ReportCreatedAt = DateTime.Now;
		}
		
		public string Title => 
			$"Список онлайн заказов " +
			$"за период с {CreateDateFrom:dd.MM.yyyy} по {CreateDateTo:dd.MM.yyyy}";
		
		public DateTime CreateDateFrom { get; }

		public DateTime CreateDateTo { get; }

		public DateTime ReportCreatedAt { get; }

		public void Export(string path)
		{
			
		}
	}
}
