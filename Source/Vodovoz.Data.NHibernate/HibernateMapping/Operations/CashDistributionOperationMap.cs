using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
{
	public class OrganisationCashMovementOperationMap : ClassMap<OrganisationCashMovementOperation>
	{
		public OrganisationCashMovementOperationMap()
		{
			Table("organisation_cash_movement_operations");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.OperationTime).Column("operation_time");
			Map(x => x.Amount).Column("amount");

			References(x => x.Organisation).Column("organisation_id");
		}
	}
}