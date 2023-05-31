using NHibernate.Criterion;
using QS.Project.DB;
using Vodovoz.Domain.Employees;

namespace Vodovoz.NHibernateProjections.Employees
{
	public static class EmployeeProjections
	{
		/// <summary>
		/// Проекции работают через рефлексию (nameof())<br/>
		/// Необходимо использовать конвенцию наименования алиасов в запросе для которого применяется проекция:<br/>
		/// camelCase названия сущности + Alias<br/>
		/// Используется <see cref="Employee"/> driverAlias
		/// </summary>
		/// <returns></returns>
		public static IProjection GetDriverFullNamePojection()
		{
			Employee driverAlias = null;

			return CustomProjections.Concat_WS(" ",
				Projections.Property(() => driverAlias.LastName),
				Projections.Property(() => driverAlias.Name),
				Projections.Property(() => driverAlias.Patronymic));
		}

		public static IProjection GetEmployeeFullNameProjection()
		{
			return CustomProjections.Concat_WS(
				" ",
				Projections.Property<Employee>(x => x.LastName),
				Projections.Property<Employee>(x => x.Name),
				Projections.Property<Employee>(x => x.Patronymic)
			);
		}
	}
}
