using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Library.DataAccess
{
    public interface IDriverMobileAppActionRecordData
    {
        void RegisterAction(Employee driver, DriverMobileAppActionType completeOrderClicked, DateTime actionTime);
    }
}