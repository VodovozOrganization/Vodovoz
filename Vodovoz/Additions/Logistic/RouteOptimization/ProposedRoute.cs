﻿using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class ProposedRoute
	{
		public List<Order> Orders = new List<Order>();
		public AtWorkDriver Driver;
		public Car Car;

		public int CurrentBottles{
			get{
				return Orders.SelectMany(x => x.OrderItems)
							 .Where(x => x.Nomenclature.Category == Domain.Goods.NomenclatureCategory.water)
							 .Sum(x => x.Count);
			}
		}

		public double CurrentWeight {
			get {
				return Orders.SelectMany(x => x.OrderItems)
					         .Sum(x => x.Nomenclature.Weight * x.Count);
			}
		}

		public double CurrentVolume {
			get {
				return Orders.SelectMany(x => x.OrderItems)
					         .Sum(x => x.Nomenclature.Volume * x.Count);
			}
		}

		public ProposedRoute(AtWorkDriver driver, Car car)
		{
			Driver = driver;
			Car = car;
		}

		public bool CanAdd(Order order)
		{
			if(Orders.Count >= Car.MaxRouteAddresses)
				return false;

			var bottles = CurrentBottles + order.OrderItems.Where(x => x.Nomenclature.Category == Domain.Goods.NomenclatureCategory.water)
							 .Sum(x => x.Count);
			if(bottles > Car.MaxBottles)
				return false;

			var weight = CurrentWeight + order.OrderItems.Sum(x => x.Nomenclature.Weight * x.Count);
			if(weight > Car.MaxWeight)
				return false;

			var volume = CurrentVolume + order.OrderItems.Sum(x => x.Nomenclature.Volume * x.Count);
			if(volume > Car.MaxVolume)
				return false;

			return true;
		}
	}
}
