using System;
using TaxcomEdo.Contracts.Documents;

namespace TaxcomEdoApi.Library.Factories
{
	public sealed class EdoContainerInfoFactory : IEdoContainerInfoFactory
	{
		public EdoDocFlow CreateEdoContainerInfo(
			string mainDocumentId,
			Guid? docFlowId,
			Guid? internalId,
			string edoDocFlowStatus,
			bool received,
			byte[] documents, 
			string errorDescription)
		{
			return new EdoDocFlow
			{
				MainDocumentId = mainDocumentId,
				DocFlowId = docFlowId,
				InternalId = internalId,
				EdoDocFlowStatus = edoDocFlowStatus,
				Received = received,
				Documents = documents,
				ErrorDescription = errorDescription
			};
		}
	}
}
