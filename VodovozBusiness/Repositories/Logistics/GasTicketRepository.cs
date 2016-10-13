using System;
using System.Linq;
using NHibernate.Transform;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Order = Vodovoz.Domain.Orders.Order;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using System.Collections.Generic;

namespace Vodovoz.Repository.Logistics
{
	public static class GasTicketRepository
	{
		public static IList<GazTicket> GetGasTickets (IUnitOfWork uow, FuelType type)
		{
			DeliveryPoint point = null;
			return uow.Session.QueryOver<GazTicket> ()
				.Where (x => x.FuelType == type)
				.List<GazTicket> ();
		}
	}


}