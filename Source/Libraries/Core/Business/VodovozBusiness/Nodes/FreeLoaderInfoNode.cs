using System;

namespace Vodovoz.Nodes
{
	public class FreeLoaderInfoNode
	{
		public int OrderId { get; set; }
		public DateTime? Date { get; set; }
		public string Client { get; set; }
		public string Address { get; set; }
		public string Phone { get; set; }
	}
}
