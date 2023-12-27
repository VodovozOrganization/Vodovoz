using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Converters
{
	public interface ICameFromConverter
	{
		CounterpartyFrom ConvertCameFromToCounterpartyFrom(int cameFromId);
	}
}
