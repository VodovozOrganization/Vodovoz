using System.Collections.Generic;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Factories
{
	public interface IRentPackageFactory
	{
		FreeRentPackagesDto CreateFreeRentPackagesDto(IEnumerable<FreeRentPackageWithOnlineParametersNode> packageNodes);
	}
}
