using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Errors.Logistics;
using Vodovoz.Settings.Cash;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.EntityRepositories.Nodes;

namespace VodovozBusiness.Services.Cash
{
	public class RouteListCashProcessingService : IRouteListCashProcessingService
	{
		private readonly IFinancialCategoriesGroupsSettings _financialCategoriesGroupsSettings;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly ICashRepository _cashRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrganizationSettings _organizationSettings;
		private readonly IRouteListCashOrganisationDistributor _routeListCashOrganisationDistributor;

		public RouteListCashProcessingService(
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IOrganizationRepository organizationRepository,
			ICashRepository cashRepository,
			IOrderRepository orderRepository,
			IOrganizationSettings organizationSettings,
			IRouteListCashOrganisationDistributor routeListCashOrganisationDistributor)
		{
			_financialCategoriesGroupsSettings =
				financialCategoriesGroupsSettings ?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			_organizationRepository =
				organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_cashRepository =
				cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
			_orderRepository =
				orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_organizationSettings =
				organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
			_routeListCashOrganisationDistributor =
				routeListCashOrganisationDistributor ?? throw new ArgumentNullException(nameof(routeListCashOrganisationDistributor));
		}

		public Result<IEnumerable<Income>> CreateManualCashIncome(
			IUnitOfWork uow,
			RouteList routeList,
			decimal cashInput)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(routeList is null)
			{
				throw new ArgumentNullException(nameof(routeList));
			}

			if(cashInput <= 0)
			{
				throw new ArgumentException("Сумма должна быть положительной", nameof(cashInput));
			}

			if(routeList.Cashier is null)
			{
				return Result.Failure<IEnumerable<Income>>(RouteListErrors.CashierIsEmpty);
			}

			if(routeList.Cashier?.Subdivision == null)
			{
				return Result.Failure<IEnumerable<Income>>(RouteListErrors.CashierSubdivisionIsEmpty);
			}

			var organizationDebts = GetRouteListCashDebtsByOrganizations(uow, routeList);

			try
			{
				var cashIncomes = CreateAndDistributeCashIncomesByOrganizationsDebts(uow, routeList, organizationDebts, cashInput);
				return Result.Success(cashIncomes.AsEnumerable());
			}
			catch(MissingOrdersWithCashPaymentTypeException ex)
			{
				return Result.Failure<IEnumerable<Income>>(RouteListErrors.MissingCashPaymentTypeOrders);
			}
		}

		public Result<IEnumerable<string>> RecalculateRouteListCashBalance(
			IUnitOfWork uow,
			RouteList routeList)
		{
			var createdOperationsResult = CreateAutomaticallyCashIncomesAndExpenses(uow, routeList);

			if(createdOperationsResult.IsFailure)
			{
				return Result.Failure<IEnumerable<string>>(createdOperationsResult.Errors);
			}

			try
			{
				var incomes = createdOperationsResult.Value.Incomes;
				var expenses = createdOperationsResult.Value.Expenses;

				if(!incomes.Any() && !expenses.Any())
				{
					return Result.Success(Enumerable.Empty<string>());
				}

				var messages = new List<string>();

				messages.AddRange(incomes.Select(income =>
					$"Создан приходный ордер на сумму {income.Money:C0} по организации \"{income.Organisation?.Name}\""));

				messages.AddRange(expenses.Select(expense =>
					$"Создан расходный ордер на сумму {expense.Money:C0} по организации \"{expense.Organisation?.Name}\""));

				return Result.Success(messages.AsEnumerable());
			}
			catch(MissingOrdersWithCashPaymentTypeException ex)
			{
				return Result.Failure<IEnumerable<string>>(RouteListErrors.MissingCashPaymentTypeOrders);
			}
		}

		private Result<(List<Income> Incomes, List<Expense> Expenses)> CreateAutomaticallyCashIncomesAndExpenses(
			IUnitOfWork uow,
			RouteList routeList)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(routeList is null)
			{
				throw new ArgumentNullException(nameof(routeList));
			}

			if(routeList.Cashier is null)
			{
				return Result.Failure<(List<Income> Incomes, List<Expense> Expenses)>(RouteListErrors.CashierIsEmpty);
			}

			if(routeList.Cashier?.Subdivision == null)
			{
				return Result.Failure<(List<Income> Incomes, List<Expense> Expenses)>(RouteListErrors.CashierSubdivisionIsEmpty);
			}

			var organizationDebts = GetRouteListCashDebtsByOrganizations(uow, routeList);

			var cashIncomes = CreateAndDistributeCashIncomesByOrganizationsDebts(uow, routeList, organizationDebts);
			var cashExpenses = CreateAndDistributeCashExpensesByOrganizationsOverpayments(uow, routeList, organizationDebts);

			return Result.Success((cashIncomes, cashExpenses));
		}

		private List<Income> CreateAndDistributeCashIncomesByOrganizationsDebts(
			IUnitOfWork uow,
			RouteList routeList,
			IList<RouteListDebtByOrganizationNode> organizationDebts,
			decimal? cashInput = null)
		{
			var incomes = new List<Income>();

			var debtsByOrganizations = organizationDebts
				.Where(x => x.DebtSum > 0)
				.ToList();

			var organizations =
				GetOrganizationsByIds(
					uow,
					debtsByOrganizations.Where(x => x.OrganizationId != null).Select(x => x.OrganizationId.Value));

			var remainder =
				cashInput is null
				? debtsByOrganizations.Sum(x => x.DebtSum)
				: cashInput.Value;

			foreach(var debtByOrganization in debtsByOrganizations)
			{
				if(remainder <= 0)
				{
					break;
				}

				var organization = organizations.FirstOrDefault(o => o.Id == debtByOrganization.OrganizationId);

				var amount = Math.Min(remainder, debtByOrganization.DebtSum);

				var cashIncome = CreateAndDistributeIncome(uow, routeList, organization, amount);
				incomes.Add(cashIncome);

				remainder -= amount;
			}

			if(remainder > 0)
			{
				var organization =
					organizations.FirstOrDefault(x => x.Id == CommonCashOrganizationId)
					?? GetCommonCashDistributionOrganisation(uow);

				var cashIncome = CreateAndDistributeIncome(uow, routeList, organization, remainder);
				incomes.Add(cashIncome);
			}

			return incomes;
		}

		private List<Expense> CreateAndDistributeCashExpensesByOrganizationsOverpayments(
			IUnitOfWork uow,
			RouteList routeList,
			IList<RouteListDebtByOrganizationNode> organizationDebts,
			decimal? cashInput = null)
		{
			var expenses = new List<Expense>();

			var overpaymentsByOrganizations = organizationDebts
				.Where(x => x.DebtSum < 0)
				.ToList();

			var remainder =
				cashInput is null ?
				overpaymentsByOrganizations.Sum(x => Math.Abs(x.DebtSum))
				: cashInput.Value;

			var organizations =
				GetOrganizationsByIds(
					uow,
					overpaymentsByOrganizations.Where(x => x.OrganizationId != null).Select(x => x.OrganizationId.Value));

			foreach(var overpaymentByOrganization in overpaymentsByOrganizations)
			{
				if(remainder <= 0)
				{
					break;
				}

				var organization = organizations.FirstOrDefault(o => o.Id == overpaymentByOrganization.OrganizationId);

				var overpaymentSum = Math.Abs(overpaymentByOrganization.DebtSum);
				var amount = Math.Min(remainder, overpaymentSum);
				var cashExpense = CreateAndDistributeExpense(uow, routeList, organization, amount);
				expenses.Add(cashExpense);
				remainder -= amount;
			}

			if(remainder > 0)
			{
				var organization =
					organizations.FirstOrDefault(x => x.Id == CommonCashOrganizationId)
					?? GetCommonCashDistributionOrganisation(uow);

				var cashExpense = CreateAndDistributeExpense(uow, routeList, organization, remainder);
				expenses.Add(cashExpense);
			}

			return expenses;
		}

		private int CommonCashOrganizationId => _organizationSettings.CommonCashDistributionOrganisationId;

		private Organization GetCommonCashDistributionOrganisation(IUnitOfWork uow) =>
			_organizationRepository.GetCommonOrganisation(uow);

		private IList<Organization> GetOrganizationsByIds(IUnitOfWork uow, IEnumerable<int> organizationIds) =>
			_organizationRepository.GetOrganizationsByIds(uow, organizationIds);

		private Income CreateAndDistributeIncome(IUnitOfWork uow, RouteList routeList, Organization organization, decimal amount)
		{
			var cashIncome = CreateIncome(routeList, organization, amount);
			uow.Save(cashIncome);
			_routeListCashOrganisationDistributor.DistributeIncomeCash(uow, routeList, cashIncome, cashIncome.Money);

			return cashIncome;
		}

		private Expense CreateAndDistributeExpense(IUnitOfWork uow, RouteList routeList, Organization organization, decimal amount)
		{
			var cashExpense = CreateExpense(routeList, organization, amount);
			uow.Save(cashExpense);
			_routeListCashOrganisationDistributor.DistributeExpenseCash(uow, routeList, cashExpense, cashExpense.Money);

			return cashExpense;
		}

		private Income CreateIncome(RouteList routeList, Organization organization, decimal amount) =>
			new Income
			{
				IncomeCategoryId = organization?.DefaultCashIncomeCategory?.Id ?? _financialCategoriesGroupsSettings.RouteListClosingFinancialIncomeCategoryId,
				TypeOperation = IncomeType.DriverReport,
				Date = DateTime.Now,
				Casher = routeList.Cashier,
				Employee = routeList.Driver,
				Organisation = organization,
				Description = $"Дополнение к МЛ №{routeList.Id} от {routeList.Date:d}",
				Money = Math.Round(amount, 0, MidpointRounding.AwayFromZero),
				RouteListClosing = routeList,
				RelatedToSubdivision = routeList.Cashier.Subdivision
			};

		private Expense CreateExpense(RouteList routeList, Organization organization, decimal amount) =>
			new Expense
			{
				ExpenseCategoryId = _financialCategoriesGroupsSettings.RouteListClosingFinancialExpenseCategoryId,
				TypeOperation = ExpenseType.Expense,
				Date = DateTime.Now,
				Casher = routeList.Cashier,
				Employee = routeList.Driver,
				Organisation = organization,
				Description = $"Дополнение к МЛ №{routeList.Id} от {routeList.Date:d}",
				Money = Math.Round(amount, 0, MidpointRounding.AwayFromZero),
				RouteListClosing = routeList,
				RelatedToSubdivision = routeList.Cashier.Subdivision
			};

		public IList<RouteListDebtByOrganizationNode> GetRouteListCashDebtsByOrganizations(
			IUnitOfWork uow,
			RouteList routeList)
		{
			var ordersCash = GetRouteListOrdersCashByOrganizations(uow, routeList);

			var cashIncomesExpensesByOrganization =
				_cashRepository.GetRouteListDriversCashIncomesExpensesByOrganizationNodes(uow, routeList.Id);

			var debtsByOrganizations = ordersCash
				.Concat(cashIncomesExpensesByOrganization)
				.GroupBy(x => x.OrganizationId)
				.Select(g => new RouteListDebtByOrganizationNode
				{
					OrganizationId = g.Key,
					OrganizationName = g.Key == null ? "Без организации" : g.Select(x => x.OrganizationName).First(name => name != null),
					OrdersCashSum = g.Sum(x => x.OrdersCashSum),
					IncomeSum = g.Sum(x => x.IncomeSum),
					ExpenseSum = g.Sum(x => x.ExpenseSum)
				})
				.ToList();

			return debtsByOrganizations;
		}

		private List<RouteListDebtByOrganizationNode> GetRouteListOrdersCashByOrganizations(IUnitOfWork uow, RouteList routeList)
		{
			var commonCashDistributionOrganisation = GetCommonCashDistributionOrganisation(uow);

			var ordersCashRaw =
				routeList.Addresses
				.Where(item =>
					item.Status == RouteListItemStatus.Completed
					|| (item.Status == RouteListItemStatus.EnRoute
						&& (routeList.Status == RouteListStatus.EnRoute
							|| routeList.Status == RouteListStatus.OnClosing
							|| routeList.Status == RouteListStatus.Closed)))
				.Where(item => item.Order.PaymentType == PaymentType.Cash)
				.Where(item => item.Order.Contract != null && item.Order.Contract.Organization != null)
				.Select(item => new
				{
					OrderId = item.Order.Id,
					ContractOrganizationId = item.Order.Contract.Organization.Id,
					ContractOrganizationName = item.Order.Contract.Organization.Name,
					OrdersCashSum = item.TotalCash
				});

			// Если по заказу нет чека,то считаем, что данный заказ на дефолтную организацию, независимо от организации в договоре заказа
			var ordersCash = ordersCashRaw
				.Select(x =>
				{
					var hasSentReceipt = _orderRepository.OrderHasSentReceipt(uow, x.OrderId);

					return new RouteListDebtByOrganizationNode
					{
						OrganizationId = hasSentReceipt ? x.ContractOrganizationId : commonCashDistributionOrganisation.Id,
						OrganizationName = hasSentReceipt ? x.ContractOrganizationName : commonCashDistributionOrganisation.Name,
						OrdersCashSum = x.OrdersCashSum
					};
				})
				.ToList();
			return ordersCash;
		}
	}
}
