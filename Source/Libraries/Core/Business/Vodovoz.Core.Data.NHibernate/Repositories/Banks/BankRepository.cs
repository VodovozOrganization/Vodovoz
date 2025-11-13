using System.Collections.Generic;
using System.Linq;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Banks
{
	public class BankRepository : IBankRepository
	{
		public IEnumerable<Bank> GetBanksFromPayments(IUnitOfWork uow)
		{
			var banks = from payment in uow.Session.Query<PaymentEntity>()
				join account in uow.Session.Query<Account>()
					on payment.OrganizationAccount.Id equals account.Id
				join bank in uow.Session.Query<Bank>()
					on account.InBank.Id equals bank.Id
				select bank;
			
			return banks;
		}
	}
}
