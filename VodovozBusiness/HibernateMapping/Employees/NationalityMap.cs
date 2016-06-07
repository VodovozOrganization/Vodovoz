using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.HMap
{
	public class NationalityMap : ClassMap<Nationality>
	{
		public NationalityMap ()
		{
			Table ("nationality");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
		}
	}
}