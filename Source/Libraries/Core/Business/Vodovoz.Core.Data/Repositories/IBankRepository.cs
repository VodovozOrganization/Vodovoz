using System.Collections.Generic;
using QS.Banks.Domain;
using QS.DomainModel.UoW;

namespace Vodovoz.Core.Data.Repositories
{
	public interface IBankRepository
	{
		IEnumerable<Bank> GetBanksFromPayments(IUnitOfWork uow);
	}
}
