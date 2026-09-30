using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoDeviationSourceMap : ClassMap<EdoDeviationSource>
	{
		public EdoDeviationSourceMap()
		{
			Table("edo_deviation_sources");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.DeviationType)
				.Column("deviation_type");

			Map(x => x.Description)
				.Column("description");

			Map(x => x.ErrorMessage)
				.Column("error_message");

			Map(x => x.Timeout)
				.Column("timeout")
				.CustomType<TimeAsTimeSpanType>();

			Map(x => x.IsActive)
				.Column("is_active");
		}
	}
}
