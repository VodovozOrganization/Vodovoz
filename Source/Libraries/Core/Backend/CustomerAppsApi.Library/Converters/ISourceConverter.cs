using CustomerAppsApi.Library.Dto;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.Converters
{
	public interface ISourceConverter
	{
		GoodsOnlineParameterType ConvertToNomenclatureOnlineParameterType(Source source);
		CounterpartyFrom ConvertToCounterpartyFrom(Source source);
	}
}
