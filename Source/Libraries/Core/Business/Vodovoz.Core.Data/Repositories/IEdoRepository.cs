using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IEdoRepository
	{
		Task<IEnumerable<OrganizationEntity>> GetEdoOrganizationsAsync(CancellationToken cancellationToken);
	}
}
