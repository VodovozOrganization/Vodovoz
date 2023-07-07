using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Converters
{
	public interface ISourceConverter
	{
		NomenclatureOnlineParameterType ConvertToNomenclatureOnlineParameterType(Source source);
	}
}
