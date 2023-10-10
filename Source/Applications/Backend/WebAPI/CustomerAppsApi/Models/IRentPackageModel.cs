using CustomerAppsApi.Library.Dto;

namespace CustomerAppsApi.Models
{
	public interface IRentPackageModel
	{
		FreeRentPackagesDto GetFreeRentPackages(Source source);
	}
}
