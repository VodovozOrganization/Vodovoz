using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Converters
{
	public interface ICameFromConverter
	{
		CounterpartyFrom ConvertCameFromToCounterpartyFrom(int cameFromId);
		Result<CounterpartyFrom> ConvertSourceToCounterpartyFrom(Source source);
	}
}
