using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HMap
{
	public class SignificanceMap : ClassMap<Significance>
	{
		public SignificanceMap ()
		{
			Table ("significance");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
		}
	}
}