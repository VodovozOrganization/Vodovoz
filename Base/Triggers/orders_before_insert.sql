CREATE DEFINER = CURRENT_USER TRIGGER `Vodovoz_orders`.`orders_BEFORE_INSERT` BEFORE INSERT ON `orders` FOR EACH ROW
BEGIN
	SET new.daily_number_1c = 
		IF(
			new.order_status in ('NewOrder', 'WaitForPayment'),
			null,
			(SELECT 
				IFNULL(MAX(daily_number_1c), 0) + 1 as daily_number_1c 
			FROM
				orders
			WHERE orders.delivery_date = new.delivery_date)
        );
END