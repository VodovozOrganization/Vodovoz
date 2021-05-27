using DriverAPI.Library.Models;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;

namespace DriverAPI.Library.DataAccess
{
	public interface IDriverMobileAppActionRecordData
	{
		void RegisterAction(Employee driver, APIDriverAction driverAction);
		void RegisterActionsRangeForDriver(Employee driver, IEnumerable<APIDriverAction> driverActionModels);
	}
}