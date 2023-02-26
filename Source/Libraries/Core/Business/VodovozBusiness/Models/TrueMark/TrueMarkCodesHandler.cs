using Microsoft.Extensions.Logging;
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
		private readonly ILogger<TrueMarkSelfDeliveriesHandler> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly TrueMarkWaterCodeParser _trueMarkWaterCodeParser;
		private readonly TrueMarkTransactionalCodesPool _codePool;
		private readonly ITrueMarkRepository _trueMarkRepository;
		private readonly TrueMarkApiClient _trueMarkApiClient;

		public TrueMarkCodesHandler(
			ILogger<TrueMarkSelfDeliveriesHandler> logger,
			IUnitOfWorkFactory uowFactory,
			TrueMarkApiClientFactory trueMarkApiClientFactory, 
			TrueMarkWaterCodeParser trueMarkWaterCodeParser, 
			TrueMarkTransactionalCodesPool codePool, 
			ITrueMarkRepository trueMarkRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_trueMarkWaterCodeParser = trueMarkWaterCodeParser ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeParser));
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
			var trueMarkOrderIds = GetTrueMarkOrderIds();

			foreach(var trueMarkOrderId in trueMarkOrderIds)
			{
				if(cancellationToken.IsCancellationRequested)
				{
					_codePool.Rollback();
					return;
				}

				try
				{
					await ProcessOrder(trueMarkOrderId, cancellationToken);
				}
				catch(Exception ex)
				{
					RegisterException(trueMarkOrderId, ex);
				}
			}
		}

		private IEnumerable<int> GetTrueMarkOrderIds()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var newCashReceiptOrderIds = _trueMarkRepository.GetNewCashReceiptOrderIds(uow);
				return newCashReceiptOrderIds;
			}
		}

		private async Task ProcessOrder(int trueMarkOrderId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var trueMarkOrder = uow.GetById<TrueMarkCashReceiptOrder>(trueMarkOrderId);
				await ProcessOrderCodes(uow, trueMarkOrder, cancellationToken);

				try
				{
					uow.Save(trueMarkOrder);
					uow.Commit();
				}
				catch(Exception)
				{
					_codePool.Rollback();
					throw;
				}

				//не мешаем сохранению сущности, ошибка пула кода не важна если сущность сохранилась
				try
				{
					_codePool.Commit();
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка коммита пула кодов.");
				}
			}
		}

		private void RegisterException(int trueMarkOrderId, Exception ex)
		{
			_logger.LogError(ex, $"Ошибка обработки заказа честного знака {trueMarkOrderId}.");
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var trueMarkOrder = uow.GetById<TrueMarkCashReceiptOrder>(trueMarkOrderId);
				trueMarkOrder.Status = TrueMarkCashReceiptOrderStatus.CodeError;
				trueMarkOrder.ErrorDescription = ex.Message;
				uow.Save(trueMarkOrder);
				uow.Commit();
			}
		}

		private async Task ProcessOrderCodes(IUnitOfWork uow, TrueMarkCashReceiptOrder trueMarkCashReceiptOrder, CancellationToken cancellationToken)
		{
			var codeEntities = trueMarkCashReceiptOrder.ScannedCodes;

			ProcessDefectiveCodes(uow, codeEntities);

			var order = trueMarkCashReceiptOrder.Order;
			if(order.Client.PersonType == PersonType.legal)
			{
				ProcessLegalCounterparty(order, codeEntities);
				trueMarkCashReceiptOrder.Status = TrueMarkCashReceiptOrderStatus.ReceiptNotNeeded;
			}
			else
			{
				await ProcessNaturalCounterparty(uow, order, codeEntities, cancellationToken);
				trueMarkCashReceiptOrder.Status = TrueMarkCashReceiptOrderStatus.ReadyToSend;
			}

			trueMarkCashReceiptOrder.ErrorDescription = null;
		}

		private void ProcessDefectiveCodes(IUnitOfWork uow, IEnumerable<TrueMarkCashReceiptProductCode> codeEntities)
		{
			foreach(var codeEntity in codeEntities.Where(x => x.IsDefectiveSourceCode))
			{
				if(!codeEntity.SourceCode.IsInvalid && codeEntity.SourceCode != null)
				{
					_codePool.PutDefectiveCode(codeEntity.SourceCode.Id);
				}

				codeEntity.ResultCode = GetCodeFromPool(uow);

				uow.Save(codeEntity);
			}
		}

		private void ProcessLegalCounterparty(Order order, IEnumerable<TrueMarkCashReceiptProductCode> codeEntities)
		{
			if(order.Client.ReasonForLeaving != ReasonForLeaving.ForOwnNeeds)
			{
				return;
			}

			var goodCodeEntities = codeEntities.Where(x => !x.IsDefectiveSourceCode);

			foreach(var goodCodeEntity in goodCodeEntities)
			{
				if(goodCodeEntity.SourceCode == null || goodCodeEntity.SourceCode.IsInvalid)
				{
					continue;
				}

				_codePool.PutCode(goodCodeEntity.SourceCode.Id);
			}
		}

		private async Task ProcessNaturalCounterparty(IUnitOfWork uow, Order order, IEnumerable<TrueMarkCashReceiptProductCode> codeEntities, CancellationToken cancellationToken)
		{
			var goodCodeEntities = codeEntities.Where(x => !x.IsDefectiveSourceCode);

			if(order.Client.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
			{
				var goodValidCodes = goodCodeEntities.Where(x => x.SourceCode != null && !x.SourceCode.IsInvalid);

				var productCodes = goodValidCodes
					.ToDictionary(x => _trueMarkWaterCodeParser.GetWaterIdentificationCode(x.SourceCode));

				var productInstancesInfo = await _trueMarkApiClient.GetProductInstanceInfoAsync(productCodes.Keys, cancellationToken);
				if(!string.IsNullOrWhiteSpace(productInstancesInfo.ErrorMessage))
				{
					throw new TrueMarkException($"Не удалось получить информацию о состоянии товаров в системе Честный знак. Подробности: {productInstancesInfo.ErrorMessage}");
				}

				foreach(var instanceStatus in productInstancesInfo.InstanceStatuses)
				{
					var codeEntity = productCodes[instanceStatus.IdentificationCode];
					if(codeEntity == null)
					{
						throw new TrueMarkException("Проверенный в системе Честный знак, код не был найден среди отправленных на проверку.");
					}

					if(instanceStatus.Status == ProductInstanceStatusEnum.Introduced)
					{
						codeEntity.ResultCode = codeEntity.SourceCode;
					}
					else
					{
						codeEntity.ResultCode = GetCodeFromPool(uow);
					}
				}

				var goodInvalidCodes = codeEntities.Where(x => x.SourceCode == null || x.SourceCode.IsInvalid);
				foreach(var goodInvalidCode in goodInvalidCodes)
				{
					goodInvalidCode.ResultCode = GetCodeFromPool(uow);
				}
			}
			else
			{
				//Для первой реаизации решено бракованные коды заменять на коды из пула
				//в будущем необходимо будет придумать как оперативно работать с бракованными
				//бутылями чтобы в чек отправлялись только те бутыли что получил клиент
				foreach(var goodCodeEntity in goodCodeEntities)
				{
					if(goodCodeEntity.SourceCode == null || goodCodeEntity.SourceCode.IsInvalid)
					{
						goodCodeEntity.ResultCode = GetCodeFromPool(uow);
					}
					else
					{
						goodCodeEntity.ResultCode = goodCodeEntity.SourceCode;
					}
				}
			}
		}

		private TrueMarkWaterIdentificationCode GetCodeFromPool(IUnitOfWork uow)
		{
			var codeId = _codePool.TakeCode();
			return uow.GetById<TrueMarkWaterIdentificationCode>(codeId);
		}
	}
}
