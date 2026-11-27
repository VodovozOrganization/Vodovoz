using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrueMark.Contracts
{
	/// <summary>
	/// Товарная группа маркировки «Честный знак»,
	/// по которой осуществляется учёт кода маркировки.
	/// </summary>
	public enum TrueMarkProductGroup
	{
		/// <summary>
		/// Упакованная вода
		/// </summary>
		[Display(Name = "Вода", ShortName = "Вода")]
		Water,

		/// <summary>
		/// Корма для животных
		/// </summary>
		[Display(Name = "Корма для животных", ShortName = "Корма")]
		PetFood
	}

	/// <summary>
	/// Конвертер одиночного значения: пишет нижний регистр (water/petfood), читает без учёта регистра;
	/// неизвестную группу (табак, обувь и т.п.) превращает в <c>null</c>, а не роняет разбор ответа ЧЗ.
	/// </summary>
	public class TrueMarkProductGroupJsonConverter : JsonConverter<TrueMarkProductGroup?>
	{
		/// <inheritdoc/>
		public override TrueMarkProductGroup? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if(reader.TokenType == JsonTokenType.Null)
			{
				return null;
			}

			var value = reader.GetString();

			if(Enum.TryParse<TrueMarkProductGroup>(value, ignoreCase: true, out var result))
			{
				return result;
			}

			return null;
		}

		/// <inheritdoc/>
		public override void Write(Utf8JsonWriter writer, TrueMarkProductGroup? value, JsonSerializerOptions options)
		{
			if(value.HasValue)
			{
				writer.WriteStringValue(value.Value.ToApiValue());
			}
			else
			{
				writer.WriteNullValue();
			}
		}
	}

	/// <summary>
	/// Конвертер списка групп: читает ответ Честного Знака, пропуская неизвестные группы
	/// (табак, обувь, медизделия и т.п.) и дубли, пишет в нижнем регистре (water/petfood).
	/// </summary>
	public class TrueMarkProductGroupsJsonConverter : JsonConverter<IEnumerable<TrueMarkProductGroup>>
	{
		/// <inheritdoc/>
		public override IEnumerable<TrueMarkProductGroup> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if(reader.TokenType == JsonTokenType.Null)
			{
				return Array.Empty<TrueMarkProductGroup>();
			}

			var result = new List<TrueMarkProductGroup>();

			using(var document = JsonDocument.ParseValue(ref reader))
			{
				foreach(var element in document.RootElement.EnumerateArray())
				{
					var value = element.GetString();

					if(Enum.TryParse<TrueMarkProductGroup>(value, ignoreCase: true, out var productGroup)
						&& !result.Contains(productGroup))
					{
						result.Add(productGroup);
					}
				}
			}

			return result;
		}

		/// <inheritdoc/>
		public override void Write(Utf8JsonWriter writer, IEnumerable<TrueMarkProductGroup> value, JsonSerializerOptions options)
		{
			writer.WriteStartArray();

			if(value != null)
			{
				foreach(var productGroup in value)
				{
					writer.WriteStringValue(productGroup.ToApiValue());
				}
			}

			writer.WriteEndArray();
		}
	}
}
