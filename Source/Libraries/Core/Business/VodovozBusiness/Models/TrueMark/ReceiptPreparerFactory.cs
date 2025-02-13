using System;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Factories;
using VodovozBusiness.Models.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	public class ReceiptPreparerFactory
	{
		private readonly ILogger<ReceiptPreparer> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly TrueMarkCodesChecker _codeChecker;
		private readonly ICashReceiptRepository _cashReceiptRepository;
		private readonly ICashReceiptFactory _cashReceiptFactory;
		private readonly OurCodesChecker _ourCodesChecker;
		private readonly Tag1260Updater _tag1260Updater;

		public ReceiptPreparerFactory(
			ILogger<ReceiptPreparer> logger,
			IUnitOfWorkFactory uowFactory,
			TrueMarkCodesChecker codeChecker,
			ICashReceiptRepository cashReceiptRepository,
			ICashReceiptFactory cashReceiptFactory,
			OurCodesChecker ourCodesChecker,
			Tag1260Updater tag1260Updater
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_codeChecker = codeChecker ?? throw new ArgumentNullException(nameof(codeChecker));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
			_cashReceiptFactory = cashReceiptFactory ?? throw new ArgumentNullException(nameof(cashReceiptFactory));
			_ourCodesChecker = ourCodesChecker ?? throw new ArgumentNullException(nameof(ourCodesChecker));
			_tag1260Updater = tag1260Updater ?? throw new ArgumentNullException(nameof(tag1260Updater));
		}

		public ReceiptPreparer Create(int receiptId)
		{
			var codePool = new TrueMarkTransactionalCodesPool(_uowFactory);
			var preparer = new ReceiptPreparer(
				_logger, _uowFactory, codePool, _codeChecker, _cashReceiptRepository, _cashReceiptFactory,
				_ourCodesChecker, _tag1260Updater, receiptId);
			return preparer;
		}
	}
}
