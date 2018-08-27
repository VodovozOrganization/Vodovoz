using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;


namespace Vodovoz.Repositories
{
	public static class FinesRepository
	{
		public static IList<FinesVMNodeForUndelivery> GetFinesForUndelivery(IUnitOfWork uow, UndeliveredOrder undelivery){
			FinesVMNodeForUndelivery resultAlias = null;
			Fine fineAlias = null;
			FineItem fineItemAlias = null;
			Employee employeeAlias = null;

			var query = uow.Session.QueryOver<Fine>(() => fineAlias)
						   .Where(f => f.UndeliveredOrder.Id == undelivery.Id)
						   .JoinAlias(f => f.Items, () => fineItemAlias)
						   .JoinAlias(() => fineItemAlias.Employee, () => employeeAlias)
						   .SelectList(list => list
									   .SelectGroup(() => fineAlias.Id).WithAlias(() => resultAlias.Id)
									   .Select(() => fineAlias.Date).WithAlias(() => resultAlias.Date)
									   .Select(
										   Projections.SqlFunction(
											   new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
											   NHibernateUtil.String,
											   Projections.SqlFunction(new StandardSQLFunction("CONCAT_WS"),
																	   NHibernateUtil.String,
																	   Projections.Constant(" "),
																	   Projections.Property(() => employeeAlias.Name),
																	   Projections.Property(() => employeeAlias.Patronymic),
																	   Projections.Property(() => employeeAlias.LastName)
																	  ),
											   Projections.Constant("\n")
											  )
										  ).WithAlias(() => resultAlias.EmployeesName)
									   .Select(() => fineAlias.TotalMoney).WithAlias(() => resultAlias.FineSumm)
									  )
						   .TransformUsing(Transformers.AliasToBean<FinesVMNodeForUndelivery>())
						   .List<FinesVMNodeForUndelivery>();
			
			return query;
		}
	}

	public class FinesVMNodeForUndelivery
	{
		public int Id { get; set; }
		public string EmployeesName { get; set; }
		public decimal FineSumm { get; set; }
		public DateTime Date { get; set; }
	}
}
