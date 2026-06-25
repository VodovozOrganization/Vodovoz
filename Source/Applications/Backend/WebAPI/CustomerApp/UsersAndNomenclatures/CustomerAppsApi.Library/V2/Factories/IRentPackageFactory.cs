using System.Collections.Generic;
using CustomerAppsApi.Library.V2.Dto;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.V2.Factories
{
	public interface IRentPackageFactory
	{
		FreeRentPackagesDto CreateFreeRentPackagesDto(IEnumerable<FreeRentPackageWithOnlineParametersNode> packageNodes);
	}
}
