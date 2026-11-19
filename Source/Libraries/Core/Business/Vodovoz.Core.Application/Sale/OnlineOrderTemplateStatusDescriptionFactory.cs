using Vodovoz.Core.Domain.Sale;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class OnlineOrderTemplateStatusDescriptionFactory
	{
		public string GetTemplateStatusDescription(
			int onlineOrderId,
			PaymentData paymentData,
			OnlineOrderTemplateStatus templateStatus
			)
		{
			switch(templateStatus)
			{
				case OnlineOrderTemplateStatus.PaymentProblem:
					if(paymentData.Card is null)
					{
						return OnlineOrderTemplateStatusDescription.PaymentProblemDescription.CardNotLinked;
					}
					
					if(paymentData.NotEnoughFunds)
					{
						return OnlineOrderTemplateStatusDescription.PaymentProblemDescription.NotEnoughFunds;
					}

					return null;
				case OnlineOrderTemplateStatus.ManualProcessing:
					return OnlineOrderTemplateStatusDescription.ManualProcessingDescription.AutoOrderUnPaid(onlineOrderId);
				default:
					return null;
			}
		}
	}

	public class PaymentData
	{
		public string Card { get; set; }
		public bool NotEnoughFunds { get; set; }
	}
}
