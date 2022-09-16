using Microsoft.Extensions.Logging;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace DeliveryRulesService.Cache
{
	/// <summary>
	/// Класс для получение районов даже при отсутствии соединения с базой. Обновляет список районов каждый час
	/// </summary>
	public class DistrictCache
    {
		private readonly ILogger<DistrictCache> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;

		public DistrictCache(ILogger<DistrictCache> logger, IUnitOfWorkFactory uowFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

        public IEnumerable<District> Districts { get; private set; } = new List<District>();

		public void UpdateCache()
		{
			_logger.LogInformation("Обновление бэкапа районов...");

			using(var uow = _uowFactory.CreateWithoutRoot())
			{

				DistrictsSet districtsSetAlias = null;
				var districts = uow.Session.QueryOver<District>()
					.JoinAlias(x => x.DistrictsSet, () => districtsSetAlias)
					.Where(() => districtsSetAlias.Status == DistrictsSetStatus.Active).List();

				foreach(var district in districts)
				{
					NHibernateUtil.Initialize(district.GeographicGroup);

					foreach(var scheduleRestriction in district.GetAllDeliveryScheduleRestrictions())
					{
						NHibernateUtil.Initialize(scheduleRestriction.DeliverySchedule);
					}
					foreach(var weekDayRuleItem in district.GetAllWeekDayDistrictRuleItems())
					{
						NHibernateUtil.Initialize(weekDayRuleItem.DeliveryPriceRule);
					}
					foreach(var commonRuleItem in district.CommonDistrictRuleItems)
					{
						NHibernateUtil.Initialize(commonRuleItem.DeliveryPriceRule);
					}
				}

				Districts = districts;
			}

			_logger.LogInformation("Обновление бэкапа районов успешно завершено");
		}
    }
}
