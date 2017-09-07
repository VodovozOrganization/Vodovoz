select route_list_id, count(id) as cnt, max(order_in_route) as max_id
from route_list_addresses
group by route_list_addresses.route_list_id
having max_id >= cnt