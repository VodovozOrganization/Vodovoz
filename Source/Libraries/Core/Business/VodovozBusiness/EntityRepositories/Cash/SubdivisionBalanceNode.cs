using System.Collections.Generic;

namespace Vodovoz.EntityRepositories.Cash
{
	public class SubdivisionBalanceNode
	{
		public string SubdivisionName { get; set; }
		public IList<EmployeeBalanceNode> ChildResultBalanceNodes { get; set; }
	}
}
