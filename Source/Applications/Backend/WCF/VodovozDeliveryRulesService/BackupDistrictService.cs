using System;
using System.Collections.Generic;
using System.Timers;
using NHibernate;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace VodovozDeliveryRulesService
{
    /// <summary>
    /// Класс для получение районов даже при отсутствии соединения с базой. Обновляет список районов каждый час
    /// </summary>
    public class BackupDistrictService : IBackupDistrictService
    {
        private Timer timer;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private const double startInterval =  5 * 1000;    //5 секунд
        private const double interval = 60 * 60 * 1000;    //1 час
        
        public IEnumerable<District> Districts { get; private set; } = new List<District>();

        public void StartAutoUpdateTask()
        {
            logger.Info("Запуск процесса создания бэкапа районов...");
            timer = new Timer(startInterval);
            timer.Elapsed += TimerOnElapsed;
            timer.Start();
            logger.Info("Процесс создания бэкапа районов запущен");
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try {
                timer.Interval = interval;
                logger.Info("Обновление бэкапа районов...");
                
                using (IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot()) {

                    DistrictsSet districtsSetAlias = null;
                    var districts = uow.Session.QueryOver<District>()
                        .JoinAlias(x => x.DistrictsSet, () => districtsSetAlias)
                        .Where(() => districtsSetAlias.Status == DistrictsSetStatus.Active).List();

                    foreach (var district in districts) {
                        NHibernateUtil.Initialize(district.GeographicGroup);

                        foreach (var scheduleRestriction in district.GetAllDeliveryScheduleRestrictions()) {
                            NHibernateUtil.Initialize(scheduleRestriction.DeliverySchedule);
                        }
                        foreach (var weekDayRuleItem in district.GetAllWeekDayDistrictRuleItems()) {
                            NHibernateUtil.Initialize(weekDayRuleItem.DeliveryPriceRule);
                        }
                        foreach (var commonRuleItem in district.CommonDistrictRuleItems) {
                            NHibernateUtil.Initialize(commonRuleItem.DeliveryPriceRule);
                        }
                    }

                    Districts = districts;
                }
                
                logger.Info("Обновление бэкапа районов успешно завершено");
            }
            catch (Exception ex) {
                logger.Error(ex, "Ошибка при обновлении бэкапа районов");
            }
        }

    }

    public interface IBackupDistrictService
    {
        IEnumerable<District> Districts { get; }
    }
}