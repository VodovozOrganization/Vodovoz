using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Users.Settings;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class CashSubdivisionSortingSettingsMap : ClassMap<CashSubdivisionSortingSettings>
	{
		public CashSubdivisionSortingSettingsMap()
		{
			Table("cash_subdivision_sorting_settings");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.SortingIndex).Column("sorting_index");
			Map(x => x.UserSettingsId).Column("user_settings_id");
			Map(x => x.CashSubdivisionId).Column("cash_subdivision_id");
		}
	}
}
