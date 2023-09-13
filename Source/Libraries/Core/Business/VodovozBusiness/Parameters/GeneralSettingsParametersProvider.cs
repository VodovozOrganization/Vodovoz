using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Parameters
{
	public class GeneralSettingsParametersProvider : IGeneralSettingsParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;
		private const string _routeListPrintedFormPhones = "route_list_printed_form_phones";
		private const string _canAddForwarderToLargus = "can_add_forwarders_to_largus";
		private const string _orderAutoComment = "OrderAutoComment";
		private const string _subdivisionsToInformComplaintHasNoDriverParameterName = "SubdivisionsToInformComplaintHasNoDriver";
		private const string _subdivisionsForAlternativePricesName = "SubdivisionsForAlternativePricesName";
		private const string _driversUnclosedRouteListsHavingDebtMaxCount = "drivers_stop_list_unclosed_route_lists_max_count";
		private const string _driversRouteListsMaxDebtSum = "drivers_stop_list_route_lists_max_debt_sum";
		private const string _isClientsSecondOrderDiscountActive = "is_client_second_order_discount_active";
		private const string _warehousesForPricesAndStocksIntegrationName = "warehouses_for_prices_and_stocks_integration_name";

		public GeneralSettingsParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public string GetRouteListPrintedFormPhones => _parametersProvider.GetStringValue(_routeListPrintedFormPhones);

		public void UpdateRouteListPrintedFormPhones(string text) =>
			_parametersProvider.CreateOrUpdateParameter(_routeListPrintedFormPhones, text);

		public bool GetCanAddForwardersToLargus => _parametersProvider.GetValue<bool>(_canAddForwarderToLargus);

		public string OrderAutoComment => _parametersProvider.GetStringValue(_orderAutoComment);
		
		public string SubdivisionsToInformComplaintHasNoDriverParameterName => _subdivisionsToInformComplaintHasNoDriverParameterName;
		public string SubdivisionsAlternativePricesName => _subdivisionsForAlternativePricesName;
		public string WarehousesForPricesAndStocksIntegrationName => _warehousesForPricesAndStocksIntegrationName;

		public int[] SubdivisionsToInformComplaintHasNoDriver => GetSubdivisionsToInformComplaintHasNoDriver();
		public int[] SubdivisionsForAlternativePrices => GetSubdivisionsForAlternativePrices();
		public int[] WarehousesForPricesAndStocksIntegration => GetWarehousesForPricesAndStocksIntegration();

		public void UpdateOrderAutoComment(string value) =>
			_parametersProvider.CreateOrUpdateParameter(_orderAutoComment, value);

		public void UpdateCanAddForwardersToLargus(bool value) =>
			_parametersProvider.CreateOrUpdateParameter(_canAddForwarderToLargus, value.ToString());

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

			_parametersProvider.CreateOrUpdateParameter(parameterName, string.Join(", ", result));
		}

		public void UpdateWarehousesIdsForParameter(IEnumerable<int> warehousesIds, string parameterName)
		{
			_parametersProvider.CreateOrUpdateParameter(parameterName, string.Join(", ", warehousesIds));
		}

		public int DriversUnclosedRouteListsHavingDebtMaxCount => _parametersProvider.GetValue<int>(_driversUnclosedRouteListsHavingDebtMaxCount);

		public void UpdateDriversUnclosedRouteListsHavingDebtMaxCount(int value) =>
			_parametersProvider.CreateOrUpdateParameter(_driversUnclosedRouteListsHavingDebtMaxCount, value.ToString());

		public int DriversRouteListsMaxDebtSum => _parametersProvider.GetValue<int>(_driversRouteListsMaxDebtSum);

		public void UpdateDriversRouteListsMaxDebtSum(decimal value) =>
			_parametersProvider.CreateOrUpdateParameter(_driversRouteListsMaxDebtSum, value.ToString());
		public bool GetIsClientsSecondOrderDiscountActive => _parametersProvider.GetValue<bool>(_isClientsSecondOrderDiscountActive);

		public void UpdateIsClientsSecondOrderDiscountActive(bool value) =>
			_parametersProvider.CreateOrUpdateParameter(_isClientsSecondOrderDiscountActive, value.ToString());
		
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
		
		private int[] ParseIdsFromString(string parameterName, bool allowEmpty = true)
		{
			var parameterValue = _parametersProvider.GetParameterValue(parameterName, allowEmpty);
			var splitedIds = parameterValue.Split(new string[] {", "}, StringSplitOptions.RemoveEmptyEntries);
			return splitedIds
				.Select(x => int.Parse(x))
				.ToArray();
		}
	}
}
