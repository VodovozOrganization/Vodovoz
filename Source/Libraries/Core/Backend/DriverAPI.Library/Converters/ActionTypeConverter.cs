using DriverAPI.Library.DTOs;
using System;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Library.Converters
{
	public class ActionTypeConverter
	{
		public ActionDtoType ConvertToAPIActionType(DriverMobileAppActionType driverMobileAppActionType)
		{
			ActionDtoType result;

			switch (driverMobileAppActionType)
			{
				case DriverMobileAppActionType.OpenOrderInfoPanel:
					result = ActionDtoType.OpenOrderInfoPanel;
					break;
				case DriverMobileAppActionType.OpenOrderDeliveryPanel:
					result = ActionDtoType.OpenOrderDeliveryPanel;
					break;
				case DriverMobileAppActionType.OpenOrderReceiptionPanel:
					result = ActionDtoType.OpenOrderReceiptionPanel;
					break;
				case DriverMobileAppActionType.CompleteOrderClicked:
					result = ActionDtoType.CompleteOrderClicked;
					break;
				default:
					throw new ConverterException(nameof(driverMobileAppActionType), driverMobileAppActionType, $"Значение { driverMobileAppActionType } не поддерживается");
			}

			return result;
		}

		public DriverMobileAppActionType ConvertToDriverMobileAppActionType(ActionDtoType aPIActionType)
		{
			DriverMobileAppActionType result;

			switch (aPIActionType)
			{
				case ActionDtoType.OpenOrderInfoPanel:
					result = DriverMobileAppActionType.OpenOrderInfoPanel;
					break;
				case ActionDtoType.OpenOrderDeliveryPanel:
					result = DriverMobileAppActionType.OpenOrderDeliveryPanel;
					break;
				case ActionDtoType.OpenOrderReceiptionPanel:
					result = DriverMobileAppActionType.OpenOrderReceiptionPanel;
					break;
				case ActionDtoType.CompleteOrderClicked:
					result = DriverMobileAppActionType.CompleteOrderClicked;
					break;
				default:
					throw new ConverterException(nameof(aPIActionType), aPIActionType, $"Значение { aPIActionType } не поддерживается");
			}

			return result;
		}
	}
}
