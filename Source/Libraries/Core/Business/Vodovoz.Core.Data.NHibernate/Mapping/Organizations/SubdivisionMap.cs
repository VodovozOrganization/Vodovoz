using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Organizations
{
	public class SubdivisionMap : ClassMap<SubdivisionEntity>
	{
		public SubdivisionMap()
		{
			Table("subdivisions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.FinancialResponsibilityCenterId).Column("financial_responsibility_center_id");
			Map(x => x.ChiefId).Column("chief_id");
			Map(x => x.PacsTimeManagementEnabled).Column("pacs_time_management_enabled");
		}
	}
}
