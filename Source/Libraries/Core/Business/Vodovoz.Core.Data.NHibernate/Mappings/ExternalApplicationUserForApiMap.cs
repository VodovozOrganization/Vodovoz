using FluentNHibernate.Mapping;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Domain.Schemas.Employees;

namespace Vodovoz.Core.Data.NHibernate.Mappings
{
	/// <summary>
	/// После реорганизации проекта VodovozBussines, можно будет избавиться от дубликата класса
	/// </summary>
	public class ExternalApplicationUserForApiMap : ClassMap<ExternalApplicationUserForApi>
	{
		public ExternalApplicationUserForApiMap()
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
