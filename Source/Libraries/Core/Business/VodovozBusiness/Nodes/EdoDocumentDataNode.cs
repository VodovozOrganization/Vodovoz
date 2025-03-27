using Gamma.Utilities;
using System;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Orders.Documents;
using Type = Vodovoz.Domain.Orders.Documents.Type;

namespace VodovozBusiness.Nodes
{
	public class EdoDocumentDataNode
	{
		public EdoDocumentDataNode(EdoContainer edoContainer)
		{
			if(edoContainer is null)
			{
				throw new ArgumentNullException(nameof(edoContainer));
			}

			DocflowId = edoContainer.DocFlowId?.ToString();
			OldEdoDocumentType = edoContainer.Type;
			EdoDocFlowStatus = edoContainer.EdoDocFlowStatus;
			IsReceived = edoContainer.Received;
			ErrorDescription = edoContainer.ErrorDescription;
			IsNewDockflow = false;
		}

		public EdoDocumentDataNode(){ }

		public string DocflowId { get; set; }
		public EdoDocFlowStatus? EdoDocFlowStatus { get; set; }
		public bool IsReceived { get; set; }
		public string ErrorDescription { get; set; }
		public bool IsNewDockflow { get; set; }

		public Type? OldEdoDocumentType { get; set; }

		public EdoDocumentType? EdoDocumentType { get; set; }
		public DocumentEdoTaskStage? EdoTaskStage { get; set; }
		public EdoDocumentStatus? EdoDocumentStatus { get; set; }


		public string DocumentType =>
			IsNewDockflow
			? EdoDocumentType?.GetEnumTitle()
			: OldEdoDocumentType?.GetEnumTitle();
	}
}
