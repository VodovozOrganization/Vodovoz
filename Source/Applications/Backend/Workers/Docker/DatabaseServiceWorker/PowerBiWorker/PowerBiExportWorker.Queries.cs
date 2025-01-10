using DatabaseServiceWorker.PowerBiWorker.Dto;
using QS.DomainModel.UoW;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Delivery;
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;
using static Vodovoz.ViewModels.ViewModels.Reports.FastDelivery.FastDeliveryAdditionalLoadingReportViewModel;


namespace DatabaseServiceWorker.PowerBiWorker
{
	internal partial class PowerBiExportWorker
	{
		#region Reports

		private DateTime GetStartDate(DateTime optionsStartDate, int numberOfDaysToUpdate)
		{
			var updateDate = DateTime.Today.AddDays(-numberOfDaysToUpdate);
			var startDate = optionsStartDate > updateDate ? optionsStartDate : updateDate;
			return startDate;
		}

		private string GetUndeliveredSql(DateTime startDate, DateTime endDate)
		{
			var sqlBuilder = new StringBuilder();

			var caseString =
				@$"CASE guilty.guilty_side
					WHEN '{nameof(GuiltyTypes.Department)}' THEN IFNULL(CONCAT('Отд: ', subdivision.short_name), 'Отдел ВВ')
					WHEN '{nameof(GuiltyTypes.Client)}' THEN 'Клиент'
					WHEN '{nameof(GuiltyTypes.Driver)}' THEN 'Водитель'
					WHEN '{nameof(GuiltyTypes.ServiceMan)}' THEN 'Мастер СЦ'
					WHEN '{nameof(GuiltyTypes.ForceMajor)}' THEN 'Форс-мажор'
					WHEN '{nameof(GuiltyTypes.DirectorLO)}' THEN 'Доставка за час'
					WHEN '{nameof(GuiltyTypes.DirectorLOCurrentDayDelivery)}' THEN 'Довезли в тот же день'
					WHEN '{nameof(GuiltyTypes.AutoСancelAutoTransfer)}' THEN 'Автоотмена автопереноса'
					WHEN '{nameof(GuiltyTypes.None)}' THEN 'Нет (не недовоз)'
					ELSE guilty.guilty_side
				END";

			var sql =
				@$"SELECT /*DATE*/ AS date, {caseString} AS Responsible,
				 COUNT({caseString}) AS Quantity,
				 SUM((SELECT sum(IFNULL(order_items.count,0)) as y0_
					 FROM order_items order_items
					 LEFT JOIN nomenclature ON order_items.nomenclature_id = nomenclature.id
					 WHERE order_items.order_id = old_order.id AND (nomenclature.category = '{nameof(NomenclatureCategory.water)}'
					 AND nomenclature.tare_volume = '{nameof(TareVolume.Vol19L)}')))
				 AS Quantity19
				 FROM undelivered_orders uo
				 LEFT JOIN orders old_order ON uo.old_order_id = old_order.id
				 LEFT JOIN orders new_order ON uo.new_order_id = new_order.id
				 LEFT JOIN employees author ON uo.author_employee_id = author.id
				 LEFT JOIN guilty_in_undelivered_orders guilty ON uo.id = guilty.undelivery_id
				 LEFT JOIN counterparty counterparty ON old_order.client_id = counterparty.id
				 LEFT JOIN employees old_order_author ON old_order.author_employee_id = old_order_author.id
				 LEFT JOIN delivery_points ON old_order.delivery_point_id = delivery_points.id
				 LEFT JOIN subdivisions subdivision ON guilty.guilty_department_id = subdivision.id
				 WHERE old_order.delivery_date = /*DATE*/
				 GROUP BY Responsible;";

			for(DateTime date = startDate; date < endDate; date = date.AddDays(1))
			{
				var sqlForDate = sql.Replace("/*DATE*/", $"'{date.ToString("yyyy-MM-dd")}'");
				sqlBuilder.AppendLine(sqlForDate);
			}

			return sqlBuilder.ToString();
		}

		private string GetDeliveredSql(DateTime startDate, DateTime endDate)
		{
			var sqlBuilder = new StringBuilder();

			var sql = @$"WITH order_items_cte AS
				(SELECT order_items.order_id, SUM(order_items.count) AS count FROM order_items INNER JOIN orders on orders.id = order_items.order_id
				INNER JOIN nomenclature ON order_items.nomenclature_id = nomenclature.id WHERE nomenclature.category = '{nameof(NomenclatureCategory.water)}'
				AND nomenclature.tare_volume = '{nameof(TareVolume.Vol19L)}'
				AND orders.delivery_date = /*DATE*/ GROUP BY order_items.order_id)
				SELECT /*DATE*/ as date, CAST(SUM(order_sum.count) as SIGNED) as ShipmentDayPlan, COUNT(delivery_points_plan.compiled_address) as DeliveryPlan,
				CAST(SUM(IF(orders.order_status NOT IN ('{nameof(OrderStatus.DeliveryCanceled)}', '{nameof(OrderStatus.NotDelivered)}'),
				order_sum.count, 0)) as SIGNED) as ShipmentDayFact,
				COUNT(delivery_points_fact.compiled_address) as DeliveryFact FROM orders
				LEFT JOIN ( SELECT order_id, count FROM order_items_cte ) AS order_sum ON order_sum.order_id = orders.id
				LEFT JOIN delivery_points AS delivery_points_plan ON delivery_point_id = delivery_points_plan.id
				LEFT JOIN delivery_points delivery_points_fact ON orders.delivery_point_id = delivery_points_fact.id AND orders.order_status
				NOT IN ('{nameof(OrderStatus.DeliveryCanceled)}', '{nameof(OrderStatus.NotDelivered)}')
				LEFT JOIN delivery_schedule ON delivery_schedule.id = orders.delivery_schedule_id
				LEFT JOIN route_list_addresses rla ON rla.order_id = orders.id
				LEFT JOIN route_lists rl ON rl.id = rla.route_list_id
				LEFT JOIN cars c ON c.id = rl.car_id LEFT JOIN car_models cm ON cm.id = c.model_id
				WHERE orders.delivery_date = /*DATE*/ AND orders.order_status NOT IN ('{nameof(OrderStatus.Canceled)}', '{nameof(OrderStatus.NewOrder)}', '{nameof(OrderStatus.WaitForPayment)}')
				AND !orders.self_delivery
				AND !orders.is_contract_closer AND orders.order_address_type != '{nameof(OrderAddressType.Service)}' AND delivery_schedule.id IS NOT NULL
				AND orders.delivery_point_id IS NOT NULL
				AND rla.status <> 'Transfered' AND (rl.id IS NULL OR  cm.car_type_of_use <> '{nameof(CarTypeOfUse.Truck)}');";

			for(DateTime date = startDate; date < endDate; date = date.AddDays(1))
			{
				var sqlForDate = sql.Replace("/*DATE*/", $"'{date.ToString("yyyy-MM-dd")}'");
				sqlBuilder.AppendLine(sqlForDate);
			}

			return sqlBuilder.ToString();
		}

		private string GetRevenuesSql(DateTime startDate, DateTime endDate)
		{

			var sqlBuilder = new StringBuilder();

			var sql = @$"SELECT /*DATE*/ as date, SUM(TRUNCATE(IFNULL(order_items.actual_count, order_items.count) * order_items.price - order_items.discount_money, 2)) AS revenueDay FROM order_items
				LEFT JOIN orders ON order_items.order_id = orders.id
				LEFT JOIN nomenclature ON order_items.nomenclature_id = nomenclature.id
				WHERE orders.delivery_date = /*DATE*/ AND(order_status = '{nameof(OrderStatus.Accepted)}'OR order_status = '{nameof(OrderStatus.InTravelList)}' OR order_status = '{nameof(OrderStatus.OnLoading)}'
				OR order_status = '{nameof(OrderStatus.OnTheWay)}' OR order_status = '{nameof(OrderStatus.Shipped)}'
				OR order_status = '{nameof(OrderStatus.UnloadingOnStock)}' OR order_status = '{nameof(OrderStatus.Closed)}' OR (order_status = '{nameof(OrderStatus.WaitForPayment)}' AND self_delivery AND pay_after_shipment))
				AND nomenclature.category = 'water'				 
				AND !nomenclature.is_disposable_tare
				AND !orders.is_contract_closer;";

			for(DateTime date = startDate; date < endDate; date = date.AddDays(1))
			{
				var sqlForDate = sql.Replace("/*DATE*/", $"'{date.ToString("yyyy-MM-dd")}'");
				sqlBuilder.AppendLine(sqlForDate);
			}
			return sqlBuilder.ToString();
		}
		private string GetNumberOfFastDeliverySalesSql(DateTime startDate, DateTime endDate)
		{
			var sqlBuilder = new StringBuilder();
			var sql = @$"SELECT /*DATE*/ AS date, SUM(IFNULL(oi.actual_count, oi.count)) AS total_count
			FROM order_items oi INNER JOIN orders o on o.id = oi.order_id
			INNER JOIN base_parameters bp on bp.str_value = oi.nomenclature_id
			WHERE bp.name = 'fast_delivery_nomenclature_id'
				AND  (order_status = '{nameof(OrderStatus.Accepted)}'OR order_status = '{nameof(OrderStatus.InTravelList)}' OR order_status = '{nameof(OrderStatus.OnLoading)}'
				OR order_status = '{nameof(OrderStatus.OnTheWay)}' OR order_status = '{nameof(OrderStatus.Shipped)}'
				OR order_status = '{nameof(OrderStatus.UnloadingOnStock)}' OR order_status = '{nameof(OrderStatus.Closed)}' OR(order_status = '{nameof(OrderStatus.WaitForPayment)}' AND o.self_delivery AND o.pay_after_shipment))
				AND (delivery_date =  /*DATE*/) AND !o.is_contract_closer;";

			for(DateTime date = startDate; date <= endDate; date = date.AddDays(1))
			{
				var sqlForDate = sql.Replace("/*DATE*/", $"'{date.ToString("yyyy-MM-dd")}'");
				sqlBuilder.AppendLine(sqlForDate);
			}

			return sqlBuilder.ToString();
		}

		private string GetLatesSql(DateTime startDate, DateTime endDate)
		{
			var sqlBuilder = new StringBuilder();

			var sql = $@"SELECT /*DATE*/ AS date, Sum(Delays.delay < '00:05:00') AS LessThan5Minutes,
							Sum(Delays.delay < '00:30:00' AND Delays.delay >= '00:05:00') AS LessThan30Minutes,
							Sum(Delays.delay >= '00:30:00') AS MoreThan30Minutes
							FROM (SELECT IF(is_fast_delivery,
							TIMEDIFF(IFNULL(orders.time_delivered, route_list_addresses.status_last_update),
								(IF(@interval_select_mode = 'RouteListItemTransfered', route_list_addresses.creation_date, IF(@interval_select_mode =
									'{nameof(FastDeliveryIntervalFromEnum.AddedInFirstRouteList)}', 
									(SELECT rla.creation_date FROM route_list_addresses rla WHERE rla.order_id = orders.id ORDER BY creation_date LIMIT 1), orders.create_date
							)) + INTERVAL (SELECT str_value FROM base_parameters WHERE base_parameters.name = 'fast_delivery_time') HOUR )),
							TIMEDIFF(TIME(IFNULL(orders.time_delivered, route_list_addresses.status_last_update)), delivery_schedule.to_time)) AS delay
							FROM route_lists INNER JOIN employees driver ON driver.id = route_lists.driver_id AND driver.visiting_master = 0
							LEFT JOIN route_list_addresses ON route_list_addresses.route_list_id = route_lists.id
							LEFT JOIN orders ON orders.id = route_list_addresses.order_id
							LEFT JOIN delivery_schedule ON delivery_schedule.id = orders.delivery_schedule_id
							INNER JOIN cars c ON route_lists.car_id = c.id
							INNER JOIN car_models cm ON c.model_id = cm.id
							INNER JOIN car_versions cv ON c.id = cv.car_id
							AND cv.start_date <= route_lists.date
							AND (cv.end_date IS NULL OR cv.end_date >= route_lists.date)
							WHERE route_lists.`date` = /*DATE*/ AND is_fast_delivery = 1 AND route_list_addresses.status = '{nameof(RouteListItemStatus.Completed)}'
							GROUP BY route_list_addresses.id HAVING delay > 0) AS Delays;";

			for(DateTime date = startDate; date <= endDate; date = date.AddDays(1))
			{
				var sqlForDate = sql.Replace("/*DATE*/", $"'{date.ToString("yyyy-MM-dd")}'");
				sqlBuilder.AppendLine(sqlForDate);
			}

			return sqlBuilder.ToString();
		}

		private string GetFastDeliveryUndeliveriesSql(DateTime startDate, DateTime endDate)
		{
			var sqlBuilder = new StringBuilder();

			var sql = "SELECT /*DATE*/ as date," +
				" COUNT(uo.id) as total" +
				" FROM" +
				" undelivered_orders uo" +
				" INNER JOIN orders old_order ON uo.old_order_id = old_order.id" +
				" WHERE old_order.delivery_date = /*DATE*/" +
				" AND old_order.is_fast_delivery = true;";

			for(DateTime date = startDate; date <= endDate; date = date.AddDays(1))
			{
				var sqlForDate = sql.Replace("/*DATE*/", $"'{date.ToString("yyyy-MM-dd")}'");
				sqlBuilder.AppendLine(sqlForDate);
			}

			return sqlBuilder.ToString();
		}

		private string GeNumberOfFastdeliveryComplaintsSql(DateTime startDate, DateTime endDate)
		{
			var sqlBuilder = new StringBuilder();

			var sql = "SELECT /*DATE*/ as date, COUNT(id) AS total" +
				" FROM complaints" +
				" WHERE	DATE(creation_date) = /*DATE*/" +
				" AND complaint_kind_id = 92;";

			for(DateTime date = startDate; date <= endDate; date = date.AddDays(1))
			{
				var sqlForDate = sql.Replace("/*DATE*/", $"'{date.ToString("yyyy-MM-dd")}'");
				sqlBuilder.AppendLine(sqlForDate);
			}

			return sqlBuilder.ToString();
		}

		private async Task<CoverageDto> GetCoverageAsync(
			IDeliveryRulesSettings deliveryRulesSettings,
			IUnitOfWork uow,
			IDeliveryRepository deliveryRepository,
			ITrackRepository trackRepository,
			IScheduleRestrictionRepository scheduleRestrictionRepository,
			DateTime date,
			CancellationToken cancellationToken)
		{
			var startTime = new TimeSpan(10, 0, 0);
			var endTime = new TimeSpan(21, 0, 0);

			var grouping = await FastDeliveryPercentCoverageReport.GetData(
				uow,
				date,
				date,
				startTime,
				endTime,
				deliveryRulesSettings,
				deliveryRepository,
				trackRepository,
				scheduleRestrictionRepository,
				cancellationToken);

			var report = FastDeliveryPercentCoverageReport.Create(date, date, startTime, endTime, grouping);

			return new CoverageDto
			{
				Fill = report.Grouping.PercentCoverage * 100,
				AverageRadius = report.Grouping.ServiceRadius,
				NumberOfCars = report.Grouping.CarsCount
			};
		}

		private string GetFastDeliveryFails(DateTime startDate, DateTime endDate)
		{
			var sqlBuilder = new StringBuilder();

			var sql = "WITH CTE AS (SELECT h.delivery_point_id, i.is_valid_distance_by_line_to_client, i.is_valid_is_goods_enough, i.is_valid_last_coordinate_time, i.is_valid_unclosed_fast_deliveries" +
				" FROM fast_delivery_availability_history h LEFT JOIN fast_delivery_availability_history_items i on h.id = i.fast_delivery_availability_history_id" +
				" WHERE EXISTS (SELECT o.id FROM orders o WHERE o.delivery_point_id = h.delivery_point_id and DATE(o.create_date) = DATE(h.verification_date) and o.is_fast_delivery = False)" +
				" AND NOT EXISTS ( SELECT i2.id FROM fast_delivery_availability_history_items i2 WHERE i2.fast_delivery_availability_history_id = h.id and i2.is_valid_to_fast_delivery = True)" +
				" AND NOT EXISTS ( SELECT i3.id FROM fast_delivery_availability_history_items i3 WHERE i3.fast_delivery_availability_history_id = (SELECT h2.id FROM fast_delivery_availability_history h2" +
				" WHERE h2.delivery_point_id = h.delivery_point_id and DATE(h2.verification_date) = DATE(h.verification_date) ORDER BY h2.id desc limit 1)" +
				" AND i3.is_valid_to_fast_delivery = True) AND (h.verification_date >= /*START_DATE*/ AND h.verification_date <= /*END_DATE*/) AND ( SELECT (case WHEN MAX(h3.nomenclature_id) > 0 then True else False end)" +
				" FROM fast_delivery_order_items_history h3 inner join nomenclature n on h3.nomenclature_id = n.id WHERE h3.fast_delivery_availability_history_id = h.id AND NOT (n.group_id = @product_group_id)" +
				" AND NOT EXISTS ( SELECT d.nomenclature_id FROM fast_delivery_nomenclature_distribution_history d WHERE d.fast_delivery_availability_history_id = h.id AND h3.nomenclature_id = d.nomenclature_id)) = False)" +
				" SELECT /*START_DATE*/ AS date, SUM(is_valid_is_goods_enough_total) AS IsValidIsGoodsEnoughTotal, SUM(is_valid_last_coordinate_time_total) AS IsValidLastCoordinateTimeTotal," +
				" SUM(is_valid_unclosed_fast_deliveries_total) AS IsValidUnclosedFastDeliveriesTotal, SUM(is_valid_distance_by_line_to_client_total) AS IsValidDistanceByLineToClientTotal" +
				" FROM (SELECT SUM(is_valid_is_goods_enough) AS is_valid_is_goods_enough_total, SUM(is_valid_last_coordinate_time) AS is_valid_last_coordinate_time_total," +
				" SUM(is_valid_unclosed_fast_deliveries) AS is_valid_unclosed_fast_deliveries_total, 0 as is_valid_distance_by_line_to_client_total" +
				" FROM ( SELECT MIN(is_valid_is_goods_enough) = 0 AS is_valid_is_goods_enough, MIN(is_valid_last_coordinate_time) = 0 AS is_valid_last_coordinate_time," +
				" MIN(is_valid_unclosed_fast_deliveries) = 0 AS is_valid_unclosed_fast_deliveries FROM cte WHERE is_valid_distance_by_line_to_client = 1 GROUP by cte.delivery_point_id ) t1" +
				" UNION ALL SELECT  0 AS is_valid_is_goods_enough_total, 0 AS is_valid_last_coordinate_time_total, 0 AS is_valid_unclosed_fast_deliveries_total, (SELECT COUNT(delivery_point_id)" +
				" FROM (SELECT cte.delivery_point_id FROM cte GROUP by cte.delivery_point_id HAVING max(cte.is_valid_distance_by_line_to_client) = 0) AS t2) AS is_valid_distance_by_line_to_client_total ) AS total; ";

			for(DateTime date = startDate; date <= endDate; date = date.AddDays(1))
			{
				var sqlForDate = sql
					.Replace("/*START_DATE*/", $"'{date.Add(new TimeSpan(0, 0, 0)).ToString("yyyy-MM-dd")}'")
					.Replace("/*END_DATE*/", $"'{date.Add(new TimeSpan(23, 59, 59)).ToString("yyyy-MM-dd HH:mm:ss")}'")
					;

				sqlBuilder.AppendLine(sqlForDate);
			}

			return sqlBuilder.ToString();
		}

		private RemainingBottlesDto GetRemainingBottle(IUnitOfWork uow, DateTime date)
		{
			var dateFrom = date.Date.Add(new TimeSpan(0, 0, 0));
			var dateTo = date.Date.Add(new TimeSpan(23, 59, 59));
			var report = RemainingBottlesReport.Generate(uow, dateFrom, dateTo);
			return new RemainingBottlesDto
			{
				Uploaded19 = report.Rows.Sum(x => x.BottlesLoadedAdditionallyCount),
				Sold19 = report.Rows.Sum(x => x.BottlesShippedFastDeliveryCount),
				Return19 = report.Rows.Sum(x => x.RemainingBottlesCount),
			};
		}

		#endregion

		#region tables

		private string GetOrdersSelectSql()
		{
			var sql = @$"SELECT * FROM orders where create_date > @date;";

			return sql;
		}

		private string GetInsertOrderSql()
		{
			var sql = @"INSERT INTO orders (id, version, create_date, client_id, delivery_point_id, delivery_schedule_id, is_first_order, 
				bottles_return, delivery_date, extra_money, sum_to_receive, order_status, payment_type, self_delivery, shipped,
				collect_bottles, bottles_movement_operation_id, money_movement_operation_id, counterparty_contract_id,
				document_type, code1c, daily_number_1c, address_1c, delivery_schedule_1c, time_delivered, last_edited_time,
				driver_call_type, service, trifle, bill_date, 
				is_contract_closer, is_reason_type_changed_by_user, has_comment_for_driver, pay_after_shipment, order_source,
				add_certificates, tare_non_return_reason_id, is_bottle_stock, is_bottle_stock_discrepancy, bottles_by_stock_count, bottles_by_stock_actual_count, 
				payment_by_sms, contactless_delivery, order_payment_status, need_terminal, 
				is_for_retail, is_self_delivery_paid, order_address_type, is_fast_delivery, is_copied_from_undelivery, payment_by_qr, 
				is_second_order, 
				first_delivery_date, is_do_not_make_call_before_arrival, dont_arrive_before_interval)
				VALUES (@id, @version, @create_date, IFNULL(@client_id, 0), IFNULL(@delivery_point_id, 0), IFNULL(@delivery_schedule_id, 0), IFNULL(@is_first_order, 0), 
				IFNULL(@bottles_return, 0), @delivery_date, IFNULL(@extra_money, 0), IFNULL(@sum_to_receive, 0), @order_status, @payment_type, IFNULL(@self_delivery, 0), IFNULL(@shipped, 0),
				IFNULL(@collect_bottles, 0), IFNULL(@bottles_movement_operation_id, 0), IFNULL(@money_movement_operation_id, 0), IFNULL(@counterparty_contract_id, 0),
				@document_type, IFNULL(@code1c, 0), IFNULL(@daily_number_1c, 0), IFNULL(@address_1c, 0), IFNULL(@delivery_schedule_1c, 0), @time_delivered, @last_edited_time,
				@driver_call_type, IFNULL(@service, 0), IFNULL(@trifle, 0), @bill_date, 
				IFNULL(@is_contract_closer, 0), @is_reason_type_changed_by_user, IFNULL(@has_comment_for_driver, 0), IFNULL(@pay_after_shipment, 0), @order_source,
				IFNULL(@add_certificates, 0), IFNULL(@tare_non_return_reason_id, 0), IFNULL(@is_bottle_stock, 0), IFNULL(@is_bottle_stock_discrepancy, 0), IFNULL(@bottles_by_stock_count, 0), IFNULL(@bottles_by_stock_actual_count, 0), 
				IFNULL(@payment_by_sms, 0), IFNULL(@contactless_delivery, 0), @order_payment_status, IFNULL(@need_terminal, 0), 
				IFNULL(@is_for_retail, 0), IFNULL(@is_self_delivery_paid, 0), @order_address_type, IFNULL(@is_fast_delivery, 0), IFNULL(@is_copied_from_undelivery, 0), IFNULL(@payment_by_qr, 0), 
				IFNULL(@is_second_order, 0), 
				@first_delivery_date, IFNULL(@is_do_not_make_call_before_arrival, 0), IFNULL(@dont_arrive_before_interval, 0))";

			return sql;
		}

		private string GetOrderItemsSelectSql()
		{
			var sql = @$"select oi.* from order_items oi inner join orders o on o.id = oi.order_id where o.create_date > @date;";

			return sql;
		}

		private string GetInsertOrderItemsSql()
		{
			var sql = @"INSERT INTO order_items (id, nomenclature_id, order_id, price, count, actual_count,
				include_nds, counterparty_movement_operation_id, is_discount_in_money, discount, discount_money, discount_by_stock,
				is_user_price, value_added_tax, rent_type,
				rent_sub_type, rent_count, rent_equipment_count, is_alternative_price)
				VALUES(IFNULL(@id, 0), IFNULL(@nomenclature_id, 0), IFNULL(@order_id, 0), IFNULL(@price, 0), IFNULL(@count, 0), IFNULL(@actual_count, 0), IFNULL(@include_nds, 0), IFNULL(@counterparty_movement_operation_id, 0),
				IFNULL(@is_discount_in_money, 0), IFNULL(@discount, 0), IFNULL(@discount_money, 0), IFNULL(@discount_by_stock, 0), IFNULL(@is_user_price, 0), IFNULL(@value_added_tax, 0),
				@rent_type, @rent_sub_type, IFNULL(@rent_count, 0), IFNULL(@rent_equipment_count, 0),
				IFNULL(@is_alternative_price, 0));";

			return sql;
		}

		private string GetNomenclatureSelectSql()
		{
			var sql = @$"select * from nomenclature where id > @id;";

			return sql;
		}

		private string GetMaxNomenclatureIdSelectSql()
		{
			var sql = @$"select max(id) from nomenclature;";

			return sql;
		}

		private string GetInsertNomenclatureSql()
		{
			var sql = @"INSERT INTO nomenclature
				(id, create_date, created_by, name, official_name, is_archive, model, weight, volume, `length`, width, height, vat, reserve, serial, unit_id, category,
				is_disposable_tare, tare_volume, subtype_of_equipment, type_of_deposit, color_id, route_column_id, kind_id, manufacturer_id, warehouse_id, code_1c, folder_1c_id, rent_priority,
				sum_of_damage, short_name, very_short_name, hide, no_delivery, is_duty, is_new_bottle, is_defective_bottle, can_print_price, depends_on_nomenclature, is_diler, is_shabby_bottle,
				percent_for_master, group_id, online_store_guid, min_stock_count, mobile_catalog, description, color, material, liters, `size`, package, degree_of_roast, smell, taste,
				refrigerator_capacity, cooling_type, heating_power, cooling_power, heating_performance, cooling_performance, number_of_cartridges, characteristics_of_cartridges, country_of_origin,
				amount_in_a_package, fuel_type_id, storage_cell, shipper_counterparty_id, purchase_price, bottle_cap_color, online_store_id, online_store_external_id, plan_day, plan_month,
				using_in_group_price_set, is_accountable_in_chestniy_znak, gtin, has_inventory_accounting, online_name, is_sparkling_water, equipment_installation_type, equipment_workload_type,
				pump_type, cup_holder_bracing_type, has_heating, new_heating_power, heating_productivity, protection_on_hot_water_tap, has_cooling, new_cooling_power, cooling_productivity,
				new_cooling_type, locker_refrigerator_type, locker_refrigerator_volume, tap_type, glass_holder_type, mobile_app_nomenclature_online_catalog_id,
				vodovoz_web_site_nomenclature_online_catalog_id, kuler_sale_web_site_nomenclature_online_catalog_id, nomenclature_online_group_id, nomenclature_online_category_id)
				VALUES(@id, @create_date, @created_by, @name, @official_name, @is_archive, @model, @weight, @volume, @`length`, @width, @height, @vat, @reserve, @serial, @unit_id, @category,
				@is_disposable_tare, @tare_volume, @subtype_of_equipment, @type_of_deposit, @color_id, @route_column_id, @kind_id, @manufacturer_id, @warehouse_id, @code_1c, @folder_1c_id,
				@rent_priority, @sum_of_damage, @short_name, @very_short_name, @hide, @no_delivery, @is_duty, @is_new_bottle, @is_defective_bottle, @can_print_price, @depends_on_nomenclature,
				@is_diler, @is_shabby_bottle, @percent_for_master, @group_id, @online_store_guid, @min_stock_count, @mobile_catalog, @description, @color, @material, @liters, @`size`,
				@package, @degree_of_roast, @smell, @taste, @refrigerator_capacity, @cooling_type, @heating_power, @cooling_power, @heating_performance, @cooling_performance, @number_of_cartridges,
				@characteristics_of_cartridges, @country_of_origin, @amount_in_a_package, @fuel_type_id, @storage_cell, @shipper_counterparty_id, @purchase_price, @bottle_cap_color,
				@online_store_id, @online_store_external_id, @plan_day, @plan_month, @using_in_group_price_set, @is_accountable_in_chestniy_znak, @gtin, @has_inventory_accounting,
				@online_name, @is_sparkling_water, @equipment_installation_type, @equipment_workload_type, @pump_type, @cup_holder_bracing_type, @has_heating, @new_heating_power,
				@heating_productivity, @protection_on_hot_water_tap, @has_cooling, @new_cooling_power, @cooling_productivity, @new_cooling_type, @locker_refrigerator_type,
				@locker_refrigerator_volume, @tap_type, @glass_holder_type, @mobile_app_nomenclature_online_catalog_id, @vodovoz_web_site_nomenclature_online_catalog_id,
				@kuler_sale_web_site_nomenclature_online_catalog_id, @nomenclature_online_group_id, @nomenclature_online_category_id);";

			return sql;
		}

		private string GetUndeliveredOrdersSelectSql()
		{
			var sql = @$"select * from undelivered_orders where creation_date > @date;";

			return sql;
		}

		private string GetUndeliveredOrdersInsertSql()
		{
			var sql = @"INSERT INTO undelivered_orders (id, old_order_id, new_order_id, guilty_is, guilty_department_id, dispatcher_call_time, driver_call_nr, driver_call_type,
				driver_call_time, reason, creation_date, registered_by_employee_id, author_employee_id, last_edited_time, editor_employee_id, status,
				undelivered_order_status, in_process_at, transfer_type, undelivery_problem_source_id, undelivery_transfer_absence_reason_id, undelivery_detalization_id)
				VALUES(@id, @old_order_id, @new_order_id, @guilty_is, @guilty_department_id, @dispatcher_call_time, @driver_call_nr, @driver_call_type, @driver_call_time,
				@reason, @creation_date, @registered_by_employee_id, @author_employee_id, @last_edited_time, @editor_employee_id, @status, @undelivered_order_status, @in_process_at,
				@transfer_type, @undelivery_problem_source_id, @undelivery_transfer_absence_reason_id, @undelivery_detalization_id);"
			;
			return sql;
		}

		private string GetGuiltyInUndeliveredOrdersSelectSql()
		{
			var sql = @$"select g.* from guilty_in_undelivered_orders g inner join undelivered_orders u on u.id = g.undelivery_id
						where u.creation_date > @date;";

			return sql;
		}

		private string GetGuiltyInUndeliveredOrdersInsertSql()
		{
			var sql = @"INSERT INTO guilty_in_undelivered_orders
				(id, undelivery_id, guilty_side, guilty_department_id, guilty_employee_id)
				VALUES(@id, @undelivery_id, @guilty_side, IFNULL(@guilty_department_id, 0), IFNULL(@guilty_employee_id, 0));"
			;
			return sql;
		}

		private string GetMaxSubdivisionIdSelectSql()
		{
			var sql = @$"select max(id) from subdivisions;";

			return sql;
		}

		private string GetSubdivisionsSelectSql()
		{
			var sql = @$"select * from subdivisions where id > @id;";

			return sql;
		}

		private string GetSubdivisionsInsertSql()
		{
			var sql = @"INSERT INTO subdivisions
				(id, name, short_name, chief_id, parent_subdivision_id, geo_group_id, default_wage_calculation_id, `type`, default_sales_plan_id, address, is_archive, pacs_time_management_enabled)
				VALUES(@id, @name, @short_name, @chief_id, @parent_subdivision_id, @geo_group_id, IFNULL(@default_wage_calculation_id, 0), @`type`, IFNULL(@default_sales_plan_id, 0), @address, @is_archive, @pacs_time_management_enabled);";

			return sql;
		}

		private string GetMaxCounterpartyIdSelectSql()
		{
			var sql = @$"select max(id) from counterparty;";

			return sql;
		}

		private string GetCounterpartysSelectSql()
		{
			var sql = @$"select * from counterparty where id > @id;";

			return sql;
		}

		private string GetCounterpartysInsertSql()
		{
			var sql = @"INSERT INTO counterparty
			(id, name, full_name, type_of_ownership, payment_method, person_type, accountant_id, sales_manager_id, bottles_manager_id,
			main_contact_id, financial_contact_id, maincounterparty_id, previous_counterparty_id, comment, max_credit, inn,
			kpp, ogrn, address, jur_address, default_account_id, code_1c, default_cash_expense_category_id, is_archive,
			default_document_type, signatory_FIO, signatory_post, signatory_base_of, phone_from_1c, need_new_bottles, vod_internal_id,
			ringup_phone, counterparty_camefrom_id, first_order_id, use_special_doc_fields, special_cargo_receiver, special_customer,
			special_contract_name, payer_special_kpp, special_gov_contract, special_delivery_address, use_address_from_order, need_cheque,
			is_deliveries_closed, close_delivery_date, close_delivery_comment, close_delivery_employee_id, ttn_count, torg2_count, OKPO, OKDP,
			counterparty_type, delay_days, cargo_receiver_source, is_chain_store, is_for_retail, upd_count, tax_type, create_date,
			always_send_receipts, special_expire_date_percent, special_expire_date_percent_check, delay_days_for_buyers, bitrix_id,
			delay_days_for_technical_processing, all_upd_count, torg12_count, shet_factura_count, car_proxy_count, no_phone_call,
			works_through_organization_id, always_print_invoice, roboats_exclude, is_for_sales_department, is_paperless_workflow,
			is_not_send_documents_by_edo, can_send_upd_in_advance, reason_for_leaving, registration_in_chestny_znak_status, order_status_for_sending_upd,
			consent_for_edo_status, personal_account_id_in_edo, edo_operator_id, special_contract_date, special_contract_number,
			do_not_mix_marked_and_unmarked_goods_in_order, surname, first_name, patronymic, logistics_requirements_id, need_send_bill_by_edo,
			default_financial_expense_category_id, counterparty_subtype, counterparty_subtype_id, is_liquidating, our_organization_account_for_bills,
			close_delivery_debt_type, referrer_id, exclude_from_auto_calls, hide_delivery_point_for_bill)
			VALUES(@id, @name, @full_name, @type_of_ownership, @payment_method, @person_type, @accountant_id, @sales_manager_id, @bottles_manager_id,
			main_contact_id, @financial_contact_id, @maincounterparty_id, @previous_counterparty_id, @comment, @max_credit, @inn,
			kpp, @ogrn, @address, @jur_address, @default_account_id, @code_1c, @default_cash_expense_category_id, @is_archive,
			default_document_type, @signatory_FIO, @signatory_post, @signatory_base_of, @phone_from_1c, @need_new_bottles, @vod_internal_id,
			ringup_phone, @counterparty_camefrom_id, @first_order_id, @use_special_doc_fields, @special_cargo_receiver, @special_customer,
			special_contract_name, @payer_special_kpp, @special_gov_contract, @special_delivery_address, @use_address_from_order, @need_cheque,
			is_deliveries_closed, @close_delivery_date, @close_delivery_comment, @close_delivery_employee_id, @ttn_count, @torg2_count, @OKPO, @OKDP,
			counterparty_type, @delay_days, @cargo_receiver_source, @is_chain_store, @is_for_retail, @upd_count, @tax_type, @create_date,
			always_send_receipts, @special_expire_date_percent, @special_expire_date_percent_check, @delay_days_for_buyers, @bitrix_id,
			delay_days_for_technical_processing, @all_upd_count, @torg12_count, @shet_factura_count, @car_proxy_count, @no_phone_call,
			works_through_organization_id, @always_print_invoice, @roboats_exclude, @is_for_sales_department, @is_paperless_workflow,
			is_not_send_documents_by_edo, @can_send_upd_in_advance, @reason_for_leaving, @registration_in_chestny_znak_status, @order_status_for_sending_upd,
			consent_for_edo_status, @personal_account_id_in_edo, @edo_operator_id, @special_contract_date, @special_contract_number,
			do_not_mix_marked_and_unmarked_goods_in_order, @surname, @first_name, @patronymic, @logistics_requirements_id, @need_send_bill_by_edo,
			default_financial_expense_category_id, @counterparty_subtype, @counterparty_subtype_id, @is_liquidating, @our_organization_account_for_bills,
			close_delivery_debt_type, @referrer_id, @exclude_from_auto_calls, @hide_delivery_point_for_bill);";

			return sql;
		}

		private string GetPlanPerDateSelectSql()
		{
			var sql = @$"select * from plan_per_day where date > @date;";

			return sql;
		}

		private string GetPlanPerDateInsertSql()
		{
			var sql = @"INSERT INTO power_bi.plan_per_day (`date`, `19l_water_count`, orders_count)
				VALUES(@date, @19l_water_count, @orders_count);";

			return sql;
		}

		private string GetCalendarInsertSql()
		{
			var ruCulture = CultureInfo.CreateSpecificCulture("ru-RU");
			var date = DateTime.Today.AddDays(-1);

			var montNum = date.Month;
			var monthName = ruCulture.TextInfo.ToTitleCase(date.ToString("MMMM", ruCulture));
			var monthShortName = ruCulture.TextInfo.ToTitleCase(date.ToString("MMM", ruCulture));

			var weekDayNum = (int)date.DayOfWeek;
			var weekDayName = ruCulture.TextInfo.ToTitleCase(date.ToString("dddd", ruCulture));
			var weekDayShortName = ruCulture.TextInfo.ToTitleCase(date.ToString("ddd", ruCulture));

			var dayNum = date.Day;
			var isWorkDay = weekDayNum < 6;

			var year = date.Year;

			var sql = $@"INSERT INTO calendar (`date`, month_num, month_name, month_shortName, weekday_num, weekday_name, weekday_shortName, day_num, workday, year)
						VALUES(""{date.ToString("yyyy-MM-dd")}"", {montNum}, ""{monthName}"", ""{monthShortName}"", {weekDayNum}, ""{weekDayName}"", ""{weekDayShortName}"", {dayNum}, {isWorkDay}, {year})";

			return sql;
		}

		#endregion tables
	}
}
