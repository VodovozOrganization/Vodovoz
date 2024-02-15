using EventsApi.Library.Services;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Interfaces.Employees;
using Vodovoz.Core.Domain.Interfaces.Logistics;
using Vodovoz.Settings.Employee;

namespace EventsApi.Library.Models
{
	public class WarehouseEventsService : LogisticsEventsService
	{
		public WarehouseEventsService(
			ILogger<WarehouseEventsService> logger,
			IUnitOfWork unitOfWork,
			ICompletedDriverWarehouseEventProxyRepository completedDriverWarehouseEventProxyRepository,
			IEmployeeWithLoginRepository employeeWithLoginRepository,
			IDriverWarehouseEventQrDataHandler driverWarehouseEventQrDataHandler,
			IDriverWarehouseEventSettings driverWarehouseEventSettings)
			: base(
				logger,
				unitOfWork,
				completedDriverWarehouseEventProxyRepository,
				employeeWithLoginRepository,
				driverWarehouseEventQrDataHandler,
				driverWarehouseEventSettings,
				EmployeeType.WarehouseEmployee)
		{
			
		}
	}
}
