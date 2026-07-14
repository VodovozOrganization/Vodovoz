using Vodovoz.Core.Data.InfoMessages;

namespace DeliveryRulesService.Factories
{
	public interface IInfoMessageFactory
	{
		/// <summary>
		/// Создание оповещения по платной доставке
		/// </summary>
		/// <param name="message">Сообщение</param>
		/// <returns></returns>
		InfoMessage CreatePaidDeliveryMessage(string message);
	}
}
