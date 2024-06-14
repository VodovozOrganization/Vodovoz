using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;

namespace DatabaseServiceWorker
{
	internal partial class PowerBiExportWorker
	{
		private IList<UndeliveredDto> GetUndelivered(IUnitOfWork uow, DateTime date)
		{
			var caseString = "CASE guilty.guilty_side" +
				"	WHEN 'Department' THEN IFNULL(CONCAT('Отд: ', subdivision.short_name), 'Отдел ВВ')" +
				"	WHEN 'Client' THEN 'Клиент'" +
				"	WHEN 'Driver' THEN 'Водитель'" +
				"	WHEN 'ServiceMan' THEN 'Мастер СЦ'" +
				"	WHEN 'ForceMajor' THEN 'Форс-мажор'" +
				"	WHEN 'DirectorLO' THEN 'Доставка за час'" +
				"	WHEN 'DirectorLOCurrentDayDelivery' THEN 'Доставка в тот же день'" +
				"	WHEN 'AutoСancelAutoTransfer' THEN 'Автоотмена автопереноса'" +
				"	WHEN 'None' THEN 'Нет (не недовоз)'" +
				"	ELSE guilty.guilty_side" +
				" END";

			var undeliveredSql =
				$"SELECT {caseString} AS Responsible," +
				$" COUNT({caseString}) AS Quantity," +
				" SUM((SELECT sum(IFNULL(order_items.count,0)) as y0_" +
				"	 FROM order_items order_items" +
				"	 LEFT JOIN nomenclature ON order_items.nomenclature_id = nomenclature.id" +
				"	 WHERE order_items.order_id = old_order.id AND (nomenclature.category = 'water' and nomenclature.tare_volume = 'Vol19L')))" +
				" AS Quantity19" +
				" FROM undelivered_orders uo" +
				" LEFT JOIN orders old_order ON uo.old_order_id = old_order.id" +
				" LEFT JOIN orders new_order ON uo.new_order_id = new_order.id" +
				" LEFT JOIN employees author ON uo.author_employee_id = author.id" +
				" LEFT JOIN guilty_in_undelivered_orders guilty ON uo.id = guilty.undelivery_id" +
				" LEFT JOIN counterparty counterparty ON old_order.client_id = counterparty.id" +
				" LEFT JOIN employees old_order_author ON old_order.author_employee_id = old_order_author.id" +
				" LEFT JOIN delivery_points ON old_order.delivery_point_id = delivery_points.id" +
				" LEFT JOIN subdivisions subdivision ON guilty.guilty_department_id = subdivision.id" +
				" WHERE old_order.delivery_date = :date" +
				" GROUP BY Responsible;";

			var undeliveredQuery = uow.Session
				.CreateSQLQuery(undeliveredSql)
				.SetParameter("date", date)
				.SetResultTransformer(Transformers.AliasToBean(typeof(UndeliveredDto)));

			return undeliveredQuery.List<UndeliveredDto>();
		}

		private DeliveredDto GetDelivered(IUnitOfWork uow, DateTime date)
		{
			var deliveredSql = "WITH order_items_cte AS" +
				" (SELECT order_items.order_id, SUM(order_items.count) AS count FROM order_items INNER JOIN orders on orders.id = order_items.order_id" +
				" INNER JOIN nomenclature ON order_items.nomenclature_id = nomenclature.id WHERE nomenclature.category = 'water' AND nomenclature.tare_volume = 'Vol19L'" +
				" AND orders.delivery_date = :date GROUP BY order_items.order_id)" +
				" SELECT CAST(SUM(order_sum.count) as SIGNED) as ShipmentDayPlan, COUNT(delivery_points_plan.compiled_address) as DeliveryPlan," +
				" CAST(SUM(IF(orders.order_status NOT IN ('DeliveryCanceled', 'NotDelivered'), order_sum.count, 0)) as SIGNED) as ShipmentDayFact," +
				" COUNT(delivery_points_fact.compiled_address) as DeliveryFact FROM orders" +
				" LEFT JOIN ( SELECT order_id, count FROM order_items_cte ) AS order_sum ON order_sum.order_id = orders.id" +
				" LEFT JOIN delivery_points AS delivery_points_plan ON delivery_point_id = delivery_points_plan.id" +
				" LEFT JOIN delivery_points delivery_points_fact ON orders.delivery_point_id = delivery_points_fact.id AND orders.order_status NOT IN ('DeliveryCanceled', 'NotDelivered')" +
				" LEFT JOIN delivery_schedule ON delivery_schedule.id = orders.delivery_schedule_id" +
				" LEFT JOIN route_list_addresses rla ON rla.order_id = orders.id" +
				" LEFT JOIN route_lists rl ON rl.id = rla.route_list_id" +
				" LEFT JOIN cars c ON c.id = rl.car_id LEFT JOIN car_models cm ON cm.id = c.model_id" +
				" WHERE orders.delivery_date = :date AND orders.order_status NOT IN ('Canceled', 'NewOrder', 'WaitForPayment') AND !orders.self_delivery" +
				" AND !orders.is_contract_closer AND orders.order_address_type != 'Service' AND delivery_schedule.id IS NOT NULL AND orders.delivery_point_id IS NOT NULL" +
				" AND rla.status <> 'Transfered' AND (rl.id IS NULL OR cm.car_type_of_use <> 'Truck')";

			var deliveredQuery = uow.Session
				.CreateSQLQuery(deliveredSql)
				.SetParameter("date", date)
				.SetResultTransformer(Transformers.AliasToBean(typeof(DeliveredDto)));

			return deliveredQuery.UniqueResult<DeliveredDto>();
		}

		private decimal GetRevenues(IUnitOfWork uow, DateTime date)
		{

			var revenueSql = "SELECT SUM(TRUNCATE(IFNULL(order_items.actual_count, order_items.count) * order_items.price - order_items.discount_money, 2)) AS revenueDay FROM order_items" +
				" LEFT JOIN orders ON order_items.order_id = orders.id" +
				" LEFT JOIN nomenclature ON order_items.nomenclature_id = nomenclature.id" +
				" WHERE orders.delivery_date = :date AND (order_status = 'Accepted' OR order_status = 'InTravelList' OR order_status = 'OnLoading' OR order_status = 'OnTheWay' OR order_status = 'Shipped'" +
				" OR order_status = 'UnloadingOnStock' OR order_status = 'Closed' OR (order_status = 'WaitForPayment' AND self_delivery AND pay_after_shipment))" +
				" AND !orders.is_contract_closer AND nomenclature.category = 'water' AND !nomenclature.is_disposable_tare";

			var revenueQuery = uow.Session
				.CreateSQLQuery(revenueSql)
				.SetParameter("date", date);

			return revenueQuery.UniqueResult<decimal>();
		}

		private decimal GetNumberOfFastDeliverySales(IUnitOfWork uow, DateTime date)
		{
			var sql = "SELECT SUM(IFNULL(oi.actual_count, oi.count)) AS total_count" +
				" FROM order_items oi INNER JOIN orders o on o.id = oi.order_id" +
				" INNER JOIN base_parameters bp on bp.str_value = oi.nomenclature_id" +
				" WHERE bp.name = 'fast_delivery_nomenclature_id'" +
				" AND  (o.order_status = 'Accepted' OR o.order_status = 'InTravelList'" +
				" OR o.order_status = 'OnLoading'" +
				" OR o.order_status = 'OnTheWay'" +
				" OR o.order_status = 'Shipped'" +
				" OR o.order_status = 'UnloadingOnStock' " +
				" OR o.order_status = 'Closed'" +
				" OR (o.order_status = 'WaitForPayment' AND o.self_delivery AND o.pay_after_shipment))" +
				" AND (delivery_date = :date) AND !o.is_contract_closer;";

			var query = uow.Session
				.CreateSQLQuery(sql)
				.SetParameter("date", date);

			return query.UniqueResult<int>();
		}

		private LateDto GetLates(IUnitOfWork uow, DateTime date)
		{
			var sql = "SET @interval_select_mode = (SELECT str_value AS value from base_parameters bp WHERE bp.name = 'FastDeliveryIntervalFrom');" +
				" SELECTSum(Delays.delay < '00:05:00') AS LessThan5Minutes," +
				" Sum(Delays.delay < '00:30:00' AND Delays.delay >= '00:05:00') AS LessThan30Minutes," +
				" Sum(Delays.delay >= '00:30:00') AS MoreThan30Minutes" +
				" FROM (SELECT IF(is_fast_delivery," +
				" TIMEDIFF(IFNULL(orders.time_delivered, route_list_addresses.status_last_update)," +
				"	(IF(@interval_select_mode = 'Transfer', route_list_addresses.creation_date, IF(@interval_select_mode = 'FirstAddress', " +
				"		(SELECT rla.creation_date FROM route_list_addresses rla WHERE rla.order_id = orders.id ORDER BY creation_date LIMIT 1), orders.create_date" +
				" )) + INTERVAL (SELECT str_value FROM base_parameters WHERE base_parameters.name = 'fast_delivery_time') HOUR ))," +
				" TIMEDIFF(TIME(IFNULL(orders.time_delivered, route_list_addresses.status_last_update)), delivery_schedule.to_time)) AS delay" +
				" FROM route_lists INNER JOIN employees driver ON driver.id = route_lists.driver_id AND driver.visiting_master = 0" +
				" LEFT JOIN route_list_addresses ON route_list_addresses.route_list_id = route_lists.id" +
				" LEFT JOIN orders ON orders.id = route_list_addresses.order_id" +
				" LEFT JOIN delivery_schedule ON delivery_schedule.id = orders.delivery_schedule_id" +
				" INNER JOIN cars c ON route_lists.car_id = c.id" +
				" INNER JOIN car_models cm ON c.model_id = cm.id" +
				" INNER JOIN car_versions cv ON c.id = cv.car_id" +
				" AND cv.start_date <= route_lists.date" +
				" AND (cv.end_date IS NULL OR cv.end_date >= route_lists.date)" +
				" WHERE route_lists.`date` = :date AND is_fast_delivery = 1 AND route_list_addresses.status = 'Completed'" +
				" GROUP BY route_list_addresses.id HAVING delay > 0) AS Delays;";

			var query = uow.Session
				.CreateSQLQuery(sql)
				.SetParameter("date", date);

			return query.UniqueResult<LateDto>();			
		}
	}
}
