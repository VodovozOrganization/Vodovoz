using CustomerAppsApi.Library.Dto;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Models
{
	public interface IRentPackageModel
	{
		FreeRentPackagesDto GetFreeRentPackages(Source source);
	}
}
