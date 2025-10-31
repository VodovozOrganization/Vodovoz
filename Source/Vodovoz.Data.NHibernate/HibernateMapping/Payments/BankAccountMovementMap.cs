using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Payments
{
	public class BankAccountMovementMap : ClassMap<BankAccountMovement>
	{
		public BankAccountMovementMap()
		{
			Table("bank_accounts_movements");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
		
			References(x => x.Account).Column("account_id");
			References(x => x.Bank).Column("bank_id");
			
			HasMany(x => x.BankAccountMovements)
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.KeyColumn("account_movement_id");
		}
	}
}
