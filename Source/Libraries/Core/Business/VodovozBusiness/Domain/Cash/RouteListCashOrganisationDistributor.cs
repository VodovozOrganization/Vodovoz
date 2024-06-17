using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Services;

namespace Vodovoz.Domain.Cash
{
	public class RouteListCashOrganisationDistributor : IRouteListCashOrganisationDistributor
	{
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IRouteListItemCashDistributionDocumentRepository _routeListItemCashDistributionDocumentRepository;
		private readonly IOrderRepository _orderRepository;

		public RouteListCashOrganisationDistributor(
			IOrganizationRepository organizationRepository,
			IRouteListItemCashDistributionDocumentRepository routeListItemCashDistributionDocumentRepository,
			IOrderRepository orderRepository)
		{
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_routeListItemCashDistributionDocumentRepository =
				routeListItemCashDistributionDocumentRepository ?? throw new ArgumentNullException(nameof(routeListItemCashDistributionDocumentRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
		}

		public void DistributeIncomeCash(IUnitOfWork uow, RouteList routeList, Income income, decimal amount)
		{
			if(amount == 0)
			{
				return;
			}

			var cashAddresses = routeList.Addresses.Where(x => x.TotalCash > 0);

			if(routeList.Total >
				_routeListItemCashDistributionDocumentRepository.GetDistributedAmountOnRouteList(uow, routeList.Id))
			{
				foreach(var address in cashAddresses)
				{
					var addressDistributedSum =
						_routeListItemCashDistributionDocumentRepository.GetDistributedAmountOnRouteListItem(uow,
							address.Id);

					if(addressDistributedSum == address.TotalCash)
					{
						continue;
					}

					if(addressDistributedSum < address.TotalCash)
					{
						var oldSum = addressDistributedSum;
						var sum = (addressDistributedSum + amount) >= address.TotalCash
							? address.TotalCash - addressDistributedSum
							: amount;

						var newOperation = CreateOrganisationCashMovementOperation(uow, address);
						newOperation.Amount = sum;
						var doc =
							CreateRouteListItemCashDistributionDocument(newOperation, address, income);
						Save(uow, newOperation, doc);

						amount -= address.TotalCash - oldSum;

						if(amount <= 0)
						{
							break;
						}
					}
				}

				if(amount > 0)
				{
					DistributeIncomeCashRemainingAmount(uow, routeList, income, amount);
				}
			}
			else
			{
				DistributeIncomeCashRemainingAmount(uow, routeList, income, amount);
			}
		}

		private void DistributeIncomeCashRemainingAmount(IUnitOfWork uow, RouteList routeList, Income income, decimal amount)
		{
			var operation = new OrganisationCashMovementOperation
			{
				OperationTime = DateTime.Now,
				Organisation = _organizationRepository.GetCommonOrganisation(uow),
				Amount = amount
			};

			var address = routeList.Addresses.FirstOrDefault(x => x.TotalCash > 0) ?? throw new MissingOrdersWithCashlessPaymentTypeException(routeList);
			var document = CreateRouteListItemCashDistributionDocument(operation, address, income);

			Save(uow, operation, document);
		}

		public void UpdateIncomeCash(IUnitOfWork uow, RouteList routeList, Income income, decimal amount)
		{
			var distributedIncomeAmount = _routeListItemCashDistributionDocumentRepository
				.GetDistributedIncomeAmount(uow, income.Id);

			if(distributedIncomeAmount > amount)
			{
				var docs =
					_routeListItemCashDistributionDocumentRepository.GetRouteListItemCashDistributionDocuments(uow, income.Id);

				foreach(var doc in docs)
				{
					DeleteOperation(uow, doc);
					DeleteDocument(uow, doc);
					uow.Commit();
				}

				DistributeIncomeCash(uow, routeList, income, amount);
			}

			if(distributedIncomeAmount < amount)
			{
				DistributeIncomeCash(uow, routeList, income, amount - distributedIncomeAmount);
			}
		}

		private void DeleteOperation(IUnitOfWork uow, RouteListItemCashDistributionDocument doc)
		{
			uow.Delete(doc.OrganisationCashMovementOperation);
			doc.OrganisationCashMovementOperation = null;
		}

		private void DeleteDocument(IUnitOfWork uow, RouteListItemCashDistributionDocument doc) => uow.Delete(doc);

		public void DistributeExpenseCash(IUnitOfWork uow, RouteList routeList, Expense expense, decimal amount)
		{
			if(amount == 0 || routeList is null)
			{
				return;
			}

			var cashAddresses = routeList.Addresses.Where(x => x.TotalCash > 0);

			if(routeList.Total <= _routeListItemCashDistributionDocumentRepository.GetDistributedAmountOnRouteList(uow, routeList.Id))
			{
				foreach(var address in cashAddresses)
				{
					var addressDistributedSum =
						_routeListItemCashDistributionDocumentRepository.GetDistributedAmountOnRouteListItem(uow,
							address.Id);

					var sum = (addressDistributedSum - amount) >= 0
						? -amount
						: -addressDistributedSum;

					var newOperation = CreateOrganisationCashMovementOperation(uow, address);
					newOperation.Amount = sum;
					var routeListItemCashdistributionDoc =
						CreateRouteListItemCashDistributionDocument(newOperation, address, expense);
					Save(uow, newOperation, routeListItemCashdistributionDoc);

					amount -= addressDistributedSum;

					if(amount <= 0)
					{
						break;
					}
				}
			}
			else
			{
				DistributeExpenseCashRemainingAmount(uow, routeList, expense, amount);
			}
		}

		private void DistributeExpenseCashRemainingAmount(IUnitOfWork uow, RouteList routeList, Expense expense, decimal amount)
		{
			var operation = new OrganisationCashMovementOperation
			{
				OperationTime = DateTime.Now,
				Organisation = _organizationRepository.GetCommonOrganisation(uow),
				Amount = -amount
			};

			var address = routeList.Addresses.First(x => x.TotalCash > 0);
			var document = CreateRouteListItemCashDistributionDocument(operation, address, expense);

			Save(uow, operation, document);
		}

		private OrganisationCashMovementOperation CreateOrganisationCashMovementOperation(
			IUnitOfWork uow, RouteListItem address)
		{
			var hasReceipt = _orderRepository.OrderHasSentReceipt(uow, address.Order.Id);

			return new OrganisationCashMovementOperation
			{
				OperationTime = DateTime.Now,
				Organisation = hasReceipt
					? address.Order.Contract.Organization
					: _organizationRepository.GetCommonOrganisation(uow)
			};
		}

		private RouteListItemCashDistributionDocument CreateRouteListItemCashDistributionDocument(
			OrganisationCashMovementOperation operation, RouteListItem address, Income income)
		{
			return new RouteListItemCashDistributionDocument
			{
				Organisation = operation.Organisation,
				CreationDate = DateTime.Now,
				LastEditedTime = DateTime.Now,
				Author = income.Casher,
				LastEditor = income.Casher,
				RouteListItem = address,
				Employee = income.Employee,
				Income = income,
				OrganisationCashMovementOperation = operation,
				Amount = operation.Amount
			};
		}

		private RouteListItemCashDistributionDocument CreateRouteListItemCashDistributionDocument(
			OrganisationCashMovementOperation operation, RouteListItem address, Expense expense)
		{
			return new RouteListItemCashDistributionDocument
			{
				Organisation = operation.Organisation,
				CreationDate = DateTime.Now,
				LastEditedTime = DateTime.Now,
				Author = expense.Casher,
				LastEditor = expense.Casher,
				RouteListItem = address,
				Employee = expense.Employee,
				Expense = expense,
				OrganisationCashMovementOperation = operation,
				Amount = operation.Amount
			};
		}

		private void Save(IUnitOfWork uow, OrganisationCashMovementOperation operation,
			RouteListItemCashDistributionDocument routeListItemCashdistributionDoc)
		{
			uow.Save(operation);
			uow.Save(routeListItemCashdistributionDoc);
		}
	}
}
