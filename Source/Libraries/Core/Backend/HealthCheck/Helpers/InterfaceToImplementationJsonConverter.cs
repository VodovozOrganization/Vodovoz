using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VodovozHealthCheck.Helpers
{
	/// <summary>
	///		Конвертер JSON для десериализации интерфейсных свойств в конкретную реализацию.
	///		Применяется, когда в модельном классе используется интерфейс, а в JSON нужно получить экземпляр конкретного класса.
	/// </summary>
	/// <typeparam name="TInterface">Интерфейсный тип, который обрабатывается конвертером.</typeparam>
	/// <typeparam name="TImplementation">Конкретная реализация интерфейса. Должна иметь конструктор без параметров.</typeparam>
	public class InterfaceToImplementationJsonConverter<TInterface, TImplementation> : JsonConverter<TInterface>
		where TImplementation : TInterface, new()
	{
		/// <summary>
		///		Десериализует JSON в конкретную реализацию <typeparamref name="TImplementation"/>.
		///		Возвращает значение как <typeparamref name="TInterface"/>.
		/// </summary>
		/// <param name="reader">Ридер UTF-8 JSON.</param>
		/// <param name="typeToConvert">Тип для конвертации (игнорируется, используется <typeparamref name="TImplementation"/>).</param>
		/// <param name="options">Опции сериализации/десериализации.</param>
		/// <returns>Экземпляр <typeparamref name="TInterface"/>, созданный из <typeparamref name="TImplementation"/>.</returns>
		public override TInterface Read(
			ref Utf8JsonReader reader,
			Type typeToConvert,
			JsonSerializerOptions options)
		{
			return JsonSerializer.Deserialize<TImplementation>(ref reader, options);
		}

		/// <summary>
		///		Сериализует значение интерфейсного типа, используя фактический тип объекта при записи.
		/// </summary>
		/// <param name="writer">JSON-писатель.</param>
		/// <param name="value">Значение интерфейсного типа для сериализации.</param>
		/// <param name="options">Опции сериализации.</param>
		public override void Write(
			Utf8JsonWriter writer,
			TInterface value,
			JsonSerializerOptions options)
		{
			JsonSerializer.Serialize(writer, value, value.GetType(), options);
		}
	}
}
