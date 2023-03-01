using NLog;
using QS.DomainModel.UoW;
using QS.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.TrueMark;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Models.TrueMark;
using Vodovoz.Services;
using VodovozSalesReceiptsService.DTO;

namespace VodovozSalesReceiptsService
{
	/// <summary>
	/// Класс автоматической отправки чеков для заказов
	/// </summary>
	public class FiscalizationWorker
	{
		public FiscalizationWorker(
			IOrderRepository orderRepository,
			ISalesReceiptSender salesReceiptSender,
			IOrderParametersProvider orderParametersProvider,
			IEnumerable<CashBox> cashBoxes)
		{
			this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			this.salesReceiptSender = salesReceiptSender ?? throw new ArgumentNullException(nameof(salesReceiptSender));
			this.orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
			this.cashBoxes = cashBoxes ?? throw new ArgumentNullException(nameof(cashBoxes));
		}

		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IOrderRepository orderRepository;
		private readonly ISalesReceiptSender salesReceiptSender;
		private readonly IOrderParametersProvider orderParametersProvider;
		private readonly IEnumerable<CashBox> cashBoxes;
		private readonly TimeSpan initialDelay = TimeSpan.FromSeconds(5);
		private readonly TimeSpan delay = TimeSpan.FromSeconds(45);

		/// <summary>
		/// Максимальное число чеков которое можно отправить <see cref="FiscalizationWorker"/> за один цикл 
		/// </summary>
		private readonly int maxReceiptsAllowedToSendInOneGo = 30;

		/// <summary>
		/// Запускает процесс автоматической отправки чеков
		/// </summary>
		public void Start()
		{
			Task.Run(() =>
			{
				Task.Delay(initialDelay).Wait();

				while(true)
				{
					try
					{
						PrepareAndSendReceipts();
					}
					catch(Exception ex)
					{
						logger.Error(ex);
					}

					Delay();
				}
			});
		}

		private void Delay()
		{
			if(DateTime.Now.Hour >= 1 && DateTime.Now.Hour < 5)
			{
				logger.Info("Ночь. Не пытаемся отсылать чеки с 1 до 5 утра.");
				var fiveHrsOfToday = DateTime.Today.AddHours(5);
				Task.Delay(fiveHrsOfToday.Subtract(DateTime.Now)).Wait();
			}
			else
			{
				logger.Info($"Ожидание {delay.Seconds} секунд перед отправкой чеков");
				Task.Delay(delay).Wait();
			}
		}

		private void PrepareAndSendReceipts()
		{
			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				logger.Info("Подготовка чеков к отправке на сервер фискализации...");

				var receiptForOrderNodes = orderRepository
					.GetOrdersForCashReceiptServiceToSend(uow, orderParametersProvider, DateTime.Today.AddDays(-3)).ToList();

				var receiptForLegalSelfdeliveriesNodes = orderRepository
					.GetLegalSelfdeliveriesForCashReceiptServiceToSend(uow, orderParametersProvider, DateTime.Today.AddDays(-3)).ToList();
				

				var withoutReceipts = receiptForOrderNodes.Where(r => r.ReceiptId == null).ToList();
				var withNotSentReceipts = receiptForOrderNodes.Where(r => r.ReceiptId.HasValue && r.WasSent != true).ToList();

				var withoutSelfdeliveriesReceipts = receiptForLegalSelfdeliveriesNodes.Where(r => r.ReceiptId == null).ToList();
				var withNotSentSelfdeliveriesReceipts = receiptForLegalSelfdeliveriesNodes.Where(r => r.ReceiptId.HasValue && r.WasSent != true).ToList();

				var receiptsToSend = withoutReceipts.Count + withNotSentReceipts.Count + withoutSelfdeliveriesReceipts.Count + withNotSentSelfdeliveriesReceipts.Count;
				if(receiptsToSend <= 0)
				{
					logger.Info("Нет чеков для отправки.");
					return;
				}
				logger.Info($"Общее количество чеков для отправки: {receiptsToSend}");

				int notValidCount = 0;
				SendForOrdersWithoutReceipts(uow, withoutReceipts, ref notValidCount);
				SendForOrdersWithNotSendReceipts(uow, withNotSentReceipts, ref notValidCount);

				//Отправка самовывозов для юрлиц по старой логике, не нужна будет после запуска сканирования кодов на складе
				SendForLegalSelfdeliveryWithoutReceipts(uow, withoutSelfdeliveriesReceipts, ref notValidCount);
				SendForLegalSelfdeliveryWithNotSendReceipts(uow, withoutSelfdeliveriesReceipts, ref notValidCount);

				uow.Commit();

				if(notValidCount > 0)
				{
					logger.Info($"{notValidCount} {NumberToTextRus.Case(notValidCount, "чек не валиден", "чека не валидно", "чеков не валидно")}.");
				}
			}
		}

		private void SendForOrdersWithoutReceipts(IUnitOfWork uow, IEnumerable<ReceiptForOrderNode> nodes, ref int notValidCount)
		{
			IList<PreparedReceiptNode> receiptNodesToSend = new List<PreparedReceiptNode>();
			var orders = uow.GetById<Order>(nodes.Select(x => x.OrderId));
			var trueMarkOrders = uow.GetById<TrueMarkCashReceiptOrder>(nodes.Select(x => x.TrueMarkCashReceiptOrderId));

			foreach(var node in nodes)
			{
				var order = uow.GetById<Order>(node.OrderId);
				var trueMarkOrder = uow.GetById<TrueMarkCashReceiptOrder>(node.TrueMarkCashReceiptOrderId);

				var newReceipt = new CashReceipt { Order = order };

				SalesDocumentDTO doc = null;
				try
				{
					doc = new SalesDocumentDTO(order, trueMarkOrder, order.Contract?.Organization?.ActiveOrganizationVersion?.Leader?.ShortName);
				}
				catch(TrueMarkException ex)
				{
					trueMarkOrder.ErrorDescription = ex.Message;
					trueMarkOrder.Status = TrueMarkCashReceiptOrderStatus.ReceiptSendError;
					uow.Save(trueMarkOrder);
					continue;
				}

				CashBox cashBox = null;
				if(order.Contract?.Organization?.CashBoxId != null)
				{
					cashBox = cashBoxes.FirstOrDefault(x => x.Id == order.Contract.Organization.CashBoxId);
				}

				if(doc.IsValid && cashBox != null)
				{

					var newPreparedReceiptNode = new PreparedReceiptNode
					{
						CashReceipt = newReceipt,
						SalesDocumentDTO = doc,
						CashBox = cashBox,
						TrueMarkCashReceiptOrder = trueMarkOrder
					};

					receiptNodesToSend.Add(newPreparedReceiptNode);

					if(receiptNodesToSend.Count >= maxReceiptsAllowedToSendInOneGo)
					{
						break;
					}
				}
				else
				{
					if(cashBox == null)
					{
						logger.Warn($"Для заказа №{order.Id} не удалось подобрать кассовый аппарат для отправки. Пропускаю");
					}

					trueMarkOrder.Status = TrueMarkCashReceiptOrderStatus.ReceiptSendError;
					trueMarkOrder.ErrorDescription = $"Для заказа №{order.Id} не удалось подобрать кассовый аппарат для отправки.";
					trueMarkOrder.CashReceipt = newReceipt;

					notValidCount++;
					newReceipt.HttpCode = 0;
					newReceipt.Sent = false;
					uow.Save(newReceipt);
					uow.Save(trueMarkOrder);
				}
			}

			logger.Info($"Количество новых чеков для отправки: {receiptNodesToSend.Count} (Макс.: {maxReceiptsAllowedToSendInOneGo})");
			if(!receiptNodesToSend.Any())
			{
				return;
			}

			var result = salesReceiptSender.SendReceipts(receiptNodesToSend.ToArray());

			foreach(var sendReceiptNode in result)
			{
				var receiptSent = sendReceiptNode.SendResultCode == HttpStatusCode.OK;
				var trueMarkOrder = sendReceiptNode.TrueMarkCashReceiptOrder;

				sendReceiptNode.CashReceipt.Sent = receiptSent;
				sendReceiptNode.CashReceipt.HttpCode = (int)sendReceiptNode.SendResultCode;
				uow.Save(sendReceiptNode.CashReceipt);

				if(receiptSent)
				{
					trueMarkOrder.Status = TrueMarkCashReceiptOrderStatus.Sended;
					trueMarkOrder.ErrorDescription = "";
				}
				else
				{
					trueMarkOrder.Status = TrueMarkCashReceiptOrderStatus.ReceiptSendError;
					trueMarkOrder.ErrorDescription = $"Код ошибки: {(int)sendReceiptNode.SendResultCode}";
				}
				trueMarkOrder.CashReceipt = sendReceiptNode.CashReceipt;
				uow.Save(trueMarkOrder);
			}
		}

		private void SendForOrdersWithNotSendReceipts(IUnitOfWork uow, IEnumerable<ReceiptForOrderNode> nodes, ref int notValidCount)
		{
			IList<PreparedReceiptNode> receiptNodesToSend = new List<PreparedReceiptNode>();
			int sentBefore = 0;

			var receiptNodes = nodes.Where(x => x.ReceiptId.HasValue);

			//Предзагрузка для ускорения
			var receiptIds = receiptNodes.Select(x => x.ReceiptId.Value);
			var receipts = uow.GetById<CashReceipt>(receiptIds);
			var trueMarkOrderIds = nodes.Select(x => x.TrueMarkCashReceiptOrderId);
			var trueMarkOrders = uow.GetById<TrueMarkCashReceiptOrder>(trueMarkOrderIds);

			foreach(var receiptNode in receiptNodes)
			{
				var receipt = uow.GetById<CashReceipt>(receiptNode.ReceiptId.Value);

				if(receipt.Sent)
				{
					sentBefore++;
					continue;
				}

				var trueMarkOrder = uow.GetById<TrueMarkCashReceiptOrder>(receiptNode.TrueMarkCashReceiptOrderId);

				SalesDocumentDTO doc = null;
				try
				{
					doc = new SalesDocumentDTO(receipt.Order, trueMarkOrder, receipt.Order.Contract?.Organization?.ActiveOrganizationVersion?.Leader?.ShortName);
				}
				catch(TrueMarkException ex)
				{
					trueMarkOrder.ErrorDescription = ex.Message;
					trueMarkOrder.Status = TrueMarkCashReceiptOrderStatus.ReceiptSendError;
					uow.Save(trueMarkOrder);
					continue;
				}

				CashBox cashBox = null;
				if(receipt.Order.Contract?.Organization?.CashBoxId != null)
				{
					cashBox = cashBoxes.FirstOrDefault(x => x.Id == receipt.Order.Contract.Organization.CashBoxId);
				}

				if(doc.IsValid && cashBox != null)
				{
					var newPreparedReceiptNode = new PreparedReceiptNode
					{
						CashReceipt = receipt,
						SalesDocumentDTO = doc,
						CashBox = cashBox,
						TrueMarkCashReceiptOrder = trueMarkOrder
					};

					receiptNodesToSend.Add(newPreparedReceiptNode);

					if(receiptNodesToSend.Count >= maxReceiptsAllowedToSendInOneGo)
					{
						break;
					}
				}
				else
				{
					if(cashBox == null)
					{
						logger.Warn($"Для заказа №{receipt.Order.Id} не удалось подобрать кассовый аппарат для отправки. Пропускаю");
					}

					trueMarkOrder.Status = TrueMarkCashReceiptOrderStatus.ReceiptSendError;
					trueMarkOrder.ErrorDescription = $"Для заказа №{receipt.Order.Id} не удалось подобрать кассовый аппарат для отправки.";
					trueMarkOrder.CashReceipt = receipt;

					notValidCount++;
					receipt.HttpCode = 0;
					receipt.Sent = false;
					uow.Save(receipt);
					uow.Save(trueMarkOrder);
				}
			}

			if(sentBefore > 0)
			{
				logger.Info($"{sentBefore} {NumberToTextRus.Case(sentBefore, "документ был отправлен", "документа было отправлено", "документов было отправлено")} ранее.");
			}

			logger.Info($"Количество чеков для переотправки: {receiptNodesToSend.Count} (Макс.: {maxReceiptsAllowedToSendInOneGo})");
			if(!receiptNodesToSend.Any())
			{
				return;
			}

			var result = salesReceiptSender.SendReceipts(receiptNodesToSend.ToArray());

			foreach(var sendReceiptNode in result)
			{
				var receiptSent = sendReceiptNode.SendResultCode == HttpStatusCode.OK;
				var trueMarkOrder = sendReceiptNode.TrueMarkCashReceiptOrder;

				sendReceiptNode.CashReceipt.Sent = receiptSent;
				sendReceiptNode.CashReceipt.HttpCode = (int)sendReceiptNode.SendResultCode;
				uow.Save(sendReceiptNode.CashReceipt);

				if(receiptSent)
				{
					trueMarkOrder.Status = TrueMarkCashReceiptOrderStatus.Sended;
				}
				else
				{
					trueMarkOrder.Status = TrueMarkCashReceiptOrderStatus.ReceiptSendError;
					trueMarkOrder.ErrorDescription = $"Код ошибки: {(int)sendReceiptNode.SendResultCode}";
				}
				trueMarkOrder.CashReceipt = sendReceiptNode.CashReceipt;
				uow.Save(trueMarkOrder);
			}
		}

		private void SendForLegalSelfdeliveryWithoutReceipts(IUnitOfWork uow, IEnumerable<ReceiptForOrderNode> nodes, ref int notValidCount)
		{
			IList<PreparedReceiptNode> receiptNodesToSend = new List<PreparedReceiptNode>();

			foreach(var order in uow.GetById<Order>(nodes.Select(x => x.OrderId)))
			{
				var newReceipt = new CashReceipt { Order = order };
				var doc = new SalesDocumentDTO(order, order.Contract?.Organization?.ActiveOrganizationVersion?.Leader?.ShortName);

				CashBox cashBox = null;
				if(order.Contract?.Organization?.CashBoxId != null)
				{
					cashBox = cashBoxes.FirstOrDefault(x => x.Id == order.Contract.Organization.CashBoxId);
				}

				if(doc.IsValid && cashBox != null)
				{

					var newPreparedReceiptNode = new PreparedReceiptNode
					{
						CashReceipt = newReceipt,
						SalesDocumentDTO = doc,
						CashBox = cashBox
					};

					receiptNodesToSend.Add(newPreparedReceiptNode);

					if(receiptNodesToSend.Count >= maxReceiptsAllowedToSendInOneGo)
					{
						break;
					}
				}
				else
				{
					if(cashBox == null)
					{
						logger.Warn($"Для заказа №{order.Id} не удалось подобрать кассовый аппарат для отправки. Пропускаю");
					}

					notValidCount++;
					newReceipt.HttpCode = 0;
					newReceipt.Sent = false;
					uow.Save(newReceipt);
				}
			}

			logger.Info($"Количество новых чеков для отправки: {receiptNodesToSend.Count} (Макс.: {maxReceiptsAllowedToSendInOneGo})");
			if(!receiptNodesToSend.Any())
			{
				return;
			}

			var result = salesReceiptSender.SendReceipts(receiptNodesToSend.ToArray());

			foreach(var sendReceiptNode in result)
			{
				sendReceiptNode.CashReceipt.Sent = sendReceiptNode.SendResultCode == HttpStatusCode.OK;
				sendReceiptNode.CashReceipt.HttpCode = (int)sendReceiptNode.SendResultCode;
				uow.Save(sendReceiptNode.CashReceipt);
			}
		}

		private void SendForLegalSelfdeliveryWithNotSendReceipts(IUnitOfWork uow, IEnumerable<ReceiptForOrderNode> nodes, ref int notValidCount)
		{
			IList<PreparedReceiptNode> receiptNodesToSend = new List<PreparedReceiptNode>();
			int sentBefore = 0;

			foreach(var receipt in uow.GetById<CashReceipt>(nodes.Select(n => n.ReceiptId).Where(x => x.HasValue).Select(x => x.Value)))
			{
				if(receipt.Sent)
				{
					sentBefore++;
					continue;
				}

				var doc = new SalesDocumentDTO(receipt.Order, receipt.Order.Contract?.Organization?.ActiveOrganizationVersion?.Leader?.ShortName);

				CashBox cashBox = null;
				if(receipt.Order.Contract?.Organization?.CashBoxId != null)
				{
					cashBox = cashBoxes.FirstOrDefault(x => x.Id == receipt.Order.Contract.Organization.CashBoxId);
				}

				if(doc.IsValid && cashBox != null)
				{
					var newPreparedReceiptNode = new PreparedReceiptNode
					{
						CashReceipt = receipt,
						SalesDocumentDTO = doc,
						CashBox = cashBox
					};

					receiptNodesToSend.Add(newPreparedReceiptNode);

					if(receiptNodesToSend.Count >= maxReceiptsAllowedToSendInOneGo)
					{
						break;
					}
				}
				else
				{
					if(cashBox == null)
					{
						logger.Warn($"Для заказа №{receipt.Order.Id} не удалось подобрать кассовый аппарат для отправки. Пропускаю");
					}

					notValidCount++;
					receipt.HttpCode = 0;
					receipt.Sent = false;
					uow.Save(receipt);
				}
			}

			if(sentBefore > 0)
			{
				logger.Info($"{sentBefore} {NumberToTextRus.Case(sentBefore, "документ был отправлен", "документа было отправлено", "документов было отправлено")} ранее.");
			}

			logger.Info($"Количество чеков для переотправки: {receiptNodesToSend.Count} (Макс.: {maxReceiptsAllowedToSendInOneGo})");
			if(!receiptNodesToSend.Any())
			{
				return;
			}

			var result = salesReceiptSender.SendReceipts(receiptNodesToSend.ToArray());

			foreach(var sendReceiptNode in result)
			{
				sendReceiptNode.CashReceipt.Sent = sendReceiptNode.SendResultCode == HttpStatusCode.OK;
				sendReceiptNode.CashReceipt.HttpCode = (int)sendReceiptNode.SendResultCode;
				uow.Save(sendReceiptNode.CashReceipt);
			}
		}
	}
}
