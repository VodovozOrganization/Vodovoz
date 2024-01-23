using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Schemas.Employees;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Employees
{
	public class ExternalApplicationUserMap : ClassMap<ExternalApplicationUser>
	{
		public ExternalApplicationUserMap()
		{
			Table(ExternalApplicationUserSchema.TableName);

			Id(x => x.Id).Column(ExternalApplicationUserSchema.IdColumn).GeneratedBy.Native();

			Map(x => x.Login).Column(ExternalApplicationUserSchema.LoginColumn);
			Map(x => x.Password).Column(ExternalApplicationUserSchema.PasswordColumn);
			Map(x => x.SessionKey).Column(ExternalApplicationUserSchema.SessionKeyColumn);
			Map(x => x.Token).Column(ExternalApplicationUserSchema.TokenColumn);
			Map(x => x.ExternalApplicationType)
				.Column(ExternalApplicationUserSchema.ExternalApplicationTypeColumn);

			References(x => x.Employee).Column(ExternalApplicationUserSchema.EmployeeColumn);
		}
	}
}
