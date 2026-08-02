using CustomerAppsApi.Library.V1.Dto;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V1.Models
{
	public interface IRentPackageModel
	{
		FreeRentPackagesDto GetFreeRentPackages(Source source);
	}
}
