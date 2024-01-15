using EventsApi.Library.Services;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Interfaces.Employees;
using Vodovoz.Core.Domain.Interfaces.Logistics;

namespace EventsApi.Library.Models
{
	public class WarehouseEventsModel : LogisticsEventsModel
	{
		public WarehouseEventsModel(
			ILogger<WarehouseEventsModel> logger,
			IUnitOfWork unitOfWork,
			ICompletedDriverWarehouseEventProxyRepository completedDriverWarehouseEventProxyRepository,
			IEmployeeWithLoginRepository employeeWithLoginRepository,
			IDriverWarehouseEventQrDataHandler driverWarehouseEventQrDataHandler)
			: base(
				logger,
				unitOfWork,
				completedDriverWarehouseEventProxyRepository,
				employeeWithLoginRepository,
				driverWarehouseEventQrDataHandler,
				EmployeeType.WarehouseEmployee)
		{
			
		}
	}
}
