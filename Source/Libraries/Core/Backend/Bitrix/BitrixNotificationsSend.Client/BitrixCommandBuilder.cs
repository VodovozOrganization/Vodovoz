using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BitrixNotificationsSend.Client
{
	/// <summary>
	/// Билдер команд для пакетного запроса batch.json в Битрикс24
	/// </summary>
	public static class BitrixCommandBuilder
	{
		/// <summary>
		/// Построение команды вызова REST-метода Битрикс24 с параметрами полей из DTO
		/// Поля берутся из json-сериализации DTO, поля со значением null пропускаются
		/// </summary>
		/// <param name="method">Имя REST-метода АПИ Битрикс 24, например crm.deal.add</param>
		/// <param name="fieldsDto">DTO с полями сущности, размеченными JsonPropertyName</param>
		/// <returns>Строка команды вида "method?fields[FIELD]=value&amp;..."</returns>
		public static string CreateCommand(string method, object fieldsDto)
		{
			using(var document = JsonDocument.Parse(JsonSerializer.Serialize(fieldsDto)))
			{
				var parameters = new List<string>();

				foreach(var property in document.RootElement.EnumerateObject())
				{
					if(property.Value.ValueKind == JsonValueKind.Null)
					{
						continue;
					}

					var value = property.Value.ValueKind == JsonValueKind.String
						? property.Value.GetString()
						: property.Value.GetRawText();

					parameters.Add($"fields[{property.Name}]={Uri.EscapeDataString(value)}");
				}

				return $"{method}?{string.Join("&", parameters)}";
			}
		}
	}
}
