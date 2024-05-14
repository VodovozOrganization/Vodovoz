﻿using System;
using System.Collections.Generic;

namespace Vodovoz.Settings.Common
{
	[Obsolete("Необходимо разнести настройки по соответствующим теме классам")]
	public interface IGeneralSettings
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
		bool IsFastDelivery19LBottlesLimitActive { get; }
		int FastDelivery19LBottlesLimitCount { get; }
		void UpdateIsFastDelivery19LBottlesLimitActive(bool value);
		void UpdateFastDelivery19LBottlesLimitCount(int fastDelivery19LBottlesLimitCount);

		int UpcomingTechInspectForOurCars { get; }
		int UpcomingTechInspectForRaskatCars { get; }
		void UpdateUpcomingTechInspectForOurCars(int upcomingTechInspectForOurCars);
		void UpdateUpcomingTechInspectForRaskatCars(int upcomingTechInspectForRaskatCars);

		string GetBillAdditionalInfo { get; }
		void UpdateBillAdditionalInfo(string value);
		string GetCarLoadDocumentInfoString { get; }
		void UpdateCarLoadDocumentInfoString(string value);

		FastDeliveryIntervalFromEnum FastDeliveryIntervalFrom { get; }
		void UpdateFastDeliveryIntervalFrom(FastDeliveryIntervalFromEnum value);

		int FastDeliveryMaximumPermissibleLateMinutes { get; }
		void UpdateFastDeliveryMaximumPermissibleLateMinutes(int value);
	}
}
