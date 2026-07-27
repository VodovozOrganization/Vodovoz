using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.V1.Converters
{
	public interface ICameFromConverter
	{
		CounterpartyFrom ConvertCameFromToCounterpartyFrom(int cameFromId);
	}
}
