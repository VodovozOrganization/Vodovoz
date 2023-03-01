using System.Collections.Generic;

namespace RevenueService.Client.Dto
{
	public class RevenueServiceResponseDto
	{
		public string ErrorMessage { get; set; }
		public IList<CounterpartyRevenueServiceDto> CounterpartyDetailsList { get; set; }
	}
}
