using System;
using System.Collections.Generic;
using System.Timers;
using NHibernate;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Sectors;

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
        
        public IEnumerable<Sector> Sector { get; private set; } = new List<Sector>();

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

	                SectorVersion sectorVersion = null;
                    var districts = uow.Session.QueryOver<Sector>()
                        .JoinAlias(x => x.ActiveSectorVersion, () => sectorVersion).List();

                    foreach (var district in districts) {
                        NHibernateUtil.Initialize(district.ActiveSectorVersion.GeographicGroup);

                        foreach (var scheduleRestriction in district.ActiveWeekDayScheduleVersion.SectorSchedules) {
                            NHibernateUtil.Initialize(scheduleRestriction.DeliverySchedule);
                        }
                        foreach (var weekDayRuleItem in district.ActiveWeekDayDeliveryRuleVersion.WeekDayDistrictRules) {
                            NHibernateUtil.Initialize(weekDayRuleItem.DeliveryPriceRule);
                        }
                        foreach (var commonRuleItem in district.ActiveDeliveryRuleVersion.CommonDistrictRuleItems) {
                            NHibernateUtil.Initialize(commonRuleItem.DeliveryPriceRule);
                        }
                        
                    }
                    Sector = districts;
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
        IEnumerable<Sector> Sector { get; }
    }
}