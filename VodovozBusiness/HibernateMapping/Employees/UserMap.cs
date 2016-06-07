using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;

namespace Vodovoz.HMap
{
	public class UserMap : ClassMap<User>
	{
		public UserMap ()
		{
			Table ("users");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
			Map (x => x.Login).Column ("login");
		}
	}
}