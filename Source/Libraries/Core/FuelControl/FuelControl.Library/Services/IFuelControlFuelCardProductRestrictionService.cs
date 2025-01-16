using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FuelControl.Library.Services
{
	public interface IFuelControlFuelCardProductRestrictionService
	{
		/// <summary>
		/// Получить список отоварных ограничителей для карты
		/// </summary>
		/// <param name="cardId">Id карты в газпроме</param>
		/// <param name="sessionId">Id сессии в газпроме</param>
		/// <param name="apiKey">Ключ АПИ в газпроме</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns></returns>
		Task<IEnumerable<string>> GetProductRestrictionsByCardId(string cardId, string sessionId, string apiKey, CancellationToken cancellationToken);

		/// <summary>
		/// Удалить товарный ограничитель по Id
		/// </summary>
		/// <param name="restrictionId">Id ограничителя</param>
		/// <param name="sessionId">Id сессии в газпроме</param>
		/// <param name="apiKey">Ключ АПИ в газпроме</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns></returns>
		Task<bool> RemoveProductRestictionById(string restrictionId, string sessionId, string apiKey, CancellationToken cancellationToken);

		/// <summary>
		/// Установить общий товарный ограничитель<br/>
		/// Добавляется товарный ограничитель на все топливо без указания группы топлива
		/// </summary>
		/// <param name="cardId">Id карты в газпроме</param>
		/// <param name="sessionId">Id сессии в газпроме</param>
		/// <param name="apiKey">Ключ АПИ в газпроме</param>
		/// <param name="cancellationToken"></param>
		/// <returns>CancellationToken</returns>
		Task<IEnumerable<long>> SetCommonFuelRestriction(string cardId, string sessionId, string apiKey, CancellationToken cancellationToken);

		/// <summary>
		/// Установить тованый ограничитель по группе топлива<br/>
		/// Добавляется товарный ограничитель на группу топлива
		/// </summary>
		/// <param name="cardId">Id карты в газпроме</param>
		/// <param name="productGroupId">Id группы топливных продуктов</param>
		/// <param name="sessionId">Id сессии в газпроме</param>
		/// <param name="apiKey">Ключ АПИ в газпроме</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns></returns>
		Task<IEnumerable<long>> SetFuelProductGroupRestriction(string cardId, string productGroupId, string sessionId, string apiKey, CancellationToken cancellationToken);
	}
}
