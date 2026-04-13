using System.Collections.Generic;

namespace TaxcomEdoApi.Library.Models.Containers.Interfaces
{
	public interface IContainerWarrantsChain
	{
		IContainerWarrantCard FinalWarrant { get; set; }
		IList<IContainerWarrantCard> ReDelegationWarrants { get; set; }
	}
}
