using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Models.TrueMark
{
	public class ReceiptPreparerFactory
	{
		private readonly ILogger<ReceiptPreparer> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly TrueMarkCodesChecker _codeChecker;
		private readonly ICashReceiptRepository _cashReceiptRepository;

		public ReceiptPreparerFactory(
			ILogger<ReceiptPreparer> logger, 
			IUnitOfWorkFactory uowFactory, 
			TrueMarkCodesChecker codeChecker,
			ICashReceiptRepository cashReceiptRepository
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_codeChecker = codeChecker ?? throw new ArgumentNullException(nameof(codeChecker));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
		}

		public ReceiptPreparer Create(int receiptId)
		{
			var codePool = new TrueMarkTransactionalCodesPool(_uowFactory);
			var preparer = new ReceiptPreparer(_logger, _uowFactory, codePool, _codeChecker, _cashReceiptRepository, receiptId);
			return preparer;
		}
	}
}
