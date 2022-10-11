using System.Collections.Generic;
using Gamma.Utilities;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Undeliveries
{
	public class UndeliveredOrderCountNode
	{
		public int SubdivisionId { get; set; }
		public GuiltyTypes Type { get; set; }
		public virtual int Count { get; set; }
		public string Subdivision { get; set; } = "Неизвестно";
		public virtual string GuiltySide => SubdivisionId <= 0 ? Type.GetEnumTitle() : Subdivision;
		public virtual string CountStr => Count.ToString();
		public virtual UndeliveredOrderCountNode Parent { get; set; } = null;
		public virtual List<UndeliveredOrderCountNode> Children { get; set; } = new List<UndeliveredOrderCountNode>();
	}
}