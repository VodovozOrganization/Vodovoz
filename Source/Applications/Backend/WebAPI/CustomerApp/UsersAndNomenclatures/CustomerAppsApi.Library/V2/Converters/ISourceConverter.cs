using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.V2.Converters
{
	/// <summary>
	/// Конвертер источника
	/// </summary>
	public interface ISourceConverter
	{
		/// <summary>
		/// Конвертация в тип онлайн параметра <see cref="GoodsOnlineParameterType"/>
		/// </summary>
		/// <param name="source">Источник</param>
		/// <returns></returns>
		GoodsOnlineParameterType ConvertToNomenclatureOnlineParameterType(Source source);
		/// <summary>
		/// Конвертация в откуда клиент <see cref="CounterpartyFrom"/>
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		CounterpartyFrom ConvertToCounterpartyFrom(Source source);
	}
}
