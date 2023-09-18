using System.Collections.Generic;
using Vodovoz.Nodes;

namespace CustomerAppsApi.Library.Dto
{
	public class NomenclaturesDto
	{
		public string ErrorMessage { get; set; }
		public IList<NomenclatureCharacteristicsDto> NomenclatureCharacteristics { get; set; }
	}
}
