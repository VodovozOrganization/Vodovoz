using System.Collections.Generic;
using TaxcomEdo.Contracts.Xml.Container.Entities.Warrants;

namespace TaxcomEdoApi.Library.Models.Containers.Interfaces
{
	public interface IContainerWarrant
	{
		IList<IContainerWarrantCard> WarrantCards { get; }
		byte[] RawWarrantImage { get; }
		Warrant WarrantWrapper { get; }
		IList<IContainerWarrantsChain> WarrantChains { get; }
	}
}
