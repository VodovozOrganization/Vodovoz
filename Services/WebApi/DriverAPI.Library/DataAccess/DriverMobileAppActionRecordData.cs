using DriverAPI.Library.Converters;
using DriverAPI.Library.Models;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Library.DataAccess
{
	public class DriverMobileAppActionRecordData : IDriverMobileAppActionRecordData
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly ActionTypeConverter actionTypeConverter;

		public DriverMobileAppActionRecordData(IUnitOfWork unitOfWork, ActionTypeConverter actionTypeConverter)
		{
			this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			this.actionTypeConverter = actionTypeConverter ?? throw new ArgumentNullException(nameof(actionTypeConverter));
		}

		public void RegisterAction(Employee driver, APIDriverAction driverAction)
		{
			var record = new DriverMobileAppActionRecord()
			{
				Driver = driver,
				Action = actionTypeConverter.ConvertToDriverMobileAppActionType(driverAction.ActionType),
				ActionDatetime = driverAction.ActionTime,
				RecievedDatetime = DateTime.Now
			};

			unitOfWork.Save(record);
			unitOfWork.Commit();
		}

		public void RegisterActionsRangeForDriver(Employee driver, IEnumerable<APIDriverAction> driverActions)
		{
			foreach (var action in driverActions)
			{
				var record = new DriverMobileAppActionRecord()
				{
					Driver = driver,
					Action = actionTypeConverter.ConvertToDriverMobileAppActionType(action.ActionType),
					ActionDatetime = action.ActionTime,
					RecievedDatetime = DateTime.Now
				};

				unitOfWork.Save(record);
			}

			unitOfWork.Commit();
		}
	}
}
