using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoProblemGtinItemMap : SubclassMap<EdoProblemGtinItem>
	{
		public EdoProblemGtinItemMap()
		{
			DiscriminatorValue(nameof(EdoProblemCustomItemType.Gtin));

			References(x => x.Gtin)
				.Column("gtin_id");
		}
	}
}
