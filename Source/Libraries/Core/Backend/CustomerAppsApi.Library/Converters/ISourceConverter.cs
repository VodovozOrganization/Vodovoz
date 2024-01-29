using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.Converters
{
	public interface ISourceConverter
	{
		GoodsOnlineParameterType ConvertToNomenclatureOnlineParameterType(Source source);
	}
}
