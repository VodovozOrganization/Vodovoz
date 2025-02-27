using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoTaskValidatorEntityMap : ClassMap<EdoTaskValidatorEntity>
	{
		public EdoTaskValidatorEntityMap()
		{
			Table("edo_task_validators");

			Id(x => x.Name)
				.Column("name")
				.GeneratedBy.Assigned();

			Map(x => x.Importance)
				.Column("importance");

			Map(x => x.Message)
				.Column("message");

			Map(x => x.Description)
				.Column("description");

			Map(x => x.Recommendation)
				.Column("recommendation");
		}
	}
}
