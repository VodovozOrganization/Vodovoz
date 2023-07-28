using DriverAPI.Library.Converters;
using DriverAPI.Library.DTOs;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Library.Models
{
	internal class DriverMobileAppActionRecordModel : IDriverMobileAppActionRecordModel
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly ActionTypeConverter _actionTypeConverter;

		public DriverMobileAppActionRecordModel(IUnitOfWork unitOfWork, ActionTypeConverter actionTypeConverter)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_actionTypeConverter = actionTypeConverter ?? throw new ArgumentNullException(nameof(actionTypeConverter));
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
