using System;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories.Cash
{
	public class EmployeeBalanceNode
	{
		public int SubdivisionId { get; set; }
		public string SubdivisionName { get; set; }
		public decimal Balance { get; set; }
		public DateTime Date { get; set; }
		public Employee Cashier { get; set; }
		public SubdivisionBalanceNode ParentBalanceNode { get; set; }
	}
}
