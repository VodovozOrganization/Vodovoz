using CustomerAppsApi.Library.Dto;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Converters
{
	public interface ISourceConverter
	{
		GoodsOnlineParameterType ConvertToNomenclatureOnlineParameterType(Source source);
		CounterpartyFrom ConvertToCounterpartyFrom(Source source);
	}
}
