CREATE DEFINER = CURRENT_USER TRIGGER `Vodovoz_orders`.`orders_BEFORE_UPDATE` BEFORE UPDATE ON `orders` FOR EACH ROW
BEGIN
	SET @newNumber = 
				(SELECT 
					IFNULL(MAX(daily_number_1c), 0) + 1 as daily_number_1c 
				FROM
					orders
				WHERE orders.delivery_date = new.delivery_date);
                
	IF new.order_status in ('NewOrder', 'WaitForPayment') THEN
		SET new.daily_number_1c = null;
	ELSEIF new.daily_number_1c IS NULL 
		OR new.delivery_date != old.delivery_date THEN
		SET new.daily_number_1c = @newNumber;
    END IF;
END