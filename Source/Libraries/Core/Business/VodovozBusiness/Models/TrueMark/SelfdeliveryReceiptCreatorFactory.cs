using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Models.TrueMark
{
	public class SelfdeliveryReceiptCreatorFactory
	{
		private readonly ILogger<SelfdeliveryReceiptCreator> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ICashReceiptRepository _cashReceiptRepository;

		public SelfdeliveryReceiptCreatorFactory(ILogger<SelfdeliveryReceiptCreator> logger, IUnitOfWorkFactory uowFactory, ICashReceiptRepository cashReceiptRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
		}

		public SelfdeliveryReceiptCreator Create(int orderId)
		{
			var codesPool = new TrueMarkTransactionalCodesPool(_uowFactory);
			var creator = new SelfdeliveryReceiptCreator(_logger, _uowFactory, _cashReceiptRepository, codesPool, orderId);
			return creator;
		}
	}
}
