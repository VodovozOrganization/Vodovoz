using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories;
using Vodovoz.Errors;
using Vodovoz.Settings.Contacts;

namespace Vodovoz.Application.Contacts
{
	internal class PhoneService : IPhoneService
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGenericRepository<RouteList> _routeListRepository;
		private readonly IGenericRepository<Employee> _employeeRepository;
		private readonly IPhoneSettings _phoneSettings;

		public PhoneService(
			IUnitOfWorkFactory unitOfWorkFactory,
			IGenericRepository<Employee> employeeRepository,
			IPhoneSettings phoneSettings,
			IGenericRepository<RouteList> routeListRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
			_phoneSettings = phoneSettings
				?? throw new ArgumentNullException(nameof(phoneSettings));
			_routeListRepository = routeListRepository
				?? throw new ArgumentNullException(nameof(routeListRepository));
		}

		public string GetCourierDispatcherPhone()
		{
			return _phoneSettings.CourierDispatcherPhone;
		}

		public Result<string> GetCourierPhoneNumberForOrder(int orderId)
		{
			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var routeList = _routeListRepository
					.Get(
						unitOfWork,
						rl => rl.Addresses
							.Any(rla => rla.Order.Id == orderId
								&& rla.Status != RouteListItemStatus.Transfered))
					.FirstOrDefault();

				if(routeList is null)
				{
					return Result.Failure<string>(Errors.Logistics.RouteList.NotFound);
				}

				var driver = _employeeRepository
					.Get(unitOfWork, d => d.Id == routeList.Driver.Id)
					.FirstOrDefault();

				if(driver is null)
				{
					return Result.Failure<string>(Errors.Employees.Driver.NotFound);
				}

				var phone = driver.Phones.FirstOrDefault();

				if(phone is null)
				{
					return Result.Failure<string>(Errors.Contacts.Phone.NotFound);
				}

				return Result.Success(phone.DigitsNumber);
			}
		}
	}
}
