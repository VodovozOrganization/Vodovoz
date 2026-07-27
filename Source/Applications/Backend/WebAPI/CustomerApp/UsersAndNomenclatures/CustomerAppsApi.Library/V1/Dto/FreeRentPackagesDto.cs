using System.Collections.Generic;

namespace CustomerAppsApi.Library.V1.Dto
{
	public class FreeRentPackagesDto
	{
		public string ErrorMessage { get; set; }
		public IEnumerable<FreeRentPackageDto> RentPackages { get; set; }
	}
}
