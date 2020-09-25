using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;

namespace SmsPaymentService
{
    public class BitrixPaymentController : IPaymentController
    {
        public BitrixPaymentController(string baseAddress)
        {
            this.baseAddress = baseAddress;
        }
        
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly string baseAddress;
        private readonly string dateFormat = "yyyy-MM-ddTHH:mm:ss";
        
        public SendResponse SendPayment(SmsPaymentDTO smsPayment)
        {
            Task<SendResponse> task = SendPaymentAsync(smsPayment);
            task.Wait();
            return task.Result;
        }

        private async Task<SendResponse> SendPaymentAsync(SmsPaymentDTO smsPaymentDto)
        {
            try {
                //Битриксу необходимо всегда передавать имя и фамилию для физических клиентов
                var clientName = smsPaymentDto.Recepient.Trim();
                if (smsPaymentDto.RecepientType == PersonType.natural && !clientName.Contains(' '))
                    smsPaymentDto.Recepient = clientName + " -";

                string content = JsonConvert.SerializeObject(smsPaymentDto, new JsonSerializerSettings { DateFormatString = dateFormat });
                logger.Info($"Передаём в битрикс данные: {content}");
            
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                    HttpResponseMessage httpResponse = await httpClient.PostAsync($"{baseAddress}/vodovoz/handler.php", httpContent);
                
                    var responseContent = httpResponse.Content.ReadAsStringAsync().Result;
                    logger.Info($"Битрикс вернул http код: {httpResponse.StatusCode} Content: {responseContent}");

                    JObject obj = JObject.Parse(responseContent);
                    
                    if(!Int32.TryParse(obj["dealId"]?.ToString(), out int externalId)) {
                        logger.Error($"Не получилось прочитать номер сделки");
                        return new SendResponse();
                    }
                    return new SendResponse { HttpStatusCode = httpResponse.StatusCode, ExternalId = externalId };
                }
            }
            catch (Exception ex) {
                logger.Error(ex, "Ошибка при передачи данных платежа в битрикс");
                return new SendResponse();
            }
        }
        
        public SmsPaymentStatus? GetPaymentStatus(int externalId)
        {
            Task<SmsPaymentStatus?> task = GetPaymentStatusAsync(externalId);
            task.Wait();
            return task.Result;
        }
        
        private async Task<SmsPaymentStatus?> GetPaymentStatusAsync(int externalId)
        {
            try {
                logger.Info($"Запрос статуса платежа от битрикса с externalId: {externalId}");
            
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage httpResponse = await httpClient.GetAsync($"{baseAddress}/vodovoz/status.php?id={externalId}");
                    var responseContent = httpResponse.Content.ReadAsStringAsync().Result;
                    logger.Info($"Битрикс вернул http код: {httpResponse.StatusCode}, Content: {responseContent}");
                
                    JObject obj = JObject.Parse(responseContent);
                    if (!obj.TryGetValue("status", out JToken value)) {
                        logger.Error("Не получилось прочитать ответ Битрикса");
                        return null;
                    }
                    if(!Int32.TryParse(value.ToString(), out int status)) {
                        logger.Error($"Не получилось прочитать статус платежа");
                        return null;
                    }
                    if (!Enum.GetValues(typeof(SmsPaymentStatus)).Cast<int>().Contains(status)) {
                        logger.Error($"Битрикс вернул неверный статус платежа (status: {status})");
                        return null;
                    }
                    return (SmsPaymentStatus)status;
                }
            }
            catch (Exception ex) {
                logger.Error(ex, "Ошибка при получение статуса платежа");
                return null;
            }
        }
        
    }
}