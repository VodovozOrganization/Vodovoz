using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;

namespace Vodovoz.Repositories
{
	static public class ResidueRepository
	{
		public static IList<Residue> GetResidueByCounterpary(IUnitOfWork UoW, Counterparty counterparty, int count = int.MaxValue)
		{
			Residue residueAlias = null;
			var residueQuery = UoW.Session.QueryOver<Residue>(() => residueAlias)
				.Where(() => residueAlias.Customer.Id == counterparty.Id)
				.Take(count)
				.List();
			return residueQuery;
		}

		public static IList<Residue> GetResidueByDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint, int count = int.MaxValue)
		{
			Residue residueAlias = null;
			var residueQuery = UoW.Session.QueryOver<Residue>(() => residueAlias)
				.Where(() => residueAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.Take(count)
				.List();
			return residueQuery;
		}
	}
}
