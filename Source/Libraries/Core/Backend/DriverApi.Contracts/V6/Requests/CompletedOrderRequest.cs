using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V6.Requests
{
	/// <summary>
	/// Запрос на завершение заказа
	/// </summary>
	public class CompletedOrderRequest : IDriverOrderShipmentInfo, IDriverComplaintInfo
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		[Required]
		public int OrderId { get; set; }

		/// <summary>
		/// Количество возвращаемых бутылей
		/// </summary>
		[Required]
		public int BottlesReturnCount { get; set; }

		/// <summary>
		/// Рейтинг адреса от водителя
		/// </summary>
		[Required]
		public int Rating { get; set; }

		/// <summary>
		/// Причина низкого рейтинга адреса
		/// </summary>
		public int DriverComplaintReasonId { get; set; }

		/// <summary>
		/// Комментарий низкого рейтинга адреса
		/// </summary>
		public string OtherDriverComplaintReasonComment { get; set; }

		/// <summary>
		/// Комментарий в случае меньшего количества бутылей на возврат
		/// </summary>
		public string DriverComment { get; set; }

		/// <summary>
		/// Отсканированные бутыли
		/// </summary>
		public IEnumerable<OrderScannedItemDto> ScannedBottles { get; set; }

		/// <summary>
		/// Причина не отсканированности бутылей
		/// </summary>
		public string UnscannedBottlesReason { get; set; }

		/// <summary>
		/// Время операции на стороне мобильного приложения водителя
		/// </summary>
		[Required]
		public DateTime ActionTimeUtc { get; set; }

		IEnumerable<ITrueMarkOrderItemScannedInfo> ITrueMarkOrderScannedInfo.ScannedItems => ScannedBottles;

		string ITrueMarkOrderScannedInfo.UnscannedCodesReason => UnscannedBottlesReason;
	}
}
