using System;
using System.Collections.Generic;
using TaxcomEdo.Contracts.Xml.Container;

namespace TaxcomEdoApi.Library.Models.Containers.Interfaces
{
	public interface IContainerDocflow
	{
		IEnumerable<IContainerDocument> Documents { get; }

		Guid? DocflowId { get; set; }

		DocflowStatus Status { get; set; }

		ErrorType? ErrorType { get; set; }

		string ErrorDescription { get; set; }

		DateTime? StatusChangeDateTime { get; set; }

		DateTime? CreationDate { get; set; }

		string Title { get; set; }

		string Comment { get; set; }

		ContainerDescriptionDocFlow ToWrapperXml(bool exportCardAsExternalFile);

		IList<IContainerDocflow> Split();

		DocflowInternalStatus? InternalStatus { get; set; }
	}
}
