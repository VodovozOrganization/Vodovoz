using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Factories;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.Factories
{
	public class RentPackageFactory : IRentPackageFactory
	{
		public FreeRentPackagesDto CreateFreeRentPackagesDto(IEnumerable<FreeRentPackageWithOnlineParametersNode> packageNodes)
		{
			return new FreeRentPackagesDto
			{
				RentPackages = packageNodes.Select(CreateFreeRentPackageDto).ToList()
			};
		}

		private FreeRentPackageDto CreateFreeRentPackageDto(FreeRentPackageWithOnlineParametersNode packageNode)
		{
			return new FreeRentPackageDto
			{
				ErpId = packageNode.Id,
				OnlineName = packageNode.OnlineName,
				OnlineAvailability = packageNode.OnlineAvailability,
				Deposit = packageNode.Deposit,
				MinWaterAmount = packageNode.MinWaterAmount,
				DepositServiceId = packageNode.DepositServiceId
			};
		}
	}
}
