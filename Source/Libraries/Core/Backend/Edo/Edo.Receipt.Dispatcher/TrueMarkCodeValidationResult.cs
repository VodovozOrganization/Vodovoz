using System;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Receipt.Dispatcher
{
	public class TrueMarkCodeValidationResult
	{
		public TrueMarkCodeValidationResult(TrueMarkWaterIdentificationCode code)
		{
			Code = code ?? throw new ArgumentNullException(nameof(code));
		}

		public bool IsValid { get; set; } = true;
		public bool ReadyToSell { get; set; } = true;
		public TrueMarkWaterIdentificationCode Code { get; set; }
		public bool IsOwnedByOurOrganization { get; set; } = true;
		public bool IsOurGtin { get; set; } = true;
		public bool IsIntroduced { get; set; } = true;
		public bool IsOwnedBySeller { get; set; } = true;
	}



	//public class ResaleReceiptEdoTaskHandler
	//{
	//	private readonly IUnitOfWorkFactory _uowFactory;
	//	private readonly EdoTaskMainValidator _validator;
	//	private readonly EdoTaskItemTrueMarkStatusProviderFactory _edoTaskTrueMarkCodeCheckerFactory;
	//	private readonly TransferRequestCreator _transferRequestCreator;
	//	private readonly IBus _messageBus;
	//	private readonly IUnitOfWork _uow;

	//	public ResaleReceiptEdoTaskHandler(
	//		IUnitOfWorkFactory uowFactory,
	//		EdoTaskMainValidator validator,
	//		EdoTaskItemTrueMarkStatusProviderFactory edoTaskTrueMarkCodeCheckerFactory,
	//		TransferRequestCreator transferRequestCreator,
	//		IBus messageBus
	//		)
	//	{
	//		_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
	//		_validator = validator ?? throw new ArgumentNullException(nameof(validator));
	//		_edoTaskTrueMarkCodeCheckerFactory = edoTaskTrueMarkCodeCheckerFactory ?? throw new ArgumentNullException(nameof(edoTaskTrueMarkCodeCheckerFactory));
	//		_transferRequestCreator = transferRequestCreator ?? throw new ArgumentNullException(nameof(transferRequestCreator));
	//		_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
	//		_uow = uowFactory.CreateWithoutRoot();
	//	}

	//	// handle new
	//	// Entry stage: New
	//	// Validated stage: New
	//	// Changed to: Transfering, Sending
	//	// [событие от scheduler]
	//	// (проверяет нужен ли перенос, или сразу отправляет)
	//	public async Task HandleResaleReceipt(int receiptEdoTaskId, CancellationToken cancellationToken)
	//	{
	//		var edoTask = await _uow.Session.GetAsync<ReceiptEdoTask>(receiptEdoTaskId, cancellationToken);
	//		// TEST
	//		// проверяем все коды как МН
	//		var trueMarkApiClient = new TrueMarkApiClient("https://test-mn-truemarkapi.dev.vod.qsolution.ru/", "test");
	//		var trueMarkCodeChecker = _edoTaskTrueMarkCodeCheckerFactory.Create(edoTask, trueMarkApiClient);

	//		var valid = await Validate(edoTask, trueMarkCodeChecker, cancellationToken);
	//		if(!valid)
	//		{
	//			await _uow.CommitAsync(cancellationToken);
	//			return;
	//		}

	//		object message = null;

	//		// Определя
	//	}
	//}
}
