using DriverApi.Contracts.V5;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Library.V5.Services
{
	public interface IDriverMobileAppActionRecordService
	{
		void RegisterAction(Employee driver, DriverActionDto driverAction);
		void RegisterAction(int driverId, DriverMobileAppActionType actionType, DateTime actionTime, DateTime recievedTime, string result);
		void RegisterAction(Employee driver, DriverMobileAppActionType actionType, DateTime actionTime, DateTime recievedTime, string result);
		void RegisterActionsRangeForDriver(Employee driver, IEnumerable<DriverActionDto> driverActions);
	}
}
