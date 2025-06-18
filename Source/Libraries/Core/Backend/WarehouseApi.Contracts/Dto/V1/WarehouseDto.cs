﻿namespace WarehouseApi.Contracts.Dto.V1
{
	/// <summary>
	/// DTO для представления склада
	/// </summary>
	public class WarehouseDto
    {
        /// <summary>
        /// Идентификатор склада
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название склада
        /// </summary>
        public string Name { get; set; }
    }
}
