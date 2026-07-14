using Vodovoz.Core.Data.InfoMessages;

namespace DeliveryRulesService.Factories
{
	public class InfoMessageFactory : IInfoMessageFactory
	{
		/// <summary>
		/// Создание сообщения о платной доставке
		/// </summary>
		/// <param name="message">Сообщение</param>
		/// <returns></returns>
		public InfoMessage CreatePaidDeliveryMessage(string message)
		{
			if(string.IsNullOrEmpty(message))
			{
				return null;
			}
			
			return InfoMessage.Create(
				"BasketDeliverySchedule",
				2,
				"Платная доставка",
				message
				);
		}
	}
}
