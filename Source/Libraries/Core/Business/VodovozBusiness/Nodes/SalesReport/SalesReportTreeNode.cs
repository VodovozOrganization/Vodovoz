using System.Collections.Generic;

namespace VodovozBusiness.Nodes.SalesReport
{
	public class SalesReportTreeNode
	{
		public string Name { get; set; }
		public SalesReportDataNode Data { get; set; }
		public IList<SalesReportTreeNode> Children { get; set; } = new List<SalesReportTreeNode>();
		public decimal TotalCount { get; set; }
		public decimal TotalSum { get; set; }
		public int Level { get; set; }
	}
}
