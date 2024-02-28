﻿using System.Collections.Generic;

namespace DriverApi.Contracts.V4
{
	/// <summary>
	/// Ответ сервера о статусе платежа по QR-коду
	/// </summary>
	public class OrderQRPaymentStatusResponseDto
	{
		/// <summary>
		/// Доступные типы оплаты для смена
		/// </summary>
		public IEnumerable<PaymentDtoType> AvailablePaymentTypes { get; set; }

		/// <summary>
		/// Возможно ли получить QR-код
		/// </summary>
		public bool CanReceiveQR { get; set; }

		/// <summary>
		/// Статус оплаты по QR-коду
		/// </summary>
		public QRPaymentDTOStatus? QRPaymentStatus { get; set; }

		/// <summary>
		/// Изображение QR-кода в формате Png закодированное в строку Base64
		/// </summary>
		public string QRCode { get; set; }
	}
}
