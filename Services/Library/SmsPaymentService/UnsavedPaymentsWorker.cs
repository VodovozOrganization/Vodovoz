using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain;

namespace SmsPaymentService
{
    public class UnsavedPaymentsWorker
    {
        public UnsavedPaymentsWorker(FileProvider fileProvider)
        {
            this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        }
        
        private Timer timer;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly FileProvider fileProvider;
        private bool isWorkInProgress = false;
        private const double startInterval = 10 * 1000; //10 секунд
        private const double interval = 10 * 60 * 1000; //10 минут

        public void Start()
        {
            logger.Info("Запуск процесса пересохранения платежей из файла...");
            timer = new Timer(startInterval);
            timer.Elapsed += TimerOnElapsed;
            timer.Start();
            logger.Info("Процесс пересохранения платежей из файла запущен");
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if(isWorkInProgress)
                return;

            try {
                isWorkInProgress = true;
                logger.Info("Поиск несохранённых ExternalId...");
                timer.Interval = interval;

                Dictionary<int, int> values = new Dictionary<int, int>();
                var lines = fileProvider.GetAllLines();
                
                var str = $"Найдено {lines.Length} несохранённых ExternalId.";
                if(lines.Length > 0)
                    logger.Info(str + " Сохранение...");
                else {
                    logger.Info(str);
                    return;
                }
                
                foreach (var line in lines) {
                    var splittedLine = line.Split(',');
                    values.Add(Int32.Parse(splittedLine[0]), Int32.Parse(splittedLine[1]));
                }

                using (IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {
                    foreach (var smsPayment in uow.GetById<SmsPayment>(values.Select(x => x.Key))) {
                        if(smsPayment.ExternalId == 0)
                            smsPayment.ExternalId = values[smsPayment.Id];
                        if(smsPayment.SmsPaymentStatus == SmsPaymentStatus.ReadyToSend)
                            smsPayment.SetWaitingForPayment();
                        uow.Save(smsPayment);
                    }
                    
                    uow.Commit();
                    logger.Info($"Сохранено {lines.Length} платежей. Удаление из временного файла...");
                }
                
                fileProvider.RemoveLines(lines);
                logger.Info("Пересохранение платежей из файла успешно завершено");
            }
            catch (Exception exception) {
                logger.Error(exception, "Ошибка при пересохранении платежей из файла");
            }
            finally {
                isWorkInProgress = false;
            }
        }

        public void Trigger()
        {
            TimerOnElapsed(null, null);
        }
                
    }
}