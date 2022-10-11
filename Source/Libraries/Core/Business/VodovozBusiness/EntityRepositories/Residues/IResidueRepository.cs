using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Residues
{
	public interface IResidueRepository
	{
		IList<Residue> GetResidueByCounterpary(IUnitOfWork uow, Counterparty counterparty, int count = int.MaxValue);
		IList<Residue> GetResidueByDeliveryPoint(IUnitOfWork uow, DeliveryPoint deliveryPoint, int count = int.MaxValue);
	}
}