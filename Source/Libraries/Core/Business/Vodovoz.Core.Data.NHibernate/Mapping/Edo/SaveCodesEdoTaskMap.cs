using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class SaveCodesEdoTaskMap : SubclassMap<SaveCodesEdoTask>
	{
		public SaveCodesEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.SaveCode));

			Extends(typeof(OrderEdoTask));
		}
	}
}
