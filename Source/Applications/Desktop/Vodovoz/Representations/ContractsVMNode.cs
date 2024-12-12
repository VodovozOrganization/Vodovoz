using System;

namespace Vodovoz.ViewModel
{
	public class ContractsVMNode
	{
		public int Id{ get; set;}

		public DateTime IssueDate{ get; set;}

		public bool IsArchive{ get; set;}

		public bool OnCancellation{ get; set;}

		public string ContractNumber{ get; set; }

		public string Title => $"{ContractNumber} от {IssueDate:d}";

		public string Organization { get; set;}
	}
}
