using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Cash;
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

		public virtual Result<List<Income>> CreateManualCashIncome(
			IUnitOfWork uow,
			RouteList routeList,
			decimal casheInput)
		{
			if(uow is null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(routeList is null)
			{
				throw new ArgumentNullException(nameof(routeList));
			}

			if(casheInput <= 0)
			{
				throw new ArgumentException("Сумма должна быть положительной", nameof(casheInput));
			}

			if(routeList.Cashier is null)
			{
				return Result.Failure<List<Income>>(RouteListErrors.CashierIsEmpty);
			}

			if(routeList.Cashier?.Subdivision == null)
			{
				return Result.Failure<List<Income>>(RouteListErrors.CashierSubdivisionIsEmpty);
			}

			var organizationDebts = GetCashDebtsByOrganizations(uow, routeList.Id);

			var cashIncomes = CreateAndDistributeCashIncomesByOrganizationsDebts(uow, routeList, organizationDebts, casheInput);

			return Result.Success(cashIncomes);
		}

		public virtual Result<(List<Income> Incomes, List<Expense> Expenses)> CreateAutomaticallyCashIncomesAndExpenses(
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
				return Result.Failure<(List<Income> Incomes, List<Expense> Expenses)> (RouteListErrors.CashierIsEmpty);
			}

			if(routeList.Cashier?.Subdivision == null)
			{
				return Result.Failure<(List<Income> Incomes, List<Expense> Expenses)>(RouteListErrors.CashierSubdivisionIsEmpty);
			}

			var organizationDebts = GetCashDebtsByOrganizations(uow, routeList.Id);

			var cashIncomes = CreateAndDistributeCashIncomesByOrganizationsDebts(uow, routeList, organizationDebts);
			var cashExpenses = CreateAndDistributeCashExpensesByOrganizationsOverpayments(uow, routeList, organizationDebts);

			return Result.Success((cashIncomes, cashExpenses));
		}

		private List<Income> CreateAndDistributeCashIncomesByOrganizationsDebts(
			IUnitOfWork uow,
			RouteList routeList,
			IList<RouteListDebtByOrganizationNode> organizationDebts,
			decimal? casheInput = null)
		{
			var incomes = new List<Income>();

			var detsByOrganizations = organizationDebts
				.Where(x => x.DebtSum > 0)
				.ToList();

			var organizations =
				GetOrganizationsByIds(
					uow,
					detsByOrganizations.Where(x => x.OrganizationId != null).Select(x => x.OrganizationId.Value));

			var remainder = casheInput is null ? detsByOrganizations.Sum(x => x.DebtSum) : casheInput.Value;

			foreach(var debtByOrganization in detsByOrganizations)
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
				int maxCashOrganizationId = GetMaxCashOrganizationId(organizationDebts);

				var organization = organizations.FirstOrDefault(x => x.Id == maxCashOrganizationId);

				if(organization is null)
				{
					organization = GetOrganizationById(uow, maxCashOrganizationId);
				}

				var cashIncome = CreateAndDistributeIncome(uow, routeList, organization, remainder);
				incomes.Add(cashIncome);
			}

			return incomes;
		}

		private List<Expense> CreateAndDistributeCashExpensesByOrganizationsOverpayments(
			IUnitOfWork uow,
			RouteList routeList,
			IList<RouteListDebtByOrganizationNode> organizationDebts,
			decimal? casheInput = null)
		{
			var expenses = new List<Expense>();

			var overpaymentsByOrganizations = organizationDebts
				.Where(x => x.DebtSum < 0)
				.ToList();

			var remainder = casheInput is null ? overpaymentsByOrganizations.Sum(x => Math.Abs(x.DebtSum)) : casheInput.Value;

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
				int maxCashOrganizationId = GetMaxCashOrganizationId(organizationDebts);

				var organization = organizations.FirstOrDefault(x => x.Id == maxCashOrganizationId);

				if(organization is null)
				{
					organization = GetOrganizationById(uow, maxCashOrganizationId);
				}

				var cashExpense = CreateExpense(routeList, organization, remainder);
				expenses.Add(cashExpense);
				_routeListCashOrganisationDistributor.DistributeExpenseCash(uow, routeList, cashExpense, cashExpense.Money);
			}

			return expenses;
		}

		private IList<RouteListDebtByOrganizationNode> GetCashDebtsByOrganizations(IUnitOfWork uow, int routeListId) =>
			_cashRepository.GetRouteListCashDebtByOrganizationNodes(uow, _organizationSettings, routeListId, _orderRepository.OrderHasSentReceipt);

		private static int GetMaxCashOrganizationId(IList<RouteListDebtByOrganizationNode> organizationDebts) =>
			organizationDebts
			.OrderByDescending(x => x.OrdersCashSum)
			.First()
			.OrganizationId.Value;

		private Organization GetOrganizationById(IUnitOfWork uow, int organizationId) =>
			_organizationRepository.GetOrganizationById(uow, organizationId);

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
	}
}
