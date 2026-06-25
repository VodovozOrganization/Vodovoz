using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.V2.Converters
{
	public interface ICameFromConverter
	{
		CounterpartyFrom ConvertCameFromToCounterpartyFrom(int cameFromId);
	}
}
