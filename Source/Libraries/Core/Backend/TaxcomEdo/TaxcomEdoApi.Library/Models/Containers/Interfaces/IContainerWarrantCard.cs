using System;
using System.Collections.Generic;
using TaxcomEdo.Contracts.Xml.Container.Entities.Warrant;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Models.Containers.Interfaces
{
	public interface IContainerWarrantCard
	{
		WarrantCard RawWarrantCard { get; }

		string ChildMetaWarrant { get; }

		string ChildFileWarrant { get; }

		DateTime? ValidTo { get; }

		DateTime? ValidFrom { get; }

		IFileData WarrantImage { get; }

		IList<IFileData> WarrantSignatures { get; }

		IEnumerable<IFileData> DocSigns { get; }

		ContainerWarrantCard.IWarrantCardMeta DescriptionMeta { get; }
	}
}
