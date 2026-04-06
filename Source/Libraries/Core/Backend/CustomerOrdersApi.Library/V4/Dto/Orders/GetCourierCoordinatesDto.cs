using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	/// <summary>
	/// Запрос получения координат курьера для отображения на карте
	/// </summary>
	public class GetCourierCoordinatesDto
	{
		/// <summary>
		/// id источника запроса
		/// </summary>
		[Required]
		public Source Source { get; set; }

		/// <summary>
		/// id клиента в ERP
		/// </summary>
		[Required]
		public int CounterpartyErpId { get; set; }

		/// <summary>
		/// id клиента в БД ИПЗ
		/// </summary>
		[Required]
		public Guid ExternalCounterpartyId { get; set; }

		/// <summary>
		/// Номер заказа
		/// </summary>
		public int? OrderId { get; set; }

		/// <summary>
		/// Номер онлайн заказа
		/// </summary>
		public int? OnlineOrderId { get; set; }
	}
}
