using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Contacts;

namespace Vodovoz.Application.Contacts
{
	internal class PhoneService : IPhoneService
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGenericRepository<Phone> _phoneRepository;
		private readonly IGenericRepository<RouteList> _routeListRepository;
		private readonly IGenericRepository<Order> _orderRepository;
		private readonly IGenericRepository<Employee> _employeeRepository;
		private readonly IPhoneSettings _phoneSettings;

		public PhoneService(
			IUnitOfWorkFactory unitOfWorkFactory,
			IGenericRepository<Employee> employeeRepository,
			IPhoneSettings phoneSettings,
			IGenericRepository<RouteList> routeListRepository,
			IGenericRepository<Phone> phoneRepository,
			IGenericRepository<Order> orderRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
			_phoneSettings = phoneSettings
				?? throw new ArgumentNullException(nameof(phoneSettings));
			_routeListRepository = routeListRepository
				?? throw new ArgumentNullException(nameof(routeListRepository));
			_phoneRepository = phoneRepository
				?? throw new ArgumentNullException(nameof(phoneRepository));
			_orderRepository = orderRepository
				?? throw new ArgumentNullException(nameof(orderRepository));
		}

		public string GetCourierDispatcherPhone()
		{
			return _phoneSettings.CourierDispatcherPhone;
		}

		public Result<string> GetCourierPhonesByTodayOrderContactPhone(string counterpartyPhoneNumber)
		{
			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var counterpartyPhoneIds = _phoneRepository
					.GetValue(unitOfWork,
						p => p.Id,
						p => p.DigitsNumber == counterpartyPhoneNumber
							&& (p.Counterparty != null
								|| p.DeliveryPoint != null));

				if(!counterpartyPhoneIds.Any())
				{
					return Result.Failure<string>(Vodovoz.Errors.Contacts.PhoneErrors.NotFound);
				}

				var orderId = _orderRepository
					.GetValue(
						unitOfWork,
						o => o.Id,
						o => counterpartyPhoneIds.Contains(o.ContactPhone.Id)
							&& o.OrderStatus == OrderStatus.OnTheWay)
					.FirstOrDefault();

				if(orderId == 0)
				{
					return Result.Failure<string>(Vodovoz.Errors.Orders.OrderErrors.NotFound);
				}

				var routeList = _routeListRepository
					.Get(
						unitOfWork,
						rl => rl.Addresses
							.Any(rla => rla.Order.Id == orderId
								&& rla.Status != RouteListItemStatus.Transfered))
					.FirstOrDefault();

				if(routeList is null)
				{
					return Result.Failure<string>(Vodovoz.Errors.Logistics.RouteListErrors.NotFound);
				}

				var driver = _employeeRepository
					.Get(unitOfWork, d => d.Id == routeList.Driver.Id)
					.FirstOrDefault();

				if(driver is null)
				{
					return Result.Failure<string>(Vodovoz.Errors.Employees.DriverErrors.NotFound);
				}

				if(!driver.CanRecieveCounterpartyCalls)
				{
					return Result.Failure<string>(Vodovoz.Errors.Employees.DriverErrors.CantRecieveCounterpartyCalls);
				}

				var phone = driver.PhoneForCounterpartyCalls;

				if(phone is null)
				{
					return Result.Failure<string>(Vodovoz.Errors.Contacts.PhoneErrors.NotFound);
				}

				return Result.Success(phone.DigitsNumber);
			}
		}
	}
}
