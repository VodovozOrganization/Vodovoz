using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Нормализация адресных данных
	/// </summary>
	public static class AddressNormalizer
	{
		private static readonly string[] _streetPrefixes =
		{
			"улица", "ул", "проспект", "пр-т", "пр", "переулок", "пер", "бульвар", "б-р",
			"шоссе", "ш", "набережная", "наб", "площадь", "пл", "микрорайон", "мкр",
			"аллея", "линия", "лн"
		};

		/// <summary>
		/// Нормализует строку для сравнения
		/// </summary>
		public static string NormalizeForComparison(string value)
		{
			if(string.IsNullOrWhiteSpace(value))
			{
				return string.Empty;
			}

			var normalized = value.Trim().ToLowerInvariant().Replace('ё', 'е');
			normalized = Regex.Replace(normalized, @"[.,;:""'«»()]+", " ");
			normalized = Regex.Replace(normalized, @"[%_]+", " ");
			normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

			return normalized;
		}

		/// <summary>
		/// Нормализует название улицы, убирая тип улицы
		/// </summary>
		public static string NormalizeStreet(string value)
		{
			var street = NormalizeForComparison(value);
			var changed = true;

			while(changed)
			{
				changed = false;

				foreach(var prefix in _streetPrefixes)
				{
					if(street.StartsWith(prefix + " ", StringComparison.Ordinal))
					{
						street = street.Substring(prefix.Length + 1).Trim();
						changed = true;
						break;
					}
				}
			}

			return street;
		}

		/// <summary>
		/// Нормализует название города
		/// </summary>
		public static string NormalizeCity(string value)
		{
			var city = NormalizeForComparison(value);
			city = Regex.Replace(city, @"^(город|г)\s+", string.Empty);

			return city;
		}

		/// <summary>
		/// Убирает из значения всё, кроме цифр, букв, дефиса и слеша
		/// </summary>
		public static string NormalizePart(string value)
		{
			if(string.IsNullOrWhiteSpace(value))
			{
				return null;
			}

			var normalized = value.Trim().ToLowerInvariant().Replace('ё', 'е');
			normalized = Regex.Replace(normalized, @"[^0-9a-zа-я/\-]+", string.Empty);

			return string.IsNullOrWhiteSpace(normalized)
				? null
				: normalized;
		}

		/// <summary>
		/// Нормализует номер помещения
		/// </summary>
		public static string NormalizeRoom(string value)
		{
			var room = NormalizeForComparison(value);
			room = Regex.Replace(
				room,
				@"^(офис|оф|квартира|кв|помещение|пом|комната|ком|склад|секция|сек)\s*",
				string.Empty);

			return NormalizePart(room);
		}

		/// <summary>
		/// Определяет тип помещения по строке
		/// </summary>
		public static RoomType? ParseRoomType(string value)
		{
			var room = NormalizeForComparison(value);

			if(Regex.IsMatch(room, @"(^|\s)(офис|оф)(\s|$)"))
			{
				return RoomType.Office;
			}

			if(Regex.IsMatch(room, @"(^|\s)(квартира|кв)(\s|$)"))
			{
				return RoomType.Apartment;
			}

			if(Regex.IsMatch(room, @"(^|\s)(помещение|пом)(\s|$)"))
			{
				return RoomType.Room;
			}

			if(Regex.IsMatch(room, @"(^|\s)(комната|ком)(\s|$)"))
			{
				return RoomType.Chamber;
			}

			if(Regex.IsMatch(room, @"(^|\s)склад(\s|$)"))
			{
				return RoomType.Store;
			}

			if(Regex.IsMatch(room, @"(^|\s)(секция|сек)(\s|$)"))
			{
				return RoomType.Section;
			}

			return null;
		}

		/// <summary>
		/// Собирает адрес из составных частей
		/// </summary>
		public static string BuildAddress(
			string compiledAddress,
			string city,
			string streetType,
			string street,
			string building,
			string letter,
			string room,
			RoomType roomType,
			string placement)
		{
			var addressParts = new List<string>();

			if(!string.IsNullOrWhiteSpace(city))
			{
				addressParts.Add(city);
			}

			var streetPart = string.Join(
				" ",
				new[] { streetType, street }.Where(x => !string.IsNullOrWhiteSpace(x)));

			if(!string.IsNullOrWhiteSpace(streetPart))
			{
				addressParts.Add(streetPart);
			}

			if(!string.IsNullOrWhiteSpace(building))
			{
				addressParts.Add($"д.{building}");
			}

			if(!string.IsNullOrWhiteSpace(letter))
			{
				addressParts.Add($"лит.{letter}");
			}

			if(!string.IsNullOrWhiteSpace(room))
			{
				var roomPart = $"{GetRoomTypeShortName(roomType)} {room}".Trim();

				if(!string.IsNullOrWhiteSpace(roomPart))
				{
					addressParts.Add(roomPart);
				}
			}

			if(!string.IsNullOrWhiteSpace(placement))
			{
				addressParts.Add(placement);
			}

			return addressParts.Count > 0
				? string.Join(", ", addressParts)
				: compiledAddress ?? string.Empty;
		}

		private static string GetRoomTypeShortName(RoomType roomType)
		{
			switch(roomType)
			{
				case RoomType.Apartment:
					return "кв.";
				case RoomType.Office:
					return "оф.";
				case RoomType.Store:
					return "склад";
				case RoomType.Room:
					return "пом.";
				case RoomType.Chamber:
					return "ком.";
				case RoomType.Section:
					return "сек.";
				default:
					return string.Empty;
			}
		}
	}
}
