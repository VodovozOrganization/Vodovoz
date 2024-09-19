using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class SaveCodesEdoTaskMap : SubclassMap<SaveCodesEdoTask>
	{
		public SaveCodesEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.SaveCode));

			Map(x => x.OrderId)
				.Column("order_id");
		}
	}
}
