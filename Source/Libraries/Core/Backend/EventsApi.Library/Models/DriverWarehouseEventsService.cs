using EventsApi.Library.Services;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Interfaces.Employees;
using Vodovoz.Core.Data.Interfaces.Logistics.Cars;
using Vodovoz.Core.Domain.Interfaces.Logistics;

namespace EventsApi.Library.Models
{
	public class DriverWarehouseEventsService : LogisticsEventsService
	{
		public DriverWarehouseEventsService(
			ILogger<DriverWarehouseEventsService> logger,
			IUnitOfWork unitOfWork,
			ICompletedDriverWarehouseEventProxyRepository completedDriverWarehouseEventProxyRepository,
			IEmployeeWithLoginRepository employeeWithLoginRepository,
			ICarIdRepository carIdRepository,
			IDriverWarehouseEventQrDataHandler driverWarehouseEventQrDataHandler)
			: base(
				logger,
				unitOfWork,
				completedDriverWarehouseEventProxyRepository,
				employeeWithLoginRepository,
				driverWarehouseEventQrDataHandler,
				EmployeeType.Driver,
				carIdRepository)
		{
			
		}
	}
}
