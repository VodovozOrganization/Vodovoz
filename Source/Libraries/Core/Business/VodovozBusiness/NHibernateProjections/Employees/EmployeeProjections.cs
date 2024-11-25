using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
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

		/// <summary>
		/// Фамилия и инициалы сотрудника<br />
		/// Наименование алиаса в запросе для которого применяется проекция должно быть "employeeAlias"
		/// </summary>
		public static IProjection EmployeeLastNameWithInitials
		{
			get
			{
				Employee employeeAlias = null;

				return Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT( ?1, ' ', SUBSTRING(?2, 1, 1), '. ', SUBSTRING(?3, 1, 1), '.')"),
					NHibernateUtil.String,
					Projections.Property(() => employeeAlias.LastName),
					Projections.Property(() => employeeAlias.Name),
					Projections.Property(() => employeeAlias.Patronymic));
			}
		}

		/// <summary>
		/// Фамилия и инициалы водителя<br />
		/// Наименование алиаса в запросе для которого применяется проекция должно быть "driverAlias"
		/// </summary>
		public static IProjection DriverLastNameWithInitials
		{
			get
			{
				Employee driverAlias = null;

				return Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT( ?1, ' ', SUBSTRING(?2, 1, 1), '. ', SUBSTRING(?3, 1, 1), '.')"),
					NHibernateUtil.String,
					Projections.Property(() => driverAlias.LastName),
					Projections.Property(() => driverAlias.Name),
					Projections.Property(() => driverAlias.Patronymic));
			}
		}
	}
}
