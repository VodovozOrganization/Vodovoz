using System;
using System.Collections.Generic;

namespace Vodovoz.Settings.Common
{
	[Obsolete("Необходимо разнести настройки по соответствующим теме классам")]
	public interface IGeneralSettings
	{
		string GetRouteListPrintedFormPhones { get; }
		void UpdateRouteListPrintedFormPhones(string text);

		bool GetCanAddForwardersToLargus { get; }
		bool GetCanAddForwardersToMinivan { get; }
		string OrderAutoComment { get; }
		void UpdateCanAddForwardersToLargus(bool value);
		void UpdateCanAddForwardersToMinivan(bool value);
		void UpdateOrderAutoComment(string value);

		int[] SubdivisionsToInformComplaintHasNoDriver { get; }
		int[] SubdivisionsForAlternativePrices { get; }
		int[] WarehousesForPricesAndStocksIntegration { get; }
		int[] PaymentWriteOffAllowedFinancialExpenseCategories { get; }

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

		int CarTechnicalCheckupEndingNotificationDaysBefore { get; }
		void UpdateCarTechnicalCheckupEndingNotificationDaysBefore(int value);

		string GetBillAdditionalInfo { get; }
		void UpdateBillAdditionalInfo(string value);
		string GetCarLoadDocumentInfoString { get; }
		void UpdateCarLoadDocumentInfoString(string value);

		FastDeliveryIntervalFromEnum FastDeliveryIntervalFrom { get; }
		void UpdateFastDeliveryIntervalFrom(FastDeliveryIntervalFromEnum value);

		int FastDeliveryMaximumPermissibleLateMinutes { get; }
		void UpdateFastDeliveryMaximumPermissibleLateMinutes(int value);
		void UpdatePaymentWriteOffAllowedFinancialExpenseCategoriesParameter(int[] ids, string parameterName);
		
		int DefaultPaymentDeferment { get; }
		void SaveDefaultPaymentDeferment(int defaultPaymentDeferment);
		
		decimal DefaultVatRate { get; }

		void SaveDefaultVatRate(decimal defaultVatRate);

		/// <summary>
		/// Наименование параметра, в котором хранится список номенклатур сервисного центра, которые будут использоваться для создания сделок Битрикс
		/// </summary>
		string ServiceNomenclaturesForBitrixDealsName { get; }

		/// <summary>
		/// Список номенклатур сервисного центра, которые будут использоваться для создания сделок Битрикс
		/// </summary>
		int[] ServiceNomenclaturesForBitrixDeals { get; }

		/// <summary>
		/// Обновляет список номенклатур сервисного центра, которые будут использоваться для создания сделок Битрикс
		/// </summary>
		/// <param name="nomenclatureIds">Список идентификаторов номенклатур</param>
		/// <param name="parameterName">Наименование параметра</param>
		void UpdateServiceNomenclaturesForBitrixDeals(IEnumerable<int> nomenclatureIds, string parameterName);
	}
}
