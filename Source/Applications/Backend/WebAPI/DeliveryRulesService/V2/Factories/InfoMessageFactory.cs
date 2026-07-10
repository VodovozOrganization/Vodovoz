using Vodovoz.Core.Data.InfoMessages;

namespace DeliveryRulesService.Factories
{
	//TODO по результатам ответа Кости оставить и использовать или удалить класс
	public class InfoMessageFactory
	{
		public InfoMessage CreatePaidDeliveryMessage(string message)
		{
			return InfoMessage.Create(
				"BasketDeliverySchedule",
				2,
				"Платная доставка",
				message//"Добавьте в заказ 1 бутыль 19 литров, чтобы доставка была бесплатной"
				);
		}
	}
}
