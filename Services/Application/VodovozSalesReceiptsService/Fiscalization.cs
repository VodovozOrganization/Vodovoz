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
        private static string baseAddress;
        private static HttpClient httpClient;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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

            logger.Info("Авторизация и проверка фискального регистратора...");
            FinscalizatorStatusResponseDTO response = await GetStatusAsync("fn/v1/status");

            if(response != null) {
                switch(response.Status) {
                    case FiscalRegistratorStatus.Associated:
                        logger.Warn(
                            "Клиент успешно связан с розничной точкой, но касса еще ни разу не вышла на связь и не сообщила свое состояние.");
                        return;
                    case FiscalRegistratorStatus.Failed:
                        logger.Warn(
                            "Проблемы получения статуса фискального накопителя. Этот статус не препятствует добавлению документов для фискализации. " +
                            "Все документы будут добавлены в очередь на сервере и дождутся момента когда касса будет в состоянии их фискализировать.");
                        break;
                    case FiscalRegistratorStatus.Ready:
                        logger.Info("Соединение с фискальным накопителем установлено и его состояние позволяет фискализировать чеки.");
                        break;
                    default:
                        logger.Warn($"Провал с сообщением: \"{response.Message}\".");
                        return;
                }
            }
            else {
                logger.Warn("Провал. Нет ответа от сервиса.");
                return;
            }

            logger.Info("Подготовка документов к отправке на сервер фискализации...");
            int sent = 0, sentBefore = 0, notValid = 0, receiptsToSend = 0;
            using(IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot("[Fisk] Получение подходящих заказов и чеков (если они есть)...")) {
                
                var ordersAndReceiptNodes = OrderSingletonRepository.GetInstance()
                    .GetOrdersForCashReceiptServiceToSend(uow, DateTime.Today.AddDays(-3)).ToList();
                
                var withoutReceipts = ordersAndReceiptNodes.Where(r => r.ReceiptId == null).ToList();
                var withNotSentReceipts = ordersAndReceiptNodes.Where(r => r.ReceiptId.HasValue && r.WasSent != true).ToList();
                
                receiptsToSend = withoutReceipts.Count + withNotSentReceipts.Count;
                if(receiptsToSend <= 0) {
                    logger.Info("Нет документов для отправки.");
                    return;
                }
                
                var cashier = uow.GetById<Employee>(new BaseParametersProvider().DefaultSalesReceiptCashierId).ShortName;

                if(withoutReceipts.Any()) {
                    var ordersWithoutReceipts = uow.GetById<Order>(withoutReceipts.Select(n => n.OrderId).Take(50));
                    foreach(var o in ordersWithoutReceipts) {
                        logger.Info($"Подготовка документа \"№{o.Id}\" к отправке...");
                        var newReceipt = new CashReceipt { Order = o };
                        var doc = new SalesDocumentDTO(o, cashier);
                        if(!doc.IsValid)
                            notValid++;
                        await SendSalesDocumentAsync(newReceipt, doc);
                        uow.Save(newReceipt);
                        if(newReceipt.Sent) {
                            logger.Info($"Чек для заказа \"№{o.Id}\" отправлен");
                            sent++;
                        }
                    }
                }

                if(withNotSentReceipts.Any()) {
                    uow.GetById<Order>(withNotSentReceipts.Select(n => n.OrderId));
                    var notSentReceipts = uow.GetById<CashReceipt>(withNotSentReceipts.Select(n => n.ReceiptId.Value));
                    foreach(var r in notSentReceipts) {
                        if(r.Sent) {
                            sentBefore++;
                            continue;
                        }

                        logger.Info($"Подготовка документа \"№{r.Order.Id}\" к переотправке...");
                        var doc = new SalesDocumentDTO(r.Order, cashier);
                        if(!doc.IsValid)
                            notValid++;
                        await SendSalesDocumentAsync(r, doc);
                        uow.Save(r);
                        if(r.Sent) {
                            logger.Info($"Чек для заказа \"№{r.Order.Id}\" переотправлен");
                            sent++;
                        }
                    }
                }

                uow.Commit();
            }

            logger.Info(
                $"За текущую сессию {NumberToTextRus.Case(sent, "был отправлен", "было отправлено", "было отправлено")} {sent} {NumberToTextRus.Case(sent, "чек", "чека", "чеков")} из {receiptsToSend}."
            );
            if(sentBefore > 0)
                logger.Info(
                    $"{sentBefore} {NumberToTextRus.Case(sentBefore, "документ был отправлен", "документа было отправлено", "документов было отправлено")} ранее."
                );
            if(notValid > 0)
                logger.Info(
                    $"{notValid} {NumberToTextRus.Case(notValid, "документ не валиден", "документа не валидно", "документов не валидно")}."
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
                        logger.Warn($"Документ не был отправлен на сервер фискализации. Http код - {(int)httpCode} ({httpCode}).");
                        preparedReceipt.Sent = false;
                        break;
                }

                preparedReceipt.HttpCode = (int)httpCode;
            }
            else {
                logger.Warn($"Документ \"{doc.DocNum}\" не валиден и не был отправлен на сервер фискализации (-1).");
                preparedReceipt.HttpCode = -1;
                preparedReceipt.Sent = false;
            }
        }

        static async Task<FinscalizatorStatusResponseDTO> GetStatusAsync(string path)
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