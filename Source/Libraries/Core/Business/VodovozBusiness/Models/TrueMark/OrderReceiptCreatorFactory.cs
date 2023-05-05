using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Factories;

namespace Vodovoz.Models.TrueMark
{
	public class OrderReceiptCreatorFactory
	{
		private readonly ILogger<OrderReceiptCreator> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ICashReceiptRepository _cashReceiptRepository;
		private readonly ICashReceiptFactory _cashReceiptFactory;

		public OrderReceiptCreatorFactory(
			ILogger<OrderReceiptCreator> logger,
			IUnitOfWorkFactory uowFactory,
			ICashReceiptRepository cashReceiptRepository,
			ICashReceiptFactory cashReceiptFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
			_cashReceiptFactory = cashReceiptFactory ?? throw new ArgumentNullException(nameof(cashReceiptFactory));
		}

		public OrderReceiptCreator CreateSelfDeliveryReceiptCreator(int orderId)
		{
			var codesPool = new TrueMarkTransactionalCodesPool(_uowFactory);
			var creator = new SelfdeliveryReceiptCreator(
				_logger, _uowFactory, _cashReceiptRepository, codesPool, _cashReceiptFactory, orderId);
			return creator;
		}
		
		public OrderReceiptCreator CreateDeliveryOrderReceiptCreator(int orderId)
		{
			var codesPool = new TrueMarkTransactionalCodesPool(_uowFactory);
			var creator = new DeliveryOrderReceiptCreator
				(_logger, _uowFactory, _cashReceiptRepository, codesPool, _cashReceiptFactory, orderId);
			return creator;
		}
	}
}
