using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class ProposedPlan
	{
		public static ProposedPlan BestFinishedPlan {get; set;}
		public static double BestFinishedCost { get; set; }

		public static ProposedPlan BestNotFinishedPlan { get; set; }
		public static int BestNotFinishedCount { get; set; }

		public List<ProposedRoute> Routes = new List<ProposedRoute>();
		public List<FreeOrders> RemainOrders;
		public List<AtWorkDriver> RemainDrivers;

		public double PlanCost;

		public List<int> DebugLevel = new List<int>();

		public ProposedRoute CurRoute { get; set; }
		//	get{
		//		return Routes.LastOrDefault();
		//	}
		//}

		public int FreeOrdersCount{
			get {
				return RemainOrders.Sum(x => x.Orders.Count);
			}
		}

		public ProposedPlan()
		{
		}

		public void OrderTaked(Order order)
		{
			RemainOrders.ForEach(x => x.Orders.Remove(order));
		}

		public ProposedPlan Clone()
		{
			var plan = new ProposedPlan();
			plan.Routes = Routes.Select(x => x.Clone()).ToList();
			plan.RemainOrders = RemainOrders.Select(x => x.Clone()).ToList();
			plan.RemainDrivers = RemainDrivers.ToList();
			plan.PlanCost = PlanCost;
			plan.CurRoute = CurRoute != null ? plan.Routes.Last() : null;
			plan.DebugLevel = DebugLevel.ToList();

			return plan;
		}
	}
}
