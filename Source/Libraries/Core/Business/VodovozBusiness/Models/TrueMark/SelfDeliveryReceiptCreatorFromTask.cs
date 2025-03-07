using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Factories;
using Vodovoz.Models.TrueMark;

namespace VodovozBusiness.Models.TrueMark
{
	public class SelfDeliveryReceiptCreatorFromTask : OrderReceiptCreatorFromTask
	{
		public SelfDeliveryReceiptCreatorFromTask(
			ILogger<SelfDeliveryReceiptCreatorFromTask> logger,
			IUnitOfWorkFactory uowFactory,
			TrueMarkTransactionalCodesPool codesPool,
			ICashReceiptFactory cashReceiptFactory) : base(logger, uowFactory, codesPool, cashReceiptFactory)
		{
		}
	}
}
