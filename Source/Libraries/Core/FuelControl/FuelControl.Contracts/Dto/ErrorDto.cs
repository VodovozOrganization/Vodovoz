﻿using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Ошибка при выполнении запроса к серверу
	/// </summary>
	public class ErrorDto
	{
		/// <summary>
		/// Тип ошибки
		/// </summary>
		[JsonPropertyName("type")]
		public string Type { get; set; }

		/// <summary>
		/// Описание ошибки для пользователя
		/// </summary>
		[JsonPropertyName("message")]
		public IEnumerable<string> Messages { get; set; }
	}
}
