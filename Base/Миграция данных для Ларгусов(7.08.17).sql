UPDATE `cars` SET cars.type_of_use = 'Truck'  WHERE `is_truck` = true;
UPDATE `cars` SET cars.type_of_use = 'Largus'  WHERE cars.is_company_havings AND NOT `is_truck`;
