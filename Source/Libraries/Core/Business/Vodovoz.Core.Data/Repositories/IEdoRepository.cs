﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IEdoRepository
	{
		Task<IEnumerable<OrganizationEntity>> GetEdoOrganizationsAsync(CancellationToken cancellationToken);
		Task<IEnumerable<GtinEntity>> GetGtinsAsync(CancellationToken cancellationToken);
		Task<bool> HasReceiptOnSumToday(decimal sum, CancellationToken cancellationToken);
	}
}
