using System.Collections.Generic;
using TaxcomEdo.Contracts.Xml.Container.Entities.Warrant;
using TaxcomEdoApi.Library.Models.Containers;
using TaxcomEdoApi.Library.Models.Interfaces;

namespace TaxcomEdoApi.Library.Builders
{
	public class ContainerWarrantCardBuilder
	{
		private ContainerWarrantCard _card = new ContainerWarrantCard();
		
		public ContainerWarrantCardBuilder DocSigns(IEnumerable<IFileData> docSigns)
		{
			
			return this;
		}
		
		public ContainerWarrantCardBuilder FromWarrantCard(WarrantCard warrantCard)
		{
			this
				.Meta(warrantCard)
				.WarrantImage(warrantCard)
				.WarrantSignatures(warrantCard)
				.DocSigns(warrantCard);
			
			return this;
		}
		
		public ContainerWarrantCard Build()
		{
			var card = _card;
			_card = new ContainerWarrantCard();
			
			return card;
		}

		private ContainerWarrantCardBuilder Meta(WarrantCard warrantCard)
		{
			if(warrantCard.Description.Item is WarrantCardDescriptionMeta meta)
			{
				
			}

			return this;
		}
		
		private ContainerWarrantCardBuilder WarrantImage(WarrantCard warrantCard)
		{
			return this;
		}
		
		private ContainerWarrantCardBuilder WarrantSignatures(WarrantCard warrantCard)
		{
			return this;
		}
		
		private ContainerWarrantCardBuilder DocSigns(WarrantCard warrantCard)
		{
			return this;
		}
		
		public static ContainerWarrantCardBuilder Create() => new ContainerWarrantCardBuilder();
	}
}
