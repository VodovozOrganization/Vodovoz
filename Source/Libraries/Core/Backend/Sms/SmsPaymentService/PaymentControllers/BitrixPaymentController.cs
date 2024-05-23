using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NLog;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;

namespace SmsPaymentService.PaymentControllers
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
            //Засекаем время чтобы проверить что в ответе есть хотя бы один новый, то есть мы что-то создали, но ответ не пришел
            var now = DateTime.Now;
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
                    HttpResponseMessage httpResponse = await httpClient.PostAsync($"{baseAddress}/vodovoz/handlernew.php", httpContent);
                
                    var responseContent = httpResponse.Content.ReadAsStringAsync().Result;
                    logger.Info($"Битрикс вернул http код: {httpResponse.StatusCode} Content: {responseContent}");

                    JObject obj = JObject.Parse(responseContent);

                    var externalId = Int32.Parse(obj["dealId"]?.ToString());
                    return new SendResponse { HttpStatusCode = httpResponse.StatusCode, ExternalId = externalId };
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"{ex}\nВозникла проблема, проверяем создался ли платеж на стороне битрикса");
                return await TryToGetExternalId(smsPaymentDto.OrderId, now);
            }
        }

        async Task<SendResponse> TryToGetExternalId(int orderId, DateTime now)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage httpResponse = await httpClient.GetAsync($"{baseAddress}/vodovoz/orders.php?orderId={orderId}");

                    var responseContent = httpResponse.Content.ReadAsStringAsync().Result;
                    logger.Info($"Битрикс вернул http код: {httpResponse.StatusCode} Content: {responseContent}");
                    try
                    {
                        List<SmsPaymentJson> smsPayments =
                            JsonConvert.DeserializeObject<List<SmsPaymentJson>>(responseContent);

                        var newSmsPayments = smsPayments
                            .Where(x => x.DateCreate > now && x.Status != SmsPaymentStatus.Cancelled);
                        var smsPaymentsArray = newSmsPayments as SmsPaymentJson[] ?? newSmsPayments.ToArray();
                        if (smsPaymentsArray.Count() > 1)
                        {    
                            logger.Error($"На стороне битрикса было создано {smsPaymentsArray.Count()} платежа!\n" + 
                                         $"{smsPayments}");
                            return new SendResponse();
                        } else if (!smsPaymentsArray.Any())
                        {
                            logger.Warn($"На стороне битрикса платеж не создан!");
                            return new SendResponse();
                        }
                        
                        var newSmsPayment = smsPayments.First();
                        
                        logger.Info($"Со второй попытки получен ExternalId платежа: " + $"{newSmsPayment.Id}");
                        return new SendResponse {HttpStatusCode = httpResponse.StatusCode, ExternalId = newSmsPayment.Id};
                    }
                    catch (JsonSerializationException e)
                    {
                        try
                        {
                            var errorJson = JsonConvert.DeserializeObject<ErrorJson>(responseContent);
                            logger.Warn($"Битрикс вернул ответ со статусом: " + errorJson.Status);
                        }
                        catch (JsonSerializationException unknownException)
                        {
                            logger.Warn($"Битрикс вернул что-то совсем непонятное: " + unknownException);
                            throw;
                        }
                    }
                    return new SendResponse();
                }
         
            } catch (Exception ex) {
                logger.Error("Что-то пошло совсем не так при передаче платежа в Битрикс: \n" + ex);
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
    
    class SmsPaymentJson
    {
        [JsonProperty("ID")]
        public int Id { get; set; }
        
        [JsonProperty("DATE_CREATE")]
        public DateTime DateCreate { get; set; }
        
        [JsonProperty("UF_CRM_1589369958853")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SmsPaymentStatus Status { get; set; }
    }
    public class ErrorJson
    {
        [JsonProperty("status")]
        public int Status { get; set; }
    }
}
