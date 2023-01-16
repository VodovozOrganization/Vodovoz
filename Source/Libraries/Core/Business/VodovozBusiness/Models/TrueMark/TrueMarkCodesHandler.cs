using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMarkApi.Library;
using TrueMarkApi.Library.Dto;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.TrueMark;

namespace Vodovoz.Models.TrueMark
{
	public class TrueMarkCodesHandler
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly TrueMarkTransactionalCodesPool _codePool;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly TrueMarkApiClient _trueMarkApiClient;

		public TrueMarkCodesHandler(IUnitOfWorkFactory uowFactory, TrueMarkApiClientFactory trueMarkApiClientFactory, TrueMarkTransactionalCodesPool codePool, ITrueMarkRepository trueMarkRepository)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_codePool = codePool ?? throw new ArgumentNullException(nameof(codePool));
			_trueMarkRepository = trueMarkRepository ?? throw new ArgumentNullException(nameof(trueMarkRepository));

			if(trueMarkApiClientFactory is null)
			{
				throw new ArgumentNullException(nameof(trueMarkApiClientFactory));
			}

			_trueMarkApiClient = trueMarkApiClientFactory.GetClient();
		}

		public async Task HandleOrders(CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var newCashReceiptOrders = _trueMarkRepository.GetNewCashReceiptOrders(uow);

				foreach(TrueMarkCashReceiptOrder newCashReceiptOrder in newCashReceiptOrders)
				{
					if(cancellationToken.IsCancellationRequested)
					{
						_codePool.Rollback();
						return;
					}

					await ProcessOrder(newCashReceiptOrder, cancellationToken);
					try
					{
						uow.Save(newCashReceiptOrder);
						uow.Commit();
						_codePool.Commit();
					}
					catch(Exception)
					{
						_codePool.Rollback();
					}
				}
			}
		}

		private async Task ProcessOrder(TrueMarkCashReceiptOrder trueMarkCashReceiptOrder, CancellationToken cancellationToken)
		{
			try
			{
				await ProcessOrderCodes(trueMarkCashReceiptOrder, cancellationToken);
				trueMarkCashReceiptOrder.Status = TrueMarkCashReceiptOrderStatus.ReadyToSend;
			}
			catch(Exception ex)
			{
				trueMarkCashReceiptOrder.Status = TrueMarkCashReceiptOrderStatus.Error;
				trueMarkCashReceiptOrder.ErrorDescription = ex.Message;
				throw;
			}
		}

		private async Task ProcessOrderCodes(TrueMarkCashReceiptOrder trueMarkCashReceiptOrder, CancellationToken cancellationToken)
		{
			var defectiveCodes = trueMarkCashReceiptOrder.ScannedCodes.Where(x => x.IsDefectiveSourceCode);
			ProcessDefectiveCodes(defectiveCodes);

			var goodCodes = trueMarkCashReceiptOrder.ScannedCodes.Where(x => !x.IsDefectiveSourceCode);

			var order = trueMarkCashReceiptOrder.Order;
			if(order.Client.PersonType == PersonType.legal)
			{
				ProcessLegalCounterparty(order, goodCodes);
			}
			else
			{
				await ProcessNaturalCounterparty(order, goodCodes, cancellationToken);
			}
		}

		private void ProcessDefectiveCodes(IEnumerable<TrueMarkCashReceiptProductCode> defectiveCodeEntities)
		{
			foreach(var codeEntity in defectiveCodeEntities)
			{
				_codePool.PutDefectiveCode(codeEntity.CodeSource);
				var newCode = _codePool.TakeCode();
				codeEntity.CodeResult = newCode;
			}
		}

		private void ProcessLegalCounterparty(Order order, IEnumerable<TrueMarkCashReceiptProductCode> goodCodeEntities)
		{
			if(order.Client.ReasonForLeaving != ReasonForLeaving.ForOwnNeeds)
			{
				return;
			}

			foreach(var codeEntity in goodCodeEntities)
			{
				_codePool.PutCode(codeEntity.CodeSource);
			}
		}

		private async Task ProcessNaturalCounterparty(Order order, IEnumerable<TrueMarkCashReceiptProductCode> goodCodeEntities, CancellationToken cancellationToken)
		{

			if(order.Client.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
			{
				var productCodes = goodCodeEntities.Select(x => x.CodeSource);
				var productInstancesInfo = await _trueMarkApiClient.GetProductInstanceInfoAsync(productCodes, cancellationToken);
				if(!string.IsNullOrWhiteSpace(productInstancesInfo.ErrorMessage))
				{
					throw new TrueMarkException($"Не удалось получить информацию о состоянии товаров в системе Честный знак. Подробности: {productInstancesInfo.ErrorMessage}");
				}

				foreach(var instanceStatus in productInstancesInfo.InstanceStatuses)
				{
					var goodCodeEntity = goodCodeEntities.SingleOrDefault(x => x.CodeSource == instanceStatus.IdentificationCode);
					if(goodCodeEntity == null)
					{
						throw new TrueMarkException("Проверенный в системе Честный знак, код не был найден среди отправленных на проверку.");
					}

					if(instanceStatus.Status != ProductInstanceStatusEnum.Introduced)
					{
						goodCodeEntity.CodeResult = _codePool.TakeCode();
					}
					else
					{
						goodCodeEntity.CodeResult = goodCodeEntity.CodeSource;
					}
				}
			}
			else
			{
				foreach(var codeEntity in goodCodeEntities)
				{
					codeEntity.CodeResult = codeEntity.CodeSource;
				}
			}
		}
	}
}
