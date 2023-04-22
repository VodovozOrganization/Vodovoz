using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Factories;

namespace Vodovoz.Models.TrueMark
{
	public class DeliveryOrderReceiptCreator : OrderReceiptCreator
	{
		public DeliveryOrderReceiptCreator(
			ILogger<OrderReceiptCreator> logger,
			IUnitOfWorkFactory uowFactory,
			ICashReceiptRepository cashReceiptRepository, 
			TrueMarkTransactionalCodesPool codesPool,
			ICashReceiptFactory cashReceiptFactory,
			int orderId) : base(logger, uowFactory, cashReceiptRepository, codesPool, cashReceiptFactory, orderId)
		{
		}
	}
}
