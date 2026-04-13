using Core.Infrastructure;
using TaxcomEdo.Contracts.Xml.Container.Entities.Warrants;
using TaxcomEdoApi.Library.Models.Containers;

namespace TaxcomEdoApi.Library.Builders
{
	public class ContainerWarrantBuilder
	{
		private ContainerWarrant _warrant =  new ContainerWarrant();
		
		public ContainerWarrantBuilder FromWarrant(Warrant warrant)
		{
			this
				.WarrantCards(warrant)
				.WarrantImage(warrant)
				.WarrantChains(warrant);
			
			return this;
		}
		
		public ContainerWarrantBuilder Card(ContainerWarrantCard card)
		{
			_warrant.WarrantCards.Add(card);
			return this;
		}
		
		public ContainerWarrantBuilder WarrantImage(Warrant warrant)
		{
			_warrant.RawWarrantImage = warrant.SerializeObject();
			return this;
		}

		private ContainerWarrantBuilder WarrantCards(Warrant warrant)
		{
			var cardBuilder = ContainerWarrantCardBuilder
				.Create();
			
			foreach(var warrantCard in warrant.WarrantCards)
			{
				var card = cardBuilder
					.FromWarrantCard(warrantCard)
					.Build();
				
				Card(card);
			}
			
			return this;
		}
		
		private ContainerWarrantBuilder WarrantChains(Warrant warrant)
		{
			
			return this;
		}

		public ContainerWarrant Build()
		{
			var warrant = _warrant;
			_warrant = new ContainerWarrant();
			
			return warrant;
		}
		
		public static ContainerWarrantBuilder Create() => new ContainerWarrantBuilder();
	}
}
