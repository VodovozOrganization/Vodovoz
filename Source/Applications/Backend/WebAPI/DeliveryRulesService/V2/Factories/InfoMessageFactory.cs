using Vodovoz.Core.Data.InfoMessages;

namespace DeliveryRulesService.Factories
{
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
