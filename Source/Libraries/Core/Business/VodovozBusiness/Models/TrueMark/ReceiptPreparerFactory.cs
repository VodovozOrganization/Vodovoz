using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Services;

namespace Vodovoz.Models.TrueMark
{
	public class ReceiptPreparerFactory
	{
		private readonly ILogger<ReceiptPreparer> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly TrueMarkCodesChecker _codeChecker;
		private readonly IOrderParametersProvider _orderParametersProvider;

		public ReceiptPreparerFactory(
			ILogger<ReceiptPreparer> logger, 
			IUnitOfWorkFactory uowFactory, 
			TrueMarkCodesChecker codeChecker, 
			IOrderParametersProvider orderParametersProvider
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_codeChecker = codeChecker ?? throw new ArgumentNullException(nameof(codeChecker));
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
		}

		public ReceiptPreparer Create(int receiptId)
		{
			var codePool = new TrueMarkTransactionalCodesPool(_uowFactory);
			var cashReceiptRepository = new CashReceiptRepository(_orderParametersProvider);
			var preparer = new ReceiptPreparer(_logger, _uowFactory, codePool, _codeChecker, cashReceiptRepository, receiptId);
			return preparer;
		}
	}
}
