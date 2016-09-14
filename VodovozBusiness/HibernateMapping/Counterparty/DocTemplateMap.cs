using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;

namespace Vodovoz.HMap
{
	public class DocTemplateMap : ClassMap<DocTemplate>
	{
		public DocTemplateMap ()
		{
			Table ("doc_templates");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
			Map (x => x.TempalteFile).Column ("file").LazyLoad();
			Map (x => x.TemplateType).Column ("type").CustomType<TemplateTypeStringType> ();
			References(x => x.Organization).Column("organization_id");
		}
	}
}

