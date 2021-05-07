using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Library.DataAccess
{
    public class DriverMobileAppActionRecordData : IDriverMobileAppActionRecordData
    {
        private readonly IUnitOfWork unitOfWork;

        public DriverMobileAppActionRecordData(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public void RegisterAction(
            Employee driver,
            DriverMobileAppActionType driverMobileAppActionType,
            DateTime actionTime)
        {
            var record = new DriverMobileAppActionRecord()
            {
                Driver = driver,
                Action = driverMobileAppActionType,
                ActionDatetime = actionTime,
                RecievedDatetime = DateTime.Now
            };

            unitOfWork.Save(record);
            unitOfWork.Commit();
        }
    }
}
