using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Payments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Payments
{
	public class BankAccountMovementDataMap : ClassMap<BankAccountMovementData>
	{
		public BankAccountMovementDataMap()
		{
			Table("bank_accounts_movements_data");
			
			Id(x => x.Id).GeneratedBy.Native();
		
			Map(x => x.Amount).Column("amount");
			Map(x => x.AccountMovementDataType).Column("type");
			
			References(x => x.AccountMovement).Column("account_movement_id");
		}
	}
}
