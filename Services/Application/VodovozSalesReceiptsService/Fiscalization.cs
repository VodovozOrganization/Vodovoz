using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NLog;
using QS.DomainModel.UoW;
using QS.Utilities;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using VodovozSalesReceiptsService.DTO;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;

namespace VodovozSalesReceiptsService
{
	public static class Fiscalization
	{
		static string baseAddress;
		static HttpClient httpClient;
		static Logger logger = LogManager.GetCurrentClassLogger();

		public static async Task RunAsync(string baseAddress, AuthenticationHeaderValue authentication)
		{
			if(httpClient == null) {
				httpClient = new HttpClient();
				Fiscalization.baseAddress = baseAddress;
				httpClient.BaseAddress = new Uri(baseAddress);
				httpClient.DefaultRequestHeaders.Accept.Clear();
				httpClient.DefaultRequestHeaders.Authorization = authentication;
				httpClient.DefaultRequestHeaders.Accept.Add(
					new MediaTypeWithQualityHeaderValue("application/json")
				);
			}
			logger.Info(string.Format("Авторизация и проверка фискального регистратора..."));
			FinscalizatorStatusResponseDTO response = await GetSatusAsync("fn/v1/status");

			if(response != null) {
				switch(response.Status) {
				case FiscalRegistratorStatus.Associated:
					logger.Warn("Клиент успешно связан с розничной точкой, но касса еще ни разу не вышла на связь и не сообщила свое состояние.");
					return;
				case FiscalRegistratorStatus.Failed:
					logger.Warn("Проблемы получения статуса фискального накопителя. Этот статус не препятствует добавлению документов для фискализации. " +
						"Все документы будут добавлены в очередь на сервере и дождутся момента когда касса будет в состоянии их фискализировать.");
					break;
				case FiscalRegistratorStatus.Ready:
					logger.Info("Соединение с фискальным накопителем установлено и его состояние позволяет фискализировать чеки.");
					break;
				default:
					logger.Warn(string.Format("Провал с сообщением: \"{0}\".", response.Message));
					return;
				}
			} else {
				logger.Warn("Провал. Нет ответа от сервиса.");
				return;
			}

			logger.Info("Подготовка документов к отправке на сервер фискализации...");
			int sent = 0, sentBefore = 0, notValid = 0, receiptsToSend = 0;
			using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot("[Fisk] Получение подходящих заказов и чеков (если они есть)...")) {
				var ordersAndReceiptNodes = GetReceiptsForOrders(uow);
				var withoutReceipts = ordersAndReceiptNodes.Where(r => r.ReceiptId == null);
				var withNotSentReceipts = ordersAndReceiptNodes.Where(r => r.ReceiptId.HasValue && r.WasSent != true);
				var cashier = uow.GetById<Employee>(new BaseParametersProvider().DefaultSalesReceiptCashierId).ShortName;
				receiptsToSend = withoutReceipts.Count() + withNotSentReceipts.Count();

				if(receiptsToSend <= 0) {
					logger.Info("Нет документов для отправки.");
					return;
				}

				if(withoutReceipts.Any()) {
					var ordersWithoutReceipts = uow.GetById<Order>(withoutReceipts.Select(n => n.OrderId));
					foreach(var o in ordersWithoutReceipts) {
						logger.Info(string.Format("Подготовка документа \"№{0}\" к отправке...", o.Id));
						var newReceipt = new CashReceipt { Order = o };
						var doc = new SalesDocumentDTO(o, cashier);
						if(!doc.IsValid)
							notValid++;
						await SendSalesDocumentAsync(newReceipt, doc);
						uow.Save(newReceipt);
						if(newReceipt.Sent) {
							logger.Info(string.Format("Чек для заказа \"№{0}\" отправлен", o.Id));
							sent++;
						}
						continue;
					}
				}

				if(withNotSentReceipts.Any()) {
					var ordersWithNotSentReceipts = uow.GetById<Order>(withNotSentReceipts.Select(n => n.OrderId));
					var notSentReceipts = uow.GetById<CashReceipt>(withNotSentReceipts.Select(n => n.ReceiptId.Value));
					foreach(var r in notSentReceipts) {
						if(r.Sent) {
							sentBefore++;
							continue;
						}
						logger.Info(string.Format("Подготовка документа \"№{0}\" к переотправке...", r.Order.Id));
						var doc = new SalesDocumentDTO(r.Order, cashier);
						if(!doc.IsValid)
							notValid++;
						await SendSalesDocumentAsync(r, doc);
						uow.Save(r);
						if(r.Sent) {
							logger.Info(string.Format("Чек для заказа \"№{0}\" переотправлен", r.Order.Id));
							sent++;
						}
						continue;
					}
				}
				uow.Commit();
			}

			logger.Info(
				string.Format(
					"За текущую сессию {0} {1} {2} из {3}.",
					NumberToTextRus.Case(sent, "был отправлен", "было отправлено", "было отправлено"),
					sent,
					NumberToTextRus.Case(sent, "чек", "чека", "чеков"),
					receiptsToSend
				)
			);
			if(sentBefore > 0)
				logger.Info(
					string.Format(
						"{0} {1} ранее.",
						sentBefore,
						NumberToTextRus.Case(sentBefore, "документ был отправлен", "документа было отправлено", "документов было отправлено")
					)
				);
			if(notValid > 0)
				logger.Info(
					string.Format(
						"{0} {1}.",
						notValid,
						NumberToTextRus.Case(notValid, "документ не валиден", "документа не валидно", "документов не валидно")
					)
				);
		}

		static async Task SendSalesDocumentAsync(CashReceipt preparedReceipt, SalesDocumentDTO doc)
		{
			if(doc.IsValid) {
				logger.Info("Отправка документа на сервер фискализации...");
				var httpCode = await PostSalesDocumentAsync(doc);
				switch(httpCode) {
				case HttpStatusCode.OK:
					logger.Info("Документ успешно отправлен на сервер фискализации.");
					preparedReceipt.Sent = true;
					break;
				default:
					logger.Warn(string.Format("Документ не был отправлен на сервер фискализации. Http код - {0} ({1}).", (int)httpCode, httpCode));
					preparedReceipt.Sent = false;
					break;
				}
				preparedReceipt.HttpCode = (int)httpCode;
			} else {
				logger.Warn(string.Format("Документ \"{0}\" не валиден и не был отправлен на сервер фискализации (-1).", doc.DocNum));
				preparedReceipt.HttpCode = -1;
				preparedReceipt.Sent = false;
			}
		}

		static ReceiptForOrderNode[] GetReceiptsForOrders(IUnitOfWork uow)
		{
			ReceiptForOrderNode[] notSelfDeliveredOrderIds = null;
			ReceiptForOrderNode[] selfDeliveredOrderIds = null;

			notSelfDeliveredOrderIds = OrderSingletonRepository.GetInstance()
											   .GetShippedOrdersWithReceiptsForDates(
													uow,
													DateTime.Today.AddDays(-3));

			selfDeliveredOrderIds = OrderSingletonRepository.GetInstance()
															.GetClosedSelfDeliveredOrdersWithReceiptsForDates(
																uow,
																PaymentType.cash,
																OrderStatus.Closed,
																DateTime.Today.AddDays(-3));

			var orderIds = notSelfDeliveredOrderIds.Union(selfDeliveredOrderIds).ToArray();

			return orderIds;
		}

		static async Task<FinscalizatorStatusResponseDTO> GetSatusAsync(string path)
		{
			FinscalizatorStatusResponseDTO statusResponse = null;
			HttpResponseMessage response = await httpClient.GetAsync(path);
			if(response.IsSuccessStatusCode)
				statusResponse = await response.Content.ReadAsAsync<FinscalizatorStatusResponseDTO>();

			return statusResponse;
		}

		static async Task<HttpStatusCode> PostSalesDocumentAsync(SalesDocumentDTO order)
		{
			HttpResponseMessage response = await httpClient.PostAsJsonAsync(baseAddress + "/fn/v1/doc", order);
			return response.StatusCode;
		}
	}
}