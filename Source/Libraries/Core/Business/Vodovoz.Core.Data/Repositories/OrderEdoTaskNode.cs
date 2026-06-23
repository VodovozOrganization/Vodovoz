using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.Repositories
{
	public class OrderEdoTaskNode
	{
		private string _edoTaskTypeName;

		public DateTime RequestTime { get; set; }
		public CustomerEdoRequestSource RequestSource { get; set; }
		public int EdoTaskId { get; set; }
		public string EdoTaskTypeName
		{
			get => _edoTaskTypeName;
			set
			{
				_edoTaskTypeName = value;
				EdoTaskType = (EdoTaskType)Enum.Parse(typeof(EdoTaskType), value);
			}
		}
		public EdoTaskType EdoTaskType { get; set; }
		public EdoTaskStatus EdoTaskStatus { get; set; }
	}

	public class TransferEdoTaskNode
	{
		public int OrderTaskId { get; set; }
		public DateTime RequestTime { get; set; }
		public int TransferTaskId { get; set; }
		public int OrganizationFromId { get; set; }
		public string OrganizationFrom { get; set; }
		public int OrganizationToId { get; set; }
		public string OrganizationTo { get; set; }
		public EdoTaskStatus Status { get; set; }
	}

	public class EdoProblemNode
	{
		public int OrderTaskId { get; set; }
		public int TransferTaskId { get; set; }
		public DateTime Time { get; set; }
		public TaskProblemState State { get; set; }
		public string Message { get; set; }
		public string Description { get; set; }
		public string Recommendation { get; set; }
	}

	public class EdoDocflowForOrderNode
	{
		public int OrderTaskId { get; set; }
		public int TransferTaskId { get; set; }
		public EdoType EdoType { get; set; }
		public int TaxcomDocumentId { get; set; }
		public string TaxcomDocflowId { get; set; }
		public EdoDocFlowStatus TaxcomDocflowStatus { get; set; }
	}
}
