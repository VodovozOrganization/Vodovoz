using FluentNHibernate.Mapping;
using Vodovoz.Domain.Accounting;

namespace Vodovoz.HMap
{
	public class AccountExpenseMap : ClassMap<AccountExpense>
	{
		public AccountExpenseMap ()
		{
			Table ("account_expense");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Number).Column ("number");
			Map (x => x.Date).Column ("date");
			Map (x => x.Total).Column ("total");
			Map (x => x.Description).Column ("description");
			References (x => x.Organization).Column ("organization_id");
			References (x => x.OrganizationAccount).Column ("organization_account_id");
			References (x => x.Counterparty).Column ("counterparty_id");
			References (x => x.CounterpartyAccount).Column ("counterparty_account_id");
			References (x => x.Employee).Column ("employee_id");
			//TODO
			//References (x => x.EmployeeAccount).Column ("employee_account_id");
		}
	}
}