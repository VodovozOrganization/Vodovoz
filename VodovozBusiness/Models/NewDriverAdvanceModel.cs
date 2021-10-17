﻿using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Parameters;

namespace Vodovoz.Models
{
	public class NewDriverAdvanceModel
	{
		private readonly INewDriverAdvanceParametersProvider _newDriverAdvanceParametersProvider;
		private readonly IRouteListRepository _routeListRepository;
		private readonly RouteList _routeList;
		private IList<NewDriverAdvanceRouteListNode> _unclosedRouteLists;

		public NewDriverAdvanceModel(INewDriverAdvanceParametersProvider newDriverAdvanceParametersProvider, IRouteListRepository routeListRepository, RouteList routeList)
		{
			_newDriverAdvanceParametersProvider = newDriverAdvanceParametersProvider ?? throw new ArgumentNullException(nameof(newDriverAdvanceParametersProvider));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeList = routeList ?? throw new ArgumentNullException(nameof(routeList));
		}

		public bool NeedNewDriverAdvance(IUnitOfWork uow)
		{
			if(_routeListRepository.HasEmployeeAdvance(uow, _routeList.Id, _routeList.Driver.Id))
			{
				return false;
			}

			DateTime? lastRouteListDate = _routeListRepository.GetLastRouteListDateByDriver(uow, _routeList.Driver.Id, CarTypeOfUse.DriverCar);

			DateTime? firstAdvanceDate = _routeListRepository.GetDateByDriverWorkingDayNumber(uow, _routeList.Driver.Id,
				_newDriverAdvanceParametersProvider.NewDriverAdvanceFirstDay, CarTypeOfUse.DriverCar);

			DateTime? lastAdvanceDate = _routeListRepository.GetDateByDriverWorkingDayNumber(uow, _routeList.Driver.Id,
				_newDriverAdvanceParametersProvider.NewDriverAdvanceLastDay, CarTypeOfUse.DriverCar);

			bool needNewDriverAdvance = (firstAdvanceDate <= _routeList.Date)
										&& (_routeList.Date <= (lastAdvanceDate ?? lastRouteListDate));

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
		}

		public IList<NewDriverAdvanceRouteListNode> UnclosedRouteLists(IUnitOfWork uow) =>
			_unclosedRouteLists ?? (_unclosedRouteLists = _routeListRepository.GetOldUnclosedRouteLists(uow, _routeList.Date, _routeList.Driver.Id));
			
		public string UnclosedRouteListStrings(IUnitOfWork uow) => string.Join("\n",
			(UnclosedRouteLists(uow).Select(x => $" - № {x.Id}  от {x.Date.ToShortDateString()}").ToArray()));
	}
}
