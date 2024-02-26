using System;
using System.Linq;
using System.Timers;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace SmsPaymentService.Workers
{
    public class OverduePaymentsWorker
    {
        private Timer timer;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWorkFactory _uowFactory;
		private const double startInterval = 3 * 60 * 1000; //3 минуты
        private const double interval = 60 * 60 * 1000;     //1 час

		public OverduePaymentsWorker(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

        public void Start()
        {
            logger.Info("Запуск процесса закрытия просроченных платежей...");
            timer = new Timer(startInterval);
            timer.Elapsed += TimerOnElapsed;
            timer.Start();
            logger.Info("Процесс закрытия просроченных платежей запущен");
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try {
                logger.Info("Закрытие просроченных платежей...");
                timer.Interval = interval;
                
                using (var uow = _uowFactory.CreateWithoutRoot()) {
                    
                    var paymentsToCancel = uow.Session.QueryOver<SmsPayment>()
                        .Where(x => x.SmsPaymentStatus == SmsPaymentStatus.WaitingForPayment)
                        .And(x => x.CreationDate < DateTime.Now.AddDays(-2))
                        .List();
                    
                    foreach (var payment in paymentsToCancel) {
                        payment.SetCancelled();
                        uow.Save(payment);
                    }

                    if(paymentsToCancel.Any()) {
                        uow.Commit();
                        logger.Info($"{paymentsToCancel.Count} платежей переведены в статус {SmsPaymentStatus.Cancelled}");
                    }
                    else {
                        logger.Info($"Не обнаружено просроченных платежей");
                    }
                }
            }
            catch (Exception exception) {
                logger.Error(exception, $"Ошибка при закрытии просроченных платежей");
            }
        }
        
    }
}
