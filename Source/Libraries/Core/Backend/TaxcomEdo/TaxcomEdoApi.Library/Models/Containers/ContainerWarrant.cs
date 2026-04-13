using System.Collections.Generic;
using TaxcomEdo.Contracts.Xml.Container.Entities.Warrants;
using TaxcomEdoApi.Library.Models.Containers.Interfaces;

namespace TaxcomEdoApi.Library.Models.Containers
{
	public class ContainerWarrant : IContainerWarrant
	{
		public IList<IContainerWarrantCard> WarrantCards { get; } = new List<IContainerWarrantCard>();
		public byte[] RawWarrantImage { get; set; }
		public Warrant WarrantWrapper { get; set; }
		public IList<IContainerWarrantsChain> WarrantChains { get; } = new List<IContainerWarrantsChain>();
	}
}
