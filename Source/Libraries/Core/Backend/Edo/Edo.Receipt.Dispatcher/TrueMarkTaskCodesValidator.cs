using Core.Infrastructure;
using Edo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Receipt.Dispatcher
{
	public class TrueMarkTaskCodesValidator
	{
		private readonly IEdoRepository _edoRepository;

		public TrueMarkTaskCodesValidator(IEdoRepository edoRepository)
		{
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
		}

		public async Task<TrueMarkTaskValidationResult> ValidateAsync(
			OrderEdoTask edoTask, 
			EdoTaskItemTrueMarkStatusProvider edoTaskItemTrueMarkStatusProvider, 
			CancellationToken cancellationToken)
		{
			var gtins = await _edoRepository.GetGtinsAsync(cancellationToken);
			var gtinNumbers = gtins.Select(x => x.GtinNumber);

			var edoOrganizations = await _edoRepository.GetEdoOrganizationsAsync(cancellationToken);
			var ourOrganizationInns = edoOrganizations.Select(x => x.INN);
			var sellerInn = edoTask.OrderEdoRequest.Order.Contract.Organization.INN;

			var checkResults = await edoTaskItemTrueMarkStatusProvider.GetItemsStatusesAsync(cancellationToken);
			var codeResults = new List<TrueMarkCodeValidationResult>();

			foreach(var receiptItem in edoTask.Items)
			{
				var code = receiptItem.ProductCode.ResultCode;
				var codeResult = new TrueMarkCodeValidationResult(code);

				if(!checkResults.TryGetValue(code.IdentificationCode, out var checkResult))
				{
					throw new InvalidOperationException($"Не найден код {code.IdentificationCode} " +
						$"из задачи {edoTask.Id} в результатах проверки из ЧЗ.");
				}

				// проверка на наш GTIN
				if(code.GTIN.IsNotIn(gtinNumbers))
				{
					codeResult.IsOurGtin = false;
					codeResult.IsValid = false;
					codeResult.ReadyToSell = false;
				}

				// проверка на нашу организацию
				if(checkResult.ProductInstanceStatus.OwnerInn.IsNotIn(ourOrganizationInns))
				{
					codeResult.IsOwnedByOurOrganization = false;
					codeResult.IsValid = false;
					codeResult.ReadyToSell = false;
				}

				// проверка на то что код в обороте
				if(checkResult.ProductInstanceStatus.Status == ProductInstanceStatusEnum.Introduced)
				{
					codeResult.IsIntroduced = false;
					codeResult.IsValid = false;
					codeResult.ReadyToSell = false;
				}

				// проверка на то что код на балансе продавца
				if(checkResult.ProductInstanceStatus.OwnerInn == sellerInn)
				{
					// не влияет на валидность, просто дополнительная информация
					codeResult.IsOwnedBySeller = false;
					codeResult.ReadyToSell = false;
				}

				codeResults.Add(codeResult);
			}

			return new TrueMarkTaskValidationResult(codeResults);
		}
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
