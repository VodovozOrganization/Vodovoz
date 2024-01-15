using System.Collections.Generic;
using EventsApi.Library.Dtos;
using Vodovoz.Core.Domain.Logistics.Drivers;

namespace DriverAPI.Library.Models
{
	public interface IDriverWarehouseEventsModel
	{
		/*
		/// <summary>
		/// Конвертация и проверка данных из Qr кода на корректность
		/// </summary>
		/// <param name="qrData">данные Qr кода</param>
		/// <returns></returns>
		DriverWarehouseEventQrData ConvertAndValidateQrData(string qrData);
		/// <summary>
		/// Завершение события нахождения на складе
		/// </summary>
		/// <returns></returns>
		CompletedDriverWarehouseEvent CompleteDriverWarehouseEvent(
			DriverWarehouseEventQrData qrData, DriverWarehouseEventData eventData, Employee driver);
		IEnumerable<CompletedEventDto> GetTodayCompletedEventsForDriver(Employee driver);
		*/
	}
}
