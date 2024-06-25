using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Residues;

namespace Vodovoz.Infrastructure.Persistance.Residues
{
	internal sealed class ResidueRepository : IResidueRepository
	{
		public IList<Residue> GetResidueByCounterpary(IUnitOfWork uow, Counterparty counterparty, int count = int.MaxValue)
		{
			Residue residueAlias = null;

			var residueQuery = uow.Session.QueryOver(() => residueAlias)
				.Where(() => residueAlias.Customer.Id == counterparty.Id)
				.Take(count)
				.List();

			return residueQuery;
		}

		public IList<Residue> GetResidueByDeliveryPoint(IUnitOfWork uow, DeliveryPoint deliveryPoint, int count = int.MaxValue)
		{
			Residue residueAlias = null;

			var residueQuery = uow.Session.QueryOver(() => residueAlias)
				.Where(() => residueAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.Take(count)
				.List();

			return residueQuery;
		}
	}
}
