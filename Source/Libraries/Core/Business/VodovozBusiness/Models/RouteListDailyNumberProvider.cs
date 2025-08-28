using MySqlConnector;
using QS.DomainModel.UoW;
using System;
using System.Data;
using System.Linq;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Logistics;

namespace Vodovoz.Models
{
	public class RouteListDailyNumberProvider : IRouteListDailyNumberProvider
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public RouteListDailyNumberProvider(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public int GetOrCreateDailyNumber(int routeListId, DateTime date)
		{
			for(var i = 0; i < 3; i++)
			{
                using(var uow = _uowFactory.CreateWithoutRoot(GetType().Name))
                {
                    using(var transaction = uow.Session.BeginTransaction(IsolationLevel.RepeatableRead))
                    {
                        try
                        {
                            var existingRecord = uow.Session.Query<CarLoadingDailyQueue>()
                                .FirstOrDefault(x => x.RouteList.Id == routeListId && x.Date == date);

                            if(existingRecord != null)
                            {
                                return existingRecord.DailyNumber;
                            }

                            var maxNumber = uow.Session.Query<CarLoadingDailyQueue>()
                                .Where(x => x.Date == date)
                                .Max(x => (int?)x.DailyNumber) ?? 0;

                            var dailyNumber = maxNumber + 1;

                            var newRecord = new CarLoadingDailyQueue
                            {
                                RouteList = new RouteListEntity { Id = routeListId },
                                DailyNumber = dailyNumber,
                                Date = date
                            };

                            uow.Session.Save(newRecord);
                            transaction.Commit();

                            return dailyNumber;
                        }
                        catch(Exception ex) when((ex.InnerException as MySqlException)?.Number == 1062)
                        {
                            continue;
                        }
                    }
                }
			}

			return 0;
		}
	}
}
