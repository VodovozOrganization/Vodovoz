using DriverAPI.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Models.TrueMark;

namespace DriverAPI.DTOs.V2
{
	public class CompletedOrderRequestDto : IDriverCompleteOrderInfo
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		[Required]
		public int OrderId { get; set; }

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

		public string DriverComment { get; set; }

		public IEnumerable<OrderScannedItemDto> ScannedBottles { get; set; }

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
