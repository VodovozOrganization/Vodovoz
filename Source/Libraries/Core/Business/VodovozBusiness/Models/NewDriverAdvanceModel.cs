using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Errors.Cash;
using Vodovoz.Settings.Cash;
using Vodovoz.Settings.Logistics;

namespace Vodovoz.Models
{
	public class NewDriverAdvanceModel
	{
		private readonly INewDriverAdvanceSettings _newDriverAdvanceSettings;
		private readonly IRouteListRepository _routeListRepository;
		private readonly RouteList _routeList;
		private IList<NewDriverAdvanceRouteListNode> _unclosedRouteLists;

		public NewDriverAdvanceModel(INewDriverAdvanceSettings newDriverAdvanceSettings, IRouteListRepository routeListRepository, RouteList routeList)
		{
			_newDriverAdvanceSettings = newDriverAdvanceSettings ?? throw new ArgumentNullException(nameof(newDriverAdvanceSettings));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_routeList = routeList ?? throw new ArgumentNullException(nameof(routeList));
		}

		public bool NeedNewDriverAdvance(IUnitOfWork uow)
		{
			if(!_newDriverAdvanceSettings.IsNewDriverAdvanceEnabled
			   || _routeListRepository.HasEmployeeAdvance(uow, _routeList.Id, _routeList.Driver.Id))
			{
				return false;
			}

			DateTime? lastRouteListDate =
				_routeListRepository.GetLastRouteListDateByDriver(uow, _routeList.Driver.Id, null, CarOwnType.Driver);

			DateTime? firstAdvanceDate = _routeListRepository.GetDateByDriverWorkingDayNumber(uow, _routeList.Driver.Id,
				_newDriverAdvanceSettings.NewDriverAdvanceFirstDay, null, CarOwnType.Driver);

			DateTime? lastAdvanceDate = _routeListRepository.GetDateByDriverWorkingDayNumber(uow, _routeList.Driver.Id,
				_newDriverAdvanceSettings.NewDriverAdvanceLastDay, null, CarOwnType.Driver);

			bool needNewDriverAdvance = firstAdvanceDate <= _routeList.Date
				&& _routeList.Date <= (lastAdvanceDate ?? lastRouteListDate);

			return needNewDriverAdvance;
		}

		/// <summary>
		/// Создание расходного ордера на аванс новому водителю
		/// </summary>
		/// <param name="uow">UnitOfWork, в котором сохраняется ордер</param>
		/// <param name="financialCategoriesGroupsSettings">Настройки финансовых статей</param>
		/// <param name="cashInput">Сумма аванса</param>
		/// <returns>Созданный и сохранённый ордер или ошибка, по которой в создании ордера отказано</returns>
		public Result<Expense> CreateNewDriverAdvance(
			IUnitOfWork uow,
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			decimal cashInput)
		{
			Expense cashExpense = null;

			var message = _routeList.EmployeeAdvanceOperation(ref cashExpense, cashInput, financialCategoriesGroupsSettings);

			if(cashExpense == null)
			{
				return Result.Failure<Expense>(ExpenseErrors.CreateCreationFailed(message));
			}

			uow.Save(cashExpense);
			cashExpense.UpdateWagesOperations(uow);

			return cashExpense;
		}

		public IList<NewDriverAdvanceRouteListNode> UnclosedRouteLists(IUnitOfWork uow) =>
			_unclosedRouteLists ?? (_unclosedRouteLists = _routeListRepository.GetOldUnclosedRouteLists(uow, _routeList.Date, _routeList.Driver.Id));
			
		public string UnclosedRouteListStrings(IUnitOfWork uow) => string.Join("\n",
			(UnclosedRouteLists(uow).Select(x => $" - № {x.Id}  от {x.Date.ToShortDateString()}").ToArray()));
	}
}
