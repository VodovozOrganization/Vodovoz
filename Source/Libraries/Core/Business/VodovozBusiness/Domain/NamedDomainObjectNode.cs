using QS.DomainModel.Entity;

namespace Vodovoz.Domain
{
	public class NamedDomainObjectNode : INamedDomainObject
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
