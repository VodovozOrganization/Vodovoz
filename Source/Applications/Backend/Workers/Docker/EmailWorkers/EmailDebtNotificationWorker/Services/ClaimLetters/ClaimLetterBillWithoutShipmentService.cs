using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Employees;

namespace EmailDebtNotificationWorker.Services.ClaimLetters
{
	public class ClaimLetterBillWithoutShipmentService : IClaimLetterBillWithoutShipmentService
	{
		private const int _daysToCheckExistingBills = 3;

		private readonly IGenericRepository<OrderWithoutShipmentForDebt> _orderWithoutShipmentForDebtRepository;
		private readonly IEmployeeRepository _employeeRepository;

		public ClaimLetterBillWithoutShipmentService(
			IGenericRepository<OrderWithoutShipmentForDebt> orderWithoutShipmentForDebtRepository,
			IEmployeeRepository employeeRepository)
		{
			_orderWithoutShipmentForDebtRepository = orderWithoutShipmentForDebtRepository ?? throw new ArgumentNullException(nameof(orderWithoutShipmentForDebtRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		public async Task<OrderWithoutShipmentForDebt> GetOrCreateOrderWithoutShipmentForDebtAsync(
			IUnitOfWork uow,
			int clientId,
			int organizationId,
			decimal debtSum,
			CancellationToken cancellationToken)
		{
			var dateFrom = DateTime.Now.AddDays(-_daysToCheckExistingBills);

			var existingBills = (await _orderWithoutShipmentForDebtRepository.GetAsync(
				uow,
				x => x.CreateDate.Value >= dateFrom
					&& x.Client.Id == clientId
					&& x.Organization.Id == organizationId,
				cancellationToken: cancellationToken))
			.Value;

			var existingBill = existingBills.FirstOrDefault();

			if(existingBill != null)
			{
				return existingBill;
			}

			var organization = uow.GetById<Organization>(organizationId)
				?? throw new InvalidOperationException($"Организация с Id {organizationId} не найдена");

			var client = uow.GetById<Counterparty>(clientId)
				?? throw new InvalidOperationException($"Клиент с Id {organizationId} не найден");

			var author = _employeeRepository.GetEmployeeForCurrentUser(uow);

			var newBill = new OrderWithoutShipmentForDebt
			{
				Client = client,
				Organization = organization,
				DebtSum = debtSum,
				Author = author
			};

			await uow.SaveAsync(newBill, cancellationToken: cancellationToken);

			return newBill;
		}
	}
}
