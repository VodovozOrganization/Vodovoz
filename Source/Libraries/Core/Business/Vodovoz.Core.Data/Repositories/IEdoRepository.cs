using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IEdoRepository
	{
		//bool HasCustomerEdoRequest(int orderId, out CustomerEdoRequest orderPermit);
		IEnumerable<OrganizationEntity> GetEdoOrganizations();
	}
}
