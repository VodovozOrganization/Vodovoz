using System;

namespace Vodovoz.EntityRepositories.Orders
{
	public class TrueMarkCancellationDto
	{
		public int OrderId { get; set; }
		public string OrganizationInn { get; set; }
		public Guid DocGuid { get; set; }
	}
}
