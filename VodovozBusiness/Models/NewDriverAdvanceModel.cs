using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Conventions;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Parameters;

namespace Vodovoz.Models
{
	public class NewDriverAdvanceModel
	{
		private readonly INewDriverAdvanceParametersProvider _newDriverAdvanceParametersProvider;
		private readonly RouteList _routeList;

		public NewDriverAdvanceModel(INewDriverAdvanceParametersProvider newDriverAdvanceParametersProvider, RouteList routeList)
		{
			_newDriverAdvanceParametersProvider = newDriverAdvanceParametersProvider ?? throw new ArgumentNullException(nameof(newDriverAdvanceParametersProvider));
			_routeList = routeList ?? throw new ArgumentNullException(nameof(routeList));
		}

		public IList<NewDriverAdvanceRouteListNode> UnclosedRouteLists(IUnitOfWork uow)
		{
			NewDriverAdvanceRouteListNode resultAlias = null;

			var unclosedRouteLists = uow.Session.QueryOver<RouteList>()
				.Where(x => x.Date < _routeList.Date)
				.And(x => x.Status != RouteListStatus.Closed)
				.And(x => x.Driver.Id == _routeList.Driver.Id)
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Date).WithAlias(() => resultAlias.Date)
				)
				.TransformUsing(Transformers.AliasToBean<NewDriverAdvanceRouteListNode>())
				.List<NewDriverAdvanceRouteListNode>();

			return unclosedRouteLists;
		}

		public bool NeedNewDriverAdvance(IUnitOfWork uow)
		{
			var hasAdvance = uow.Session.QueryOver<Expense>()
				.Where(x => x.RouteListClosing.Id == _routeList.Id)
				.And(x => x.Employee.Id == _routeList.Driver.Id)
				.And(x=> x.TypeOperation == ExpenseType.EmployeeAdvance)
				.RowCount() > 0;

			if(hasAdvance)
			{
				return false;
			}

			Employee employeeAlias = null;

			var firstAdvanceDate = uow.Session.QueryOver<RouteList>()
				.JoinAlias(x => x.Driver, () => employeeAlias)
				.Where(x => x.Driver.Id == _routeList.Driver.Id)
				.And(() => employeeAlias.DriverOf == CarTypeOfUse.DriverCar)
				.SelectList(list => list
					.SelectGroup(x => x.Date))
				.OrderBy(x => x.Date).Asc
				.Skip(_newDriverAdvanceParametersProvider.NewDriverAdvanceFirstDay - 1)
				.Take(1)
				.FutureValue<DateTime>();

			var lastAdvanceDate = uow.Session.QueryOver<RouteList>()
				.JoinAlias(x => x.Driver, () => employeeAlias)
				.Where(x => x.Driver.Id == _routeList.Driver.Id)
				.And(() => employeeAlias.DriverOf == CarTypeOfUse.DriverCar)
				.SelectList(list => list
					.SelectGroup(x => x.Date))
				.OrderBy(x => x.Date).Asc
				.Skip(_newDriverAdvanceParametersProvider.NewDriverAdvanceLastDay - 1)
				.Take(1)
				.FutureValue<DateTime>();

			var needNewDriverAdvance = (firstAdvanceDate.Value <= _routeList.Date)
									&& (_routeList.Date <= lastAdvanceDate.Value);

			return needNewDriverAdvance;
		}

		public void CreateNewDriverAdvance(IUnitOfWork uow, ICategoryRepository categoryRepository, decimal cashInput)
		{
			Expense cashExpense = null;

			_routeList.EmployeeAdvanceOperation(ref cashExpense, cashInput, categoryRepository);

			if(cashExpense != null)
			{
				uow.Save(cashExpense);
			}

			cashExpense?.UpdateWagesOperations(uow);

			uow.Save();
		}

		public string UnclosedRouteListStrings(IUnitOfWork uow) => string.Join("\n",
			(UnclosedRouteLists(uow).Select(x => $" - № {x.Id}  от {x.Date.ToShortDateString()}").ToArray()));

		public class NewDriverAdvanceRouteListNode
		{
			public int Id { get; set; }
			public DateTime Date { get; set; }
		}
	}
}
