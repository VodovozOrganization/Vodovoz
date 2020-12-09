using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Vodovoz.Domain.Organizations;
using VodovozSalesReceiptsService.DTO;

namespace VodovozSalesReceiptsService
{
    /// <summary>
    /// Класс, отправляющий уже подготовленные и проверенные документы и чеки
    /// </summary>
    public class SalesReceiptSender : ISalesReceiptSender
    {
        public SalesReceiptSender(string baseAddress)
        {
            this.baseAddress = baseAddress;
            sendDocumentAddress = baseAddress + "/fn/v1/doc";
            fiscalizationStatusAddress = baseAddress + "fn/v1/status";
            documentStatusAddress = baseAddress + "fn/v1/doc/{0}/status";
        }

        private readonly string baseAddress;
        private readonly string sendDocumentAddress;
        private readonly string fiscalizationStatusAddress;
        private readonly string documentStatusAddress;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        public PreparedReceiptNode[] SendReceipts(PreparedReceiptNode[] preparedReceiptNodes, uint timeoutInSeconds = 300)
        {
            //Группировка по кассовым аппаратам
            var groupedNodes = preparedReceiptNodes.GroupBy(
                x => x.CashBox,
                (machine, receiptNodes) =>
                    new {
                        machine,
                        receiptNodes
                    }
            );
            
            IList<Task> runningTasks = new List<Task>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds));

            foreach(var groupedNode in groupedNodes) {
                runningTasks.Add(Task.Run(() => {
                    SendAllDocumentsForCashMachine(groupedNode.machine, groupedNode.receiptNodes, cts.Token);
                }, cts.Token));
            }

            try {
                Task.WaitAll(runningTasks.ToArray());
                return preparedReceiptNodes;
            }
            catch(Exception ex) {
                logger.Error(ex, "Неизвестная ошибка при отправке чеков");
                return preparedReceiptNodes;
            }
        }

        private void SendAllDocumentsForCashMachine(CashBox cashBox, IEnumerable<PreparedReceiptNode> receiptNodes, CancellationToken token)
        {
            logger.Info($"Отправка чеков для фискального регистратора №{cashBox.Id}");
            
            using(HttpClient httpClient = GetHttpClient(cashBox)) {
                if(!ConnectToCashBox(httpClient, cashBox)) {
                    return;
                }
                
                foreach(var receiptNode in receiptNodes) {
                    if(token.IsCancellationRequested) {
                        return;
                    }

                    var orderId = receiptNode.CashReceipt.Order.Id;
                    logger.Info($"Отправка документа №{orderId} на сервер фискализации...");
                    
                    receiptNode.SendResultCode = SendDocument(httpClient, receiptNode.SalesDocumentDTO, orderId);
                }
            }
        }

        private HttpClient GetHttpClient(CashBox cashBox)
        {
            var authentication = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(
                    Encoding.GetEncoding("ISO-8859-1").GetBytes($"{cashBox.UserName}:{cashBox.Password}")
                )
            );

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Authorization = authentication;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.BaseAddress = new Uri(baseAddress);
            httpClient.Timeout = TimeSpan.FromSeconds(60);
           
            return httpClient;
        }

        private bool ConnectToCashBox(HttpClient httpClient, CashBox cashBox)
        {
            try {
                logger.Info($"Авторизация и проверка фискального регистратора №{cashBox.Id}...");
                
                HttpResponseMessage response = httpClient.GetAsync(fiscalizationStatusAddress).Result;
                
                if(!response.IsSuccessStatusCode) {
                    logger.Warn("Провал. Нет ответа от сервиса.");
                    return false;
                }
                
                var finscalizatorStatusResponse = response.Content.ReadAsAsync<FinscalizatorStatusResponseDTO>().Result;
                
                if(finscalizatorStatusResponse == null) {
                    logger.Warn("Провал. Нет удалось прочитать ответ от сервиса.");
                    return false;
                }

                switch(finscalizatorStatusResponse.Status) {
                    case FiscalRegistratorStatus.Ready:
                        logger.Info("Соединение с фискальным накопителем установлено и его состояние позволяет фискализировать чеки.");
                        return true;
                    case FiscalRegistratorStatus.Failed:
                        logger.Warn("Проблемы получения статуса фискального накопителя. " +
                            "Этот статус не препятствует добавлению документов для фискализации. " +
                            "Все документы будут добавлены в очередь на сервере и дождутся момента когда касса будет в состоянии их фискализировать.");
                        return true;
                    case FiscalRegistratorStatus.Associated:
                        logger.Warn("Клиент успешно связан с розничной точкой, " +
                            "но касса еще ни разу не вышла на связь и не сообщила свое состояние.");
                        return false;
                    default:
                        logger.Warn($"Провал с сообщением: \"{finscalizatorStatusResponse.Message}\".");
                        return false;
                }
            }
            catch(Exception ex) {
                logger.Error(ex, "Ошибка при авторизации и проверки фискального регистратора");
                return false;
            }
        }

        private HttpStatusCode SendDocument(HttpClient httpClient, SalesDocumentDTO doc, int orderId)
        {
            try {
                HttpResponseMessage response = httpClient.PostAsJsonAsync(sendDocumentAddress, doc).Result;

                if(response.StatusCode == HttpStatusCode.OK) {
                    logger.Info($"Чек для заказа №{orderId} отправлен");
                }
                else {
                    logger.Info($"Не удалось отправить чек для заказа №{orderId}. HTTP Code: {response.StatusCode}. Запрашиваю актуальный статус...");
                    try {
                        var statusResponse = httpClient.GetAsync(String.Format(documentStatusAddress, doc.Id)).Result;

                        if(statusResponse.IsSuccessStatusCode) {
                            var documentStatusDTO = statusResponse.Content.ReadAsAsync<SalesDocumentsStatusDTO>().Result;
                            logger.Info($"Актульный статус чека для заказа №{orderId}: {documentStatusDTO.Status}");
                            if(new[] { DocumentStatus.Completed, DocumentStatus.WaitForCallback, DocumentStatus.Printed }.Contains(documentStatusDTO.Status)) {
                                return HttpStatusCode.OK;
                            }
                        }
                        else {
                            logger.Info($"Не удалось получить актуальный статус чека для заказа №{orderId}");
                        }
                    }
                    catch {
                        logger.Info($"Ошибка при получении актального статуса чека для заказа №{orderId}");
                    }
                }

                return response.StatusCode;
            }
            catch(Exception ex) {
                logger.Error(ex, "Ошибка при отправке на сервер фискализации");
                return 0;
            }
        }
    }
}