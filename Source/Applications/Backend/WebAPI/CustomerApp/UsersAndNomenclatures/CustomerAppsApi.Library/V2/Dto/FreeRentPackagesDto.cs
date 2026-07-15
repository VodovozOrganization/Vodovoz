using System.Collections.Generic;

namespace CustomerAppsApi.Library.V2.Dto
{
	public class FreeRentPackagesDto
	{
		public string ErrorMessage { get; set; }
		public IList<FreeRentPackageDto> RentPackages { get; set; }
	}
}
