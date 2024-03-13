using NHibernate.Criterion;
using QS.Project.DB;
using System.ComponentModel.DataAnnotations;
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
		public static IProjection GetDriverFullNameProjection()
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

		[Display(Name = "ФИО оштрафованного лица")]
		public static IProjection FinedEmployeeFioProjection
		{
			get
			{
				Employee finedEmployeeAlias = null;

				return CustomProjections.Concat_WS(
					" ",
					() => finedEmployeeAlias.LastName,
					() => finedEmployeeAlias.Name,
					() => finedEmployeeAlias.Patronymic);
			}
		}
	}
}
