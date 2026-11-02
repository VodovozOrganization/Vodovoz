using Vodovoz.Core.Data.InfoMessages;
using VodovozBusiness.Domain.Orders.Delivery;

namespace DeliveryRulesService.Factories
{
	public class InfoMessageFactory : IInfoMessageFactory
	{
		/// <inheritdoc/>
		public InfoMessage CreateDeliveryMessage(IDeliveryCostData deliveryCostData)
		{
			if(string.IsNullOrEmpty(deliveryCostData.Message))
			{
				return CreateFreeDeliveryMessage(
					ProgressBarInfo.Create(
						deliveryCostData.MaxVolumeTotalBottles.Current,
						deliveryCostData.MaxVolumeTotalBottles.Max)
					);
			}
			
			return CreatePaidDeliveryMessage(
				deliveryCostData.Message,
				ProgressBarInfo.Create(
					deliveryCostData.MaxVolumeTotalBottles.Current,
					deliveryCostData.MaxVolumeTotalBottles.Max)
				);
		}
		
		private InfoMessage CreatePaidDeliveryMessage(string message, ProgressBarInfo progressBarInfo)
		{
			if(string.IsNullOrEmpty(message))
			{
				return null;
			}
			
			return InfoMessage.Create(
				"BasketDeliverySchedule",
				5,
				"Платная доставка",
				message,
				progressBarInfo
				);
		}
		
		private InfoMessage CreateFreeDeliveryMessage(ProgressBarInfo progressBarInfo)
		{
			return InfoMessage.Create(
				"BasketDeliverySchedule",
				6,
				"Бесплатная доставка ;)",
				null,
				progressBarInfo
			);
		}
	}
}
