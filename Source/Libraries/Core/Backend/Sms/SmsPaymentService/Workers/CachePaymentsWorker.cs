using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace SmsPaymentService.Workers
{
    public class CachePaymentsWorker
    {
        public CachePaymentsWorker(IUnitOfWorkFactory uowFactory, SmsPaymentFileCache smsPaymentFileCache, ISmsPaymentService smsPaymentService)
        {
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			this.smsPaymentFileCache = smsPaymentFileCache ?? throw new ArgumentNullException(nameof(smsPaymentFileCache));
            this.smsPaymentService = smsPaymentService ?? throw new ArgumentNullException(nameof(smsPaymentService));
        }

		private readonly IUnitOfWorkFactory _uowFactory;
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

                using (var uow = _uowFactory.CreateWithoutRoot()) {
                    var cachesWithIds = caches.Where(x => x.ExternalId.HasValue && x.PaymentId.HasValue).ToList();

                    var readyForSendPayments = uow.Session.QueryOver<SmsPayment>()
                        .WhereRestrictionOn(x => x.Id).IsIn(cachesWithIds.Select(x => x.PaymentId).ToArray())
                        .And(x => x.SmsPaymentStatus == SmsPaymentStatus.ReadyToSend)
                        .List();

                    if(readyForSendPayments.Any()) {
                        logger.Info($"Найдено {readyForSendPayments.Count} платежей в статусе {SmsPaymentStatus.ReadyToSend}. Меняю статус...");
                    }

                    foreach (var payment in readyForSendPayments) {
                        payment.ExternalId = cachesWithIds.First(x => x.PaymentId.Value == payment.Id).ExternalId.Value;
                        payment.SetWaitingForPayment();
                        uow.Save(payment);
                    }

                    if(readyForSendPayments.Any()) {
                        uow.Commit();
                    }
                }
                
                IList<SmsPaymentCacheDTO> cachesToRemove = new List<SmsPaymentCacheDTO>();
                int synchronizedCount = 0;
                foreach (var cache in caches.Where(x => x.ExternalId.HasValue)) {
                    var res = smsPaymentService.RefreshPaymentStatus(cache.ExternalId.Value);
                    if(res.Status == PaymentResult.MessageStatus.Ok) {
                        cachesToRemove.Add(cache);
                        synchronizedCount++;
                    }
                }

                if(synchronizedCount > 0) {
                    smsPaymentFileCache.RemovePaymentCaches(cachesToRemove);
                }

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
