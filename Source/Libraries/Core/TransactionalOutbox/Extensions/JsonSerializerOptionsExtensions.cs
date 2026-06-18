using System;
using System.Text.Json;
using TransactionalOutbox.Serialization;

namespace TransactionalOutbox.Extensions
{
	/// <summary>
	/// Сериализация и десериализация объектов в JSON-строки с использованием настроек из OutboxJsonSerializerOptions
	/// </summary>
	public static class OutboxJsonSerializerExtensions
	{
		/// <summary>
		/// Сериализует объект в JSON-строку.
		/// Если объект null — возвращает строку "null".
		/// </summary>
		public static string SerializeForOutbox<T>(this T value)
		{
			if (value == null)
			{
				return "null";
			}

			return JsonSerializer.Serialize(value, OutboxJsonSerializerOptions.Instance);
		}

		/// <summary>
		/// Десериализует JSON-строку в объект указанного типа.
		/// Если строка null или пустая — возвращает значение по умолчанию для типа T.
		/// </summary>
		public static T DeserializeFromOutbox<T>(this string json)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return default(T);
			}

			return JsonSerializer.Deserialize<T>(json, OutboxJsonSerializerOptions.Instance);
		}

		/// <summary>
		/// Десериализует JSON-строку в объект по Type.
		/// Если строка null или пустая — возвращает null.
		/// </summary>
		public static object DeserializeFromOutbox(this string json, Type type)
		{
			if (string.IsNullOrWhiteSpace(json) || type == null)
			{
				return null;
			}

			return JsonSerializer.Deserialize(json, type, OutboxJsonSerializerOptions.Instance);
		}
	}
}
