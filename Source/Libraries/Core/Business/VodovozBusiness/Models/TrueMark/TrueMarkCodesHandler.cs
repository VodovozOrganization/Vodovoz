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
					_codePool.Rollback();
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

				uow.Save(trueMarkOrder);
				uow.Commit();

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
				await ProcessLegalCounterparty(uow, order, trueMarkCashReceiptOrder, codeEntities, cancellationToken);
			}
			else
			{
				await ProcessNaturalCounterparty(uow, order, codeEntities, cancellationToken);
				CreateUnscannedCodes(uow, trueMarkCashReceiptOrder);
				trueMarkCashReceiptOrder.Status = TrueMarkCashReceiptOrderStatus.ReadyToSend;
			}

			trueMarkCashReceiptOrder.ErrorDescription = null;
		}

		private void ProcessDefectiveCodes(IUnitOfWork uow, IEnumerable<TrueMarkCashReceiptProductCode> codeEntities)
		{
			foreach(var codeEntity in codeEntities.Where(x => x.IsDefectiveSourceCode))
			{
				if(codeEntity.SourceCode != null && !codeEntity.SourceCode.IsInvalid)
				{
					_codePool.PutDefectiveCode(codeEntity.SourceCode.Id);
				}

				codeEntity.ResultCode = GetCodeFromPool(uow);

				uow.Save(codeEntity);
			}
		}

		private async Task ProcessLegalCounterparty(IUnitOfWork uow, Order order, TrueMarkCashReceiptOrder trueMarkCashReceiptOrder, IEnumerable<TrueMarkCashReceiptProductCode> codeEntities, CancellationToken cancellationToken)
		{
			if(order.Client.ReasonForLeaving != ReasonForLeaving.ForOwnNeeds)
			{
				trueMarkCashReceiptOrder.Status = TrueMarkCashReceiptOrderStatus.ReceiptNotNeeded;
				return;
			}

			if(order.Client.AlwaysSendReceipts)
			{
				var goodCodeEntities = codeEntities.Where(x => !x.IsDefectiveSourceCode);
				var goodValidCodes = goodCodeEntities.Where(x => x.SourceCode != null && !x.SourceCode.IsInvalid);

				if(goodValidCodes.Any())
				{
					var productCodes = goodValidCodes
						.ToDictionary(x => _trueMarkWaterCodeParser.GetWaterIdentificationCode(x.SourceCode));

					var productInstancesInfo = await _trueMarkApiClient.GetProductInstanceInfoAsync(productCodes.Keys, cancellationToken);
					if(!string.IsNullOrWhiteSpace(productInstancesInfo.ErrorMessage))
					{
						throw new TrueMarkException($"Не удалось получить информацию о состоянии товаров в системе Честный знак. Подробности: {productInstancesInfo.ErrorMessage}");
					}

					foreach(var instanceStatus in productInstancesInfo.InstanceStatuses)
					{
						var codeFound = productCodes.TryGetValue(instanceStatus.IdentificationCode, out TrueMarkCashReceiptProductCode codeEntity);
						if(!codeFound)
						{
							throw new TrueMarkException($"Проверенный в системе Честный знак, код ({instanceStatus.IdentificationCode}) не был найден среди отправленных на проверку.");
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
				}

				var goodInvalidCodes = codeEntities.Where(x => x.SourceCode == null || x.SourceCode.IsInvalid);
				foreach(var goodInvalidCode in goodInvalidCodes)
				{
					goodInvalidCode.ResultCode = GetCodeFromPool(uow);
				}

				CreateUnscannedCodes(uow, trueMarkCashReceiptOrder);
				trueMarkCashReceiptOrder.Status = TrueMarkCashReceiptOrderStatus.ReadyToSend;
			}
			else
			{
				var goodCodeEntities = codeEntities.Where(x => !x.IsDefectiveSourceCode);
				foreach(var goodCodeEntity in goodCodeEntities)
				{
					if(goodCodeEntity.SourceCode == null || goodCodeEntity.SourceCode.IsInvalid)
					{
						continue;
					}

					_codePool.PutCode(goodCodeEntity.SourceCode.Id);
				}

				trueMarkCashReceiptOrder.Status = TrueMarkCashReceiptOrderStatus.ReceiptNotNeeded;
			}
		}

		private async Task ProcessNaturalCounterparty(IUnitOfWork uow, Order order, IEnumerable<TrueMarkCashReceiptProductCode> codeEntities, CancellationToken cancellationToken)
		{
			var goodCodeEntities = codeEntities.Where(x => !x.IsDefectiveSourceCode);

			if(order.Client.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds)
			{
				var goodValidCodes = goodCodeEntities.Where(x => x.SourceCode != null && !x.SourceCode.IsInvalid);

				if(goodValidCodes.Any())
				{
					var productCodes = goodValidCodes
						.ToDictionary(x => _trueMarkWaterCodeParser.GetWaterIdentificationCode(x.SourceCode));

					var productInstancesInfo = await _trueMarkApiClient.GetProductInstanceInfoAsync(productCodes.Keys, cancellationToken);
					if(!string.IsNullOrWhiteSpace(productInstancesInfo.ErrorMessage))
					{
						throw new TrueMarkException($"Не удалось получить информацию о состоянии товаров в системе Честный знак. Подробности: {productInstancesInfo.ErrorMessage}");
					}

					foreach(var instanceStatus in productInstancesInfo.InstanceStatuses)
					{

						var codeFound = productCodes.TryGetValue(instanceStatus.IdentificationCode, out TrueMarkCashReceiptProductCode codeEntity);
						if(!codeFound)
						{
							throw new TrueMarkException($"Проверенный в системе Честный знак, код ({instanceStatus.IdentificationCode}) не был найден среди отправленных на проверку.");
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

		private void CreateUnscannedCodes(IUnitOfWork uow, TrueMarkCashReceiptOrder trueMarkCashReceiptOrder)
		{
			var orderItems = trueMarkCashReceiptOrder.Order.OrderItems.Where(x => x.Nomenclature.IsAccountableInTrueMark);

			foreach(var orderItem in orderItems)
			{
				var codes = trueMarkCashReceiptOrder.ScannedCodes.Where(x => x.OrderItem.Id == orderItem.Id);
				var unscannedCodesCount = orderItem.Count - codes.Count();
				if(unscannedCodesCount < 0)
				{
					var extraCodes = codes.Skip((int)orderItem.Count);
					foreach(var extraCode in extraCodes.ToList())
					{
						if(extraCode.SourceCode == null || extraCode.SourceCode.IsInvalid)
						{
							continue;
						}

						_codePool.PutCode(extraCode.SourceCode.Id);
						trueMarkCashReceiptOrder.ScannedCodes.Remove(extraCode);
						uow.Delete(extraCode);
					}
				}

				if(unscannedCodesCount == 0)
				{
					continue;
				}

				for(int i = 0; i < unscannedCodesCount; i++)
				{
					var newCode = new TrueMarkCashReceiptProductCode();
					newCode.TrueMarkCashReceiptOrder = trueMarkCashReceiptOrder;
					newCode.OrderItem = orderItem;
					newCode.IsUnscannedSourceCode = true;
					newCode.ResultCode = GetCodeFromPool(uow);

					uow.Save(newCode);
					trueMarkCashReceiptOrder.ScannedCodes.Add(newCode);
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
