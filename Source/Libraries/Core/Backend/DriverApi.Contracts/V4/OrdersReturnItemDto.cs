﻿namespace DriverApi.Contracts.V4
{
	/// <summary>
	/// Строка оборудования на возврат
	/// </summary>
	public class OrdersReturnItemDto
	{
		/// <summary>
		/// Название
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		public int Count { get; set; }
	}
}
