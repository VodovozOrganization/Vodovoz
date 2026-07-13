using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.V2.Converters
{
	/// <summary>
	/// Конвертер из значения CameFrom
	/// </summary>
	public interface ICameFromConverter
	{
		/// <summary>
		/// Конвертация в <see cref="CounterpartyFrom"/>
		/// </summary>
		/// <param name="cameFromId">Идентификатор откуда клиент</param>
		/// <returns></returns>
		CounterpartyFrom ConvertCameFromToCounterpartyFrom(int cameFromId);
	}
}
