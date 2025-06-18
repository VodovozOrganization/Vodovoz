﻿namespace WarehouseApi.Contracts.Dto.V1
{
	/// <summary>
	/// Информация о групповом Gtin
	/// </summary>
	public class GroupGtinDto
	{
		/// <summary>
		/// Номер товарной продукции GTIN (групповой)
		/// </summary>
		public string Gtin { get; set; }
		
		/// <summary>
		/// Количество товара в группе
		/// </summary>
		public int Count { get; set; }
	}
}
