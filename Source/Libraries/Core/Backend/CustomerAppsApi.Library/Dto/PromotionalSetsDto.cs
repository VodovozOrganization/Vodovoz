using System.Collections.Generic;

namespace CustomerAppsApi.Library.Dto
{
	public class PromotionalSetsDto
	{
		public string ErrorMessage { get; set; }
		public IList<PromotionalSetDto> PromotionalSets { get; set; }
	}
}
