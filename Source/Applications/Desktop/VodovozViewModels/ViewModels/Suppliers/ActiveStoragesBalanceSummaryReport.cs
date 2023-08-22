using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Suppliers
{
	public class ActiveStoragesBalanceSummaryReport
	{
		public DateTime EndDate { get; set; }
		public List<ActiveStoragesBalanceSummaryRow> ActiveStoragesBalanceRows { get; set; }
	}
}
