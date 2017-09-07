SELECT route_list_id, order_in_route, COUNT(id) as cnt 
FROM route_list_addresses 
GROUP BY route_list_addresses.route_list_id, route_list_addresses.order_in_route 
HAVING cnt > 1