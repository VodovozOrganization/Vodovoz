using System.Collections.Generic;

namespace Vodovoz.Parameters
{
	public interface IGeneralSettingsParametersProvider
	{
		string GetRouteListPrintedFormPhones { get; }
		void UpdateRouteListPrintedFormPhones(string text);

		bool GetCanAddForwardersToLargus { get; }
		string OrderAutoComment { get; }
		void UpdateCanAddForwardersToLargus(bool value);
		void UpdateOrderAutoComment(string value);

		int[] SubdivisionsToInformComplaintHasNoDriver { get; }
		int[] SubdivisionsForAlternativePrices { get; }
		int[] WarehousesForPricesAndStocksIntegration { get; }

		string SubdivisionsToInformComplaintHasNoDriverParameterName { get; }
		string SubdivisionsAlternativePricesName { get; }
		string WarehousesForPricesAndStocksIntegrationName { get; }
		void UpdateSubdivisionsForParameter(List<int> subdivisionsToAdd, List<int> subdivisionsToRemoves, string parameterName);
		void UpdateWarehousesIdsForParameter(IEnumerable<int> warehousesIds, string parameterName);
		
		int DriversUnclosedRouteListsHavingDebtMaxCount { get; }

		void UpdateDriversUnclosedRouteListsHavingDebtMaxCount(int value);

		int DriversRouteListsMaxDebtSum { get; }

		void UpdateDriversRouteListsMaxDebtSum(decimal value);
		bool GetIsClientsSecondOrderDiscountActive { get; }
		void UpdateIsClientsSecondOrderDiscountActive(bool value);

		bool GetIsOrderWaitUntilActive { get; }
		void UpdateIsOrderWaitUntilActive(bool value);
	}
}
