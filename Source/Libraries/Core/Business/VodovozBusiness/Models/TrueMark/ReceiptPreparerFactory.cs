using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Factories;
using Vodovoz.Infrastructure.Persistance;
using VodovozBusiness.Models.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	public class ReceiptPreparerFactory
	{
		private readonly ILogger<ReceiptPreparer> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly TrueMarkCodesChecker _codeChecker;
		private readonly ICashReceiptRepository _cashReceiptRepository;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly ICashReceiptFactory _cashReceiptFactory;
		private readonly OurCodesChecker _ourCodesChecker;
		private readonly ITag1260Checker _tag1260Checker;

		public ReceiptPreparerFactory(
			ILogger<ReceiptPreparer> logger,
			IUnitOfWorkFactory uowFactory,
			TrueMarkCodesChecker codeChecker,
			ICashReceiptRepository cashReceiptRepository,
			ITrueMarkRepository trueMarkRepository,
			ICashReceiptFactory cashReceiptFactory,
			OurCodesChecker ourCodesChecker,
			ITag1260Checker tag1260Checker
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_codeChecker = codeChecker ?? throw new ArgumentNullException(nameof(codeChecker));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));
			_cashReceiptFactory = cashReceiptFactory ?? throw new ArgumentNullException(nameof(cashReceiptFactory));
			_ourCodesChecker = ourCodesChecker ?? throw new ArgumentNullException(nameof(ourCodesChecker));
			_tag1260Checker = tag1260Checker ?? throw new ArgumentNullException(nameof(tag1260Checker));
		}

		public ReceiptPreparer Create(int receiptId)
		{
			var codePool = new TrueMarkTransactionalCodesPool(_uowFactory);
			var preparer = new ReceiptPreparer(
				_logger, _uowFactory, codePool, _codeChecker, _cashReceiptRepository, _trueMarkRepository, _cashReceiptFactory,
				new GenericRepository<Nomenclature>(), _ourCodesChecker, _tag1260Checker, receiptId);
			return preparer;
		}
	}
}
