using System;
using TaxcomEdo.Contracts.Documents;

namespace TaxcomEdoApi.Library.Factories
{
	public interface IEdoContainerInfoFactory
	{
		EdoContainerInfo CreateEdoContainerInfo(
			string mainDocumentId,
			Guid? docFlowId,
			Guid? internalId,
			string edoDocFlowStatus,
			bool received,
			string errorDescription);
	}
}
