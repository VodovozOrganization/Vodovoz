using MySqlConnector;
using QS.DomainModel.UoW;
using System;
using System.Data;

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
				using(var transaction = uow.Session.BeginTransaction(IsolationLevel.RepeatableRead))
				{
					var existingNumberSql = @"SELECT daily_number FROM car_loading_daily_queue WHERE route_list_id = :route_list_id and date = :date;";
					var existingNumberQuery = uow.Session.CreateSQLQuery(existingNumberSql)
						.SetParameter("route_list_id", routeListId)
						.SetParameter("date", date);

					var dailyNumber = (int)existingNumberQuery.UniqueResult<uint>();

					if(dailyNumber > 0)
					{
						return dailyNumber;
					}

					var maxNumberSql = @"SELECT MAX(daily_number) FROM car_loading_daily_queue WHERE date = :date;";
					var maxNumberQuery = uow.Session.CreateSQLQuery(maxNumberSql)
						.SetParameter("date", date);

					dailyNumber = (int)maxNumberQuery.UniqueResult<uint>();
					dailyNumber++;

					try
					{
						var insertNumberSql = @"INSERT INTO car_loading_daily_queue (route_list_id, daily_number, date) VALUES(:route_list_id, :daily_number, :date);";
						uow.Session.CreateSQLQuery(insertNumberSql)
							.SetParameter("route_list_id", routeListId)
							.SetParameter("daily_number", dailyNumber)
							.SetParameter("date", date)
							.ExecuteUpdate();

						transaction.Commit();
					}
					catch(Exception ex) when((ex.InnerException as MySqlException)?.Number == 1062)
					{
						continue;
					}

					return dailyNumber;
				}
			}

			return 0;
		}
	}
}
