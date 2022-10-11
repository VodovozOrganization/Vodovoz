using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace SmsPaymentService
{
    public class SmsPaymentFileCache
    {
        public SmsPaymentFileCache(string filePath)
        {
            this.filePath = String.IsNullOrWhiteSpace(filePath) ? throw new ArgumentNullException(nameof(filePath)) : filePath;

            if(!File.Exists(filePath)) {
                var file = File.Create(filePath);
                file.Close();
            }
            
            var fileContent = File.ReadAllText(filePath);
            if(String.IsNullOrEmpty(fileContent)) {
                File.WriteAllText(filePath, JsonConvert.SerializeObject(new List<SmsPaymentCacheDTO>()));
            }
        }

        private readonly string filePath;
        private readonly Object locker = new Object();

        public void WritePaymentCache(int? smsPaymentId, int? externalId)
        {
            lock (locker) {
                var cache = JsonConvert.DeserializeObject<List<SmsPaymentCacheDTO>>(File.ReadAllText(filePath));
                cache.Add(new SmsPaymentCacheDTO { PaymentId = smsPaymentId, ExternalId = externalId });
                File.WriteAllText(filePath, JsonConvert.SerializeObject(cache));
            }
        }

        public IList<SmsPaymentCacheDTO> GetAllPaymentCaches()
        {
            lock (locker) {
                return JsonConvert.DeserializeObject<List<SmsPaymentCacheDTO>>(File.ReadAllText(filePath)).ToList();
            }
        }

        public void RemovePaymentCaches(IList<SmsPaymentCacheDTO> cachesToRemove)
        {
            lock (locker) {
                var cache = JsonConvert.DeserializeObject<List<SmsPaymentCacheDTO>>(File.ReadAllText(filePath));

                var newContent = JsonConvert.SerializeObject(
                    cache.Where(x => 
                        !cachesToRemove.Any(j => j.PaymentId == x.PaymentId && j.ExternalId == x.ExternalId)));
                File.WriteAllText(filePath, newContent);
            }
        }
        
    }
}