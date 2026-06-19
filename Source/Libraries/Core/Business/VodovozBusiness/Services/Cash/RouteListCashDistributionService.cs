using FluentNHibernate.Data;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Errors.Logistics;
using Vodovoz.Settings.Cash;
using VodovozBusiness.EntityRepositories.Nodes;

namespace VodovozBusiness.Services.Cash
{
	public class RouteListCashDistributionService
	{
		private readonly IFinancialCategoriesGroupsSettings _financialCategoriesGroupsSettings;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly ICashRepository _cashRepository;

		public RouteListCashDistributionService(
			IFinancialCategoriesGroupsSettings financialCategoriesGroupsSettings,
			IOrganizationRepository organizationRepository,
			ICashRepository cashRepository)
		{
			_financialCategoriesGroupsSettings =
				financialCategoriesGroupsSettings ?? throw new ArgumentNullException(nameof(financialCategoriesGroupsSettings));
			_organizationRepository =
				organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_cashRepository =
				cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
		}

		public virtual Result<List<Income>> ManualCashIncomeDistribution(
			IUnitOfWork uow,
			RouteList routeList,
			decimal casheInput)
		{
			if(routeList is null)
			{
				throw new ArgumentNullException(nameof(routeList));
			}

			if(casheInput < 0)
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

			var cashIncomes = DistributeCashIncomeByOrganizationDebts(uow, routeList, organizationDebts, casheInput);

			return Result.Success(cashIncomes);
		}

		private List<Income> DistributeCashIncomeByOrganizationDebts(
			IUnitOfWork uow,
			RouteList routeList,
			IList<RouteListDebtByOrganizationNode> organizationDebts,
			decimal casheInput)
		{
			var incomes = new List<Income>();

			var detsByOrganizations = organizationDebts
				.Where(x => x.DebtSum > 0)
				.ToList();

			if(!detsByOrganizations.Any())
			{
				return incomes;
			}

			var organizations = GetOrganizationsByIds(uow, detsByOrganizations.Select(x => x.OrganizationId));

			var remainder = casheInput;

			foreach(var debtByOrganization in detsByOrganizations)
			{
				if(remainder <= 0)
				{
					break;
				}

				var organization = organizations.First(o => o.Id == debtByOrganization.OrganizationId);

				var amount = Math.Min(remainder, debtByOrganization.DebtSum);
				incomes.Add(CreateIncome(routeList, organization, amount));
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

				incomes.Add(CreateIncome(routeList, organization, remainder));
			}

			return incomes;
		}

		private List<Expense> DistributeManualExpenseByOrganizationOverpayments(
			IUnitOfWork uow,
			RouteList routeList,
			IList<RouteListDebtByOrganizationNode> organizationDebts,
			decimal casheInput)
		{
			var expenses = new List<Expense>();

			var overpaymentsByOrganizations = organizationDebts
				.Where(x => x.DebtSum < 0)
				.ToList();

			if(!overpaymentsByOrganizations.Any())
			{
				return expenses;
			}

			var remainder = casheInput;

			var organizations = GetOrganizationsByIds(uow, overpaymentsByOrganizations.Select(x => x.OrganizationId));

			foreach(var overpaymentByOrganization in overpaymentsByOrganizations)
			{
				if(remainder <= 0)
				{
					break;
				}

				var organization = organizations.First(o => o.Id == overpaymentByOrganization.OrganizationId);

				var overpaymentSum = -overpaymentByOrganization.DebtSum;
				var amount = Math.Min(remainder, overpaymentSum);
				expenses.Add(CreateExpense(routeList, organization, amount));
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

				expenses.Add(CreateExpense(routeList, organization, remainder));
			}

			return expenses;
		}

		private IList<RouteListDebtByOrganizationNode> GetCashDebtsByOrganizations(IUnitOfWork uow, int routeListId) =>
			_cashRepository.GetRouteListCashDebtByOrganizationNodes(uow, routeListId);

		private static int GetMaxCashOrganizationId(IList<RouteListDebtByOrganizationNode> organizationDebts) =>
			organizationDebts
			.OrderByDescending(x => x.OrdersCashSum)
			.First()
			.OrganizationId;

		private Organization GetOrganizationById(IUnitOfWork uow, int organizationId) =>
			_organizationRepository.GetOrganizationById(uow, organizationId);

		private IList<Organization> GetOrganizationsByIds(IUnitOfWork uow, IEnumerable<int> organizationIds) =>
			_organizationRepository.GetOrganizationsByIds(uow, organizationIds);

		private Income CreateIncome(RouteList routeList, Organization organization, decimal amount) =>
			new Income
			{
				IncomeCategoryId = organization.DefaultCashIncomeCategory.Id,
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
