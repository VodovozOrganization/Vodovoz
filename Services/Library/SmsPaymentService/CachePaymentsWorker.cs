using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace SmsPaymentService
{
    public class CachePaymentsWorker
    {
        public CachePaymentsWorker(SmsPaymentFileCache smsPaymentFileCache, ISmsPaymentService smsPaymentService)
        {
            this.smsPaymentFileCache = smsPaymentFileCache ?? throw new ArgumentNullException(nameof(smsPaymentFileCache));
            this.smsPaymentService = smsPaymentService ?? throw new ArgumentNullException(nameof(smsPaymentService));
        }
        
        private readonly SmsPaymentFileCache smsPaymentFileCache;
        private readonly ISmsPaymentService smsPaymentService;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private Timer timer;
        private bool isWorkInProgress = false;
        
        private const double startInterval = 10 * 1000; //10 секунд
        private const double interval = 30 * 1000; //30 секунд

        public void Start()
        {
            logger.Info("Запуск процесса синхронизации платежей из кэша...");
            timer = new Timer(startInterval);
            timer.Elapsed += TimerOnElapsed;
            timer.Start();
            logger.Info("Процесс синхронизации платежей из кэша запущен");
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if(isWorkInProgress)
                return;

            try {
                isWorkInProgress = true;
                timer.Interval = interval;
                logger.Info("Поиск данных платежей в кэше...");
                
                var caches = smsPaymentFileCache.GetAllPaymentCaches();
                
                var str = $"Найдено {caches.Count} данных платежей.";
                if(caches.Count > 0)
                    logger.Info(str + " Синхронизация...");
                else {
                    logger.Info(str);
                    return;
                }

                using (IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
                    
                    var smsPayments = uow.GetById<SmsPayment>(caches.Select(x => x.PaymentId));
                    int saveCount = 0;
                    foreach (var smsPayment in smsPayments.Where(x => x.ExternalId == 0 && x.SmsPaymentStatus == SmsPaymentStatus.ReadyToSend)) {
                        smsPayment.ExternalId = caches.First(x => x.PaymentId == smsPayment.Id).ExternalId;
                        smsPayment.SetWaitingForPayment();
                            
                        uow.Save(smsPayment);
                        saveCount++;
                    }
                    
                    if(saveCount > 0) {
                        uow.Commit();
                        logger.Info($"Сохранено {saveCount} платежей без ExternalId. Синхронизация статусов...");
                    }
                }
                
                IList<SmsPaymentCacheDTO> cachesToRemove = new List<SmsPaymentCacheDTO>();
                int synchronizedCount = 0;
                foreach (var cache in caches) {
                    var res = smsPaymentService.RefreshPaymentStatus(cache.ExternalId);
                    if(res.Status == PaymentResult.MessageStatus.Ok) {
                        cachesToRemove.Add(cache);
                        synchronizedCount++;
                    }
                }
                if(synchronizedCount > 0)
                    smsPaymentFileCache.RemovePaymentCaches(cachesToRemove);

                logger.Info($"Синхронизировано {synchronizedCount} статусов платежей");
                
            }
            catch (Exception exception) {
                logger.Error(exception, "Ошибка при синхронизации платежей из кэша");
            }
            finally {
                isWorkInProgress = false;
            }
        }

    }
}