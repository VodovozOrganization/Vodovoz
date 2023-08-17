using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class CashSubdivisionSortingSettingsMap : ClassMap<CashSubdivisionSortingSettings>
	{
		public CashSubdivisionSortingSettingsMap()
		{
			Table("cash_subdivision_sorting_settings");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.SortingIndex).Column("sorting_index");
			References(x => x.UserSettings).Column("user_settings_id");
			References(x => x.CashSubdivision).Column("cash_subdivision_id");
		}
	}
}
