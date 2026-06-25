using System.Collections.Generic;
using CustomerAppsApi.Library.V1.Dto;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.V1.Factories
{
	public interface IRentPackageFactory
	{
		FreeRentPackagesDto CreateFreeRentPackagesDto(IEnumerable<FreeRentPackageWithOnlineParametersNode> packageNodes);
	}
}
