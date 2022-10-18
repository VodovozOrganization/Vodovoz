using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz
{
	public class FineTemplateMap : ClassMap<FineTemplate>
	{
		public FineTemplateMap ()
		{
			Table ("fine_templates");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.Reason)		.Column ("reason");
			Map (x => x.FineMoney)	.Column ("fine_money");
		}
	}
}

