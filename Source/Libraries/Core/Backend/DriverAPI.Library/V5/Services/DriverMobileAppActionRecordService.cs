using DriverApi.Contracts.V5;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Library.V5.Services
{
	internal class DriverMobileAppActionRecordService : IDriverMobileAppActionRecordService
	{
		private readonly IUnitOfWork _unitOfWork;

		public DriverMobileAppActionRecordService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public void RegisterAction(Employee driver, DriverActionDto driverAction)
		{

		}

		public void RegisterAction(int driverId, DriverMobileAppActionType actionType, DateTime actionTime, DateTime recievedTime, string result)
			=> RegisterAction(_unitOfWork.GetById<Employee>(driverId), actionType, actionTime, recievedTime, result);

		public void RegisterAction(Employee driver, DriverMobileAppActionType actionType, DateTime actionTime, DateTime recievedTime, string result)
		{

		}

		public void RegisterActionsRangeForDriver(Employee driver, IEnumerable<DriverActionDto> driverActions)
		{

		}
	}
}
