using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Settings.Common;

namespace Vodovoz.Settings.Database.Common
{
	[Obsolete("Необходимо разнести настройки по соответствующим теме классам")]
	public class GeneralSettings : IGeneralSettings
	{
		private readonly ISettingsController _settingsController;

		public const string PaymentWriteOffAllowedFinancialExpenseCategoriesParameterName =
			"Accounting.PaymentWriteOff.AllowedFinancialExpenseCategories";
		private const string _routeListPrintedFormPhones = "route_list_printed_form_phones";
		private const string _canAddForwarderToLargus = "can_add_forwarders_to_largus";
		private const string _canAddForwarderToMinivan = "can_add_forwarders_to_minivan";
		private const string _orderAutoComment = "OrderAutoComment";
		private const string _subdivisionsToInformComplaintHasNoDriverParameterName = "SubdivisionsToInformComplaintHasNoDriver";
		private const string _subdivisionsForAlternativePricesName = "SubdivisionsForAlternativePricesName";
		private const string _driversUnclosedRouteListsHavingDebtMaxCount = "drivers_stop_list_unclosed_route_lists_max_count";
		private const string _driversRouteListsMaxDebtSum = "drivers_stop_list_route_lists_max_debt_sum";
		private const string _isClientsSecondOrderDiscountActive = "is_client_second_order_discount_active";
		private const string _isOrderWaitUntilActive = "is_order_wait_until_active";
		private const string _isFastDelivery19LBottlesLimitActive = "is_fast_delivery_19l_bottles_limit_active";
		private const string _fastDelivery19LBottlesLimitCount = "fast_delivery_19l_bottles_limit_count";
		private const string _warehousesForPricesAndStocksIntegrationName = "warehouses_for_prices_and_stocks_integration_name";
		private const string _billAdditionalInfo = "bill_additional_info";
		private const string _carLoadDocumentInfoString = "car_load_document_info_string";
		private const string _upcomingTechInspectForOurCars = nameof(UpcomingTechInspectForOurCars);
		private const string _upcomingTechInspectFoRaskatCars = nameof(UpcomingTechInspectForRaskatCars);
		private const string _carTechnicalCheckupEndingNotificationDaysBefore = "CarTechnicalCheckup.EndingNotificationDaysBefore";
		private const string _fastDeliveryIntervalFrom = nameof(FastDeliveryIntervalFrom);
		private const string _fastDeliveryMaximumPermissibleLateMinutes = nameof(FastDeliveryMaximumPermissibleLateMinutes);
		private const string _defaultPaymentDeferment = "default_payment_deferment";
		private const string _defaultVatRate = "default_vat_rate";

		public GeneralSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string GetRouteListPrintedFormPhones => _settingsController.GetStringValue(_routeListPrintedFormPhones);

		public void UpdateRouteListPrintedFormPhones(string text) =>
			_settingsController.CreateOrUpdateSetting(_routeListPrintedFormPhones, text);

		public bool GetCanAddForwardersToLargus => _settingsController.GetValue<bool>(_canAddForwarderToLargus);

		public bool GetCanAddForwardersToMinivan => _settingsController.GetValue<bool>(_canAddForwarderToMinivan);

		public string OrderAutoComment => _settingsController.GetStringValue(_orderAutoComment);

		public string SubdivisionsToInformComplaintHasNoDriverParameterName => _subdivisionsToInformComplaintHasNoDriverParameterName;
		public string SubdivisionsAlternativePricesName => _subdivisionsForAlternativePricesName;
		public string WarehousesForPricesAndStocksIntegrationName => _warehousesForPricesAndStocksIntegrationName;

		public int[] SubdivisionsToInformComplaintHasNoDriver => GetSubdivisionsToInformComplaintHasNoDriver();
		public int[] SubdivisionsForAlternativePrices => GetSubdivisionsForAlternativePrices();
		public int[] WarehousesForPricesAndStocksIntegration => GetWarehousesForPricesAndStocksIntegration();

		public void UpdateOrderAutoComment(string value) =>
			_settingsController.CreateOrUpdateSetting(_orderAutoComment, value);

		public void UpdateCanAddForwardersToLargus(bool value) =>
			_settingsController.CreateOrUpdateSetting(_canAddForwarderToLargus, value.ToString());

		public void UpdateCanAddForwardersToMinivan(bool value) =>
			_settingsController.CreateOrUpdateSetting(_canAddForwarderToMinivan, value.ToString());

		public void UpdateSubdivisionsForParameter(List<int> subdivisionsToAdd, List<int> subdivisionsToRemoves, string parameterName)
		{
			int[] subdivisions;

			switch(parameterName)
			{
				case _subdivisionsToInformComplaintHasNoDriverParameterName:
					subdivisions = SubdivisionsToInformComplaintHasNoDriver;
					break;
				case _subdivisionsForAlternativePricesName:
					subdivisions = SubdivisionsForAlternativePrices;
					break;
				default:
					throw new NotSupportedException("Параметр подразделений не поддерживается.");
			}

			var result = subdivisions
				.Concat(subdivisionsToAdd)
				.Except(subdivisionsToRemoves)
				.Distinct()
				.ToArray();

			_settingsController.CreateOrUpdateSetting(parameterName, string.Join(", ", result));
		}

		public void UpdateWarehousesIdsForParameter(IEnumerable<int> warehousesIds, string parameterName)
		{
			_settingsController.CreateOrUpdateSetting(parameterName, string.Join(", ", warehousesIds));
		}

		public int DriversUnclosedRouteListsHavingDebtMaxCount => _settingsController.GetValue<int>(_driversUnclosedRouteListsHavingDebtMaxCount);

		public void UpdateDriversUnclosedRouteListsHavingDebtMaxCount(int value) =>
			_settingsController.CreateOrUpdateSetting(_driversUnclosedRouteListsHavingDebtMaxCount, value.ToString());

		public int DriversRouteListsMaxDebtSum => _settingsController.GetValue<int>(_driversRouteListsMaxDebtSum);

		public void UpdateDriversRouteListsMaxDebtSum(decimal value) =>
			_settingsController.CreateOrUpdateSetting(_driversRouteListsMaxDebtSum, value.ToString());
		public bool GetIsClientsSecondOrderDiscountActive => _settingsController.GetValue<bool>(_isClientsSecondOrderDiscountActive);

		public void UpdateIsClientsSecondOrderDiscountActive(bool value) =>
			_settingsController.CreateOrUpdateSetting(_isClientsSecondOrderDiscountActive, value.ToString());

		public bool GetIsOrderWaitUntilActive => _settingsController.GetValue<bool>(_isOrderWaitUntilActive);
		public void UpdateIsOrderWaitUntilActive(bool value) =>
			_settingsController.CreateOrUpdateSetting(_isOrderWaitUntilActive, value.ToString());

		public bool IsFastDelivery19LBottlesLimitActive => _settingsController.GetValue<bool>(_isFastDelivery19LBottlesLimitActive);
		public void UpdateIsFastDelivery19LBottlesLimitActive(bool value) =>_settingsController.CreateOrUpdateSetting(_isFastDelivery19LBottlesLimitActive, value.ToString());
		public int FastDelivery19LBottlesLimitCount => _settingsController.GetValue<int>(_fastDelivery19LBottlesLimitCount);

		public void UpdateFastDelivery19LBottlesLimitCount(int value) => _settingsController.CreateOrUpdateSetting(_fastDelivery19LBottlesLimitCount, value.ToString());
		public void UpdateUpcomingTechInspectForOurCars(int value) => _settingsController.CreateOrUpdateSetting(_upcomingTechInspectForOurCars, value.ToString());
		public void UpdateUpcomingTechInspectForRaskatCars(int value) => _settingsController.CreateOrUpdateSetting(_upcomingTechInspectFoRaskatCars, value.ToString());
		public void UpdateCarTechnicalCheckupEndingNotificationDaysBefore(int value) => _settingsController.CreateOrUpdateSetting(_carTechnicalCheckupEndingNotificationDaysBefore, value.ToString());
		public int UpcomingTechInspectForOurCars => _settingsController.GetValue<int>(_upcomingTechInspectForOurCars);
		public int UpcomingTechInspectForRaskatCars => _settingsController.GetValue<int>(_upcomingTechInspectFoRaskatCars);
		public int CarTechnicalCheckupEndingNotificationDaysBefore => _settingsController.GetValue<int>(_carTechnicalCheckupEndingNotificationDaysBefore);
		public FastDeliveryIntervalFromEnum FastDeliveryIntervalFrom => _settingsController.GetValue<FastDeliveryIntervalFromEnum>(_fastDeliveryIntervalFrom);
		public void UpdateFastDeliveryIntervalFrom(FastDeliveryIntervalFromEnum value) => _settingsController.CreateOrUpdateSetting(_fastDeliveryIntervalFrom, value.ToString());
		public int FastDeliveryMaximumPermissibleLateMinutes => _settingsController.GetValue<int>(_fastDeliveryMaximumPermissibleLateMinutes);
		public void UpdateFastDeliveryMaximumPermissibleLateMinutes(int value) => _settingsController.CreateOrUpdateSetting(_fastDeliveryMaximumPermissibleLateMinutes, value.ToString());

		private int[] GetSubdivisionsToInformComplaintHasNoDriver()
		{
			return ParseIdsFromString(_subdivisionsToInformComplaintHasNoDriverParameterName);
		}

		private int[] GetSubdivisionsForAlternativePrices()
		{
			return ParseIdsFromString(_subdivisionsForAlternativePricesName);
		}

		private int[] GetWarehousesForPricesAndStocksIntegration()
		{
			return ParseIdsFromString(_warehousesForPricesAndStocksIntegrationName);
		}

		public string GetBillAdditionalInfo
		{
			get
			{
				try
				{
					return _settingsController.GetStringValue(_billAdditionalInfo);
				}
				catch(SettingException)
				{
					return "";
				}
			}
		}

		public void UpdateBillAdditionalInfo(string value) =>
			_settingsController.CreateOrUpdateSetting(_billAdditionalInfo, value);

		public string GetCarLoadDocumentInfoString
		{
			get
			{
				try
				{
					return _settingsController.GetStringValue(_carLoadDocumentInfoString);
				}
				catch(SettingException)
				{
					return "";
				}
			}
		}

		public int[] PaymentWriteOffAllowedFinancialExpenseCategories => GetPaymentWriteOffAllowedFinancialExpenseCategoriesParameter();

		public void UpdateCarLoadDocumentInfoString(string value) =>
			_settingsController.CreateOrUpdateSetting(_carLoadDocumentInfoString, value);

		private int[] ParseIdsFromString(string parameterName)
		{
			string parameterValue;
			try
			{
				parameterValue = _settingsController.GetStringValue(parameterName);
			}
			catch(SettingException)
			{
				parameterValue = "";
			}
			var splitedIds = parameterValue.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			return splitedIds
				.Select(x => int.Parse(x))
				.ToArray();
		}

		private int[] GetPaymentWriteOffAllowedFinancialExpenseCategoriesParameter()
		{
			return ParseIdsFromString(PaymentWriteOffAllowedFinancialExpenseCategoriesParameterName);
		}

		public void UpdatePaymentWriteOffAllowedFinancialExpenseCategoriesParameter(int[] ids, string parameterName)
		{
			_settingsController.CreateOrUpdateSetting(parameterName, string.Join(", ", ids));
		}
		
		public int DefaultPaymentDeferment => _settingsController.GetValue<int>(_defaultPaymentDeferment);
		
		public void SaveDefaultPaymentDeferment(int defaultPaymentDeferment)
		{
			_settingsController.CreateOrUpdateSetting(_defaultPaymentDeferment, defaultPaymentDeferment.ToString());
		}

		public decimal DefaultVatRate => _settingsController.GetValue<decimal>(_defaultVatRate);
		public void SaveDefaultVatRate(decimal defaultVatRate)
		{
			_settingsController.CreateOrUpdateSetting(_defaultVatRate, defaultVatRate.ToString());
		}
	}
}
