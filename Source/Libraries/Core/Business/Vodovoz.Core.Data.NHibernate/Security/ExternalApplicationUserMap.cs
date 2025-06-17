using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Schemas.Employees;

namespace Vodovoz.Core.Data.NHibernate.Mappings
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
			Map(x => x.EmployeeId).Column(ExternalApplicationUserSchema.EmployeeColumn);
		}
	}
}
