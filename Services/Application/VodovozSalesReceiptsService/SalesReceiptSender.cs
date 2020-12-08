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
                if(!ConnectToCashMachine(httpClient, cashBox)) {
                    return;
                }
                
                foreach(var receiptNode in receiptNodes) {
                    if(token.IsCancellationRequested) {
                        return;
                    }

                    var orderId = receiptNode.CashReceipt.Order.Id;
                    logger.Info($"Отправка документа №{orderId} на сервер фискализации...");
                    
                    receiptNode.SendResultCode = SendDocument(cashBox, httpClient, receiptNode.SalesDocumentDTO);

                    if(receiptNode.SendResultCode == HttpStatusCode.OK) {
                        logger.Info($"Чек для заказа №{orderId} отправлен");
                    }
                    else if(receiptNode.SendResultCode != 0) {
                        logger.Info($"Не удалось отправить чек для заказа №{orderId}. HTTP Code: {receiptNode.SendResultCode}");
                    }
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
            httpClient.BaseAddress = new Uri(cashBox.BaseAddress);
            httpClient.Timeout = TimeSpan.FromSeconds(60);
           
            return httpClient;
        }

        private bool ConnectToCashMachine(HttpClient httpClient, CashBox cashBox)
        {
            try {
                logger.Info($"Авторизация и проверка фискального регистратора №{cashBox.Id}...");
                
                HttpResponseMessage response = httpClient.GetAsync(cashBox.StatusPath).Result;
                
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

        private HttpStatusCode SendDocument(CashBox cashBox, HttpClient httpClient, SalesDocumentDTO doc)
        {
            try {
                HttpResponseMessage response = httpClient.PostAsJsonAsync(cashBox.BaseAddress + cashBox.SendDocumentPath, doc).Result;
                return response.StatusCode;
            }
            catch(Exception ex) {
                logger.Error(ex, "Ошибка при отправке на сервер фискализации");
                return 0;
            }
        }
    }
}