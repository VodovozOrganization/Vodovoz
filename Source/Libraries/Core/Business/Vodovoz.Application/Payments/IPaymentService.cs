using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;
using Vodovoz.EntityRepositories.Payments;

namespace Vodovoz.Application.Payments
{
	public interface IPaymentService
	{
		Result DistributeByClientIdAndOrganizationId(IUnitOfWork unitOfWork, int counterpartyId, int organizationId, bool distributeCompletedPayments = false);
		Result<IEnumerable<UnallocatedBalancesJournalNode>> GetAllUnallocatedBalancesForAutomaticDistribution(IUnitOfWork unitOfWork);
	}
}