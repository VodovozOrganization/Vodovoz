using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Converters
{
	public interface ICameFromConverter
	{
		CounterpartyFrom ConvertCameFromToCounterpartyFrom(int cameFromId);
	}
}
