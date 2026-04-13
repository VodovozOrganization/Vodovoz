using TaxcomEdo.Contracts.Xml.Container.Entities.Warrants;

namespace TaxcomEdoApi.Library.Models.Containers
{
	public class NewContainerWarrant
	{
		protected NewContainerWarrant() { }

		protected NewContainerWarrant(Warrant warrant)
		{
			Warrant = warrant;
		}
		
		public Warrant Warrant { get; protected set; }
		
		public static NewContainerWarrant Create() => new NewContainerWarrant();
		public static NewContainerWarrant Create(Warrant warrant) => new NewContainerWarrant(warrant);
	}
}
