using Dapper;
using DatabaseServiceWorker.Helpers;
using MySqlConnector;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseServiceWorker.PowerBiWorker
{
	internal partial class PowerBiExportWorker
	{
		private const int _timeOut = 120;
		private async Task ExportTablesAsync(
			MySqlConnection connectionSource,
			MySqlConnection connectionTarget,
			DateTime startDate,
			CancellationToken stoppingToken)
		{

			#region planPerDay

			// Пока уточнается, нужно ли
			//var planPerDaysSql = GetPlanPerDateSelectSql();
			//var planPerDayList = await connectionSource.GetDataAsync<dynamic>(planPerDaysSql, new { date = startDate });
			//var planPerDayInsertSql = GetPlanPerDateInsertSql();

			//var planPerDayTransaction = connectionTarget.BeginTransaction();
			//connectionTarget.Execute(planPerDayInsertSql, planPerDayList, planPerDayTransaction);
			//planPerDayTransaction.Commit();

			#endregion

			#region nomenclature

			var maxNomenclatureIdSql = GetMaxNomenclatureIdSelectSql();
			var maxNomenclatureInTarget = (await connectionTarget.GetDataAsync<int>(maxNomenclatureIdSql)).Single();
			var maxNomenclatureInSource = (await connectionSource.GetDataAsync<int>(maxNomenclatureIdSql)).Single();

			if(maxNomenclatureInSource > maxNomenclatureInTarget)
			{
				var nomenclaturesSql = GetNomenclatureSelectSql();
				var nomenclatureList = await connectionSource.GetDataAsync<dynamic>(nomenclaturesSql, new { id = maxNomenclatureInTarget });

				var nomenclatureInsertSql = GetInsertNomenclatureSql();
				var nomenclatureTransaction = connectionTarget.BeginTransaction();
				connectionTarget.Execute(nomenclatureInsertSql, nomenclatureList, nomenclatureTransaction, _timeOut);
				nomenclatureTransaction.Commit();
			}
			#endregion

			TruncateTables(connectionTarget, startDate);

			#region Orders

			var ordersSql = GetOrdersSelectSql();
			var orderList = await connectionSource.GetDataAsync<dynamic>(ordersSql, new { date = startDate });
			var orderInsertSql = GetInsertOrderSql();

			var orderTransaction = connectionTarget.BeginTransaction();
			connectionTarget.Execute(orderInsertSql, orderList, orderTransaction, _timeOut);
			orderTransaction.Commit();

			var orderItemsSql = GetOrderItemsSelectSql();
			var orderItemsList = await connectionSource.GetDataAsync<dynamic>(orderItemsSql, new { date = startDate });
			var orderItemsInsertSql = GetInsertOrderItemsSql();

			var orderItemTransaction = connectionTarget.BeginTransaction();
			connectionTarget.Execute(orderItemsInsertSql, orderItemsList, orderItemTransaction, _timeOut);
			orderItemTransaction.Commit();

			#endregion


			#region undelivered_orders

			var undeliveredOrdersSql = GetUndeliveredOrdersSelectSql();
			var undeliveredOrderList = await connectionSource.GetDataAsync<dynamic>(undeliveredOrdersSql, new { date = startDate });
			var undeliveredOrderInsertSql = GetUndeliveredOrdersInsertSql();

			var undeliveredOrderTransaction = connectionTarget.BeginTransaction();
			connectionTarget.Execute(undeliveredOrderInsertSql, undeliveredOrderList, undeliveredOrderTransaction, _timeOut);
			undeliveredOrderTransaction.Commit();

			#endregion
		}


		private void TruncateTables(MySqlConnection connectionTarget, DateTime truncateFromdate, int? maxNomenclatureId = null)
		{
			var truncateTransaction = connectionTarget.BeginTransaction();

			connectionTarget.Execute("delete from undelivered_orders where creation_date >= @date;", new { date = truncateFromdate }, truncateTransaction, _timeOut);
			connectionTarget.Execute("delete oi.* from order_items oi inner join orders o on o.id = oi.order_id where o.create_date >= @date;",
				new { date = truncateFromdate }, truncateTransaction, _timeOut);
			connectionTarget.Execute("delete from orders where create_date >= @date;", new { date = truncateFromdate }, truncateTransaction, _timeOut);			

			truncateTransaction.Commit();
		}

	}
}
