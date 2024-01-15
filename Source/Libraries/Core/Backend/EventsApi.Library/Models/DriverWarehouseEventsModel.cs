using EventsApi.Library.Services;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Interfaces.Employees;
using Vodovoz.Core.Data.Interfaces.Logistics.Cars;
using Vodovoz.Core.Domain.Interfaces.Logistics;

namespace EventsApi.Library.Models
{
	public class DriverWarehouseEventsModel : LogisticsEventsModel
	{
		public DriverWarehouseEventsModel(
			ILogger<DriverWarehouseEventsModel> logger,
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
