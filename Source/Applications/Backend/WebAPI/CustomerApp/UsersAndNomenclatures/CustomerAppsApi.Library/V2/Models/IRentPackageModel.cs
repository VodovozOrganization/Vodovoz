using CustomerAppsApi.Library.V2.Dto;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.V2.Models
{
	public interface IRentPackageModel
	{
		FreeRentPackagesDto GetFreeRentPackages(Source source);
	}
}
