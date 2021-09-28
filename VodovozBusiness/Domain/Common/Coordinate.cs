using System;
using System.Globalization;

namespace Vodovoz.Domain.Common
{
	/// <summary>
	/// Определяет географические координаты, с точностью до 6 десятичных знаков.
	/// Пример: 50.123456,-60.123456
	/// </summary>
	public struct Coordinate
	{
		public decimal Latitude { get; }
		public decimal Longitude { get; }

		public Coordinate(decimal latitude, decimal longitude)
		{
			Latitude = Math.Round(latitude, 6);
			Longitude = Math.Round(longitude, 6);
		}

		public override string ToString()
		{
			return $"{Latitude:0:000000},{Longitude:0:000000}";
		}

		public override bool Equals(object obj)
		{
			return obj is Coordinate coordinate &&
				   Latitude == coordinate.Latitude &&
				   Longitude == coordinate.Longitude;
		}

		public override int GetHashCode()
		{
			int hashCode = -1416534245;
			hashCode = hashCode * -1521134295 + Latitude.GetHashCode();
			hashCode = hashCode * -1521134295 + Longitude.GetHashCode();
			return hashCode;
		}

		#region Parsing

		public static Coordinate Parse(string coordinate)
		{
			if(string.IsNullOrWhiteSpace(coordinate))
			{
				throw new InvalidOperationException($"Не получится распознать координаты из пустой строки.");
			}

			coordinate = coordinate.Replace(" ", "");

			string latitudeStr = "";
			string longitudeStr = "";

			var coordinates = SplitCoordinate(coordinate);
			if(coordinates.Length == 4)
			{
				latitudeStr = $"{coordinates[0]}.{coordinates[1]}";
				longitudeStr = $"{coordinates[2]}.{coordinates[3]}";
			}
			else if(coordinates.Length == 2)
			{
				latitudeStr = coordinates[0];
				longitudeStr = coordinates[1];
			}
			else
			{
				throw new InvalidOperationException($"Не удалось распознать координаты из строки \"{coordinate}\"");
			}

			latitudeStr = latitudeStr.Replace(",", ".");
			longitudeStr = longitudeStr.Replace(",", ".");

			bool latitudeParsed = decimal.TryParse(latitudeStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal latitude);
			if(!latitudeParsed)
			{
				throw new InvalidOperationException($"Не удалось распознать широту из строки координат. Широта: \"{latitudeStr}\", Координаты: \"{coordinate}\"");
			}

			bool longitudeParsed = decimal.TryParse(longitudeStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal longitude);
			if(!longitudeParsed)
			{
				throw new InvalidOperationException($"Не удалось распознать долготу из строки координат. Долгота: \"{longitudeStr}\", Координаты: \"{coordinate}\"");
			}

			if(latitude > 90M || latitude < -90M)
			{
				throw new InvalidOperationException($"Широта должна быть не более 90 и не менее -90. Широта: \"{latitude}\"");
			}

			if(longitude > 180M || longitude < -180M)
			{
				throw new InvalidOperationException($"Долгота должна быть не более 180 и не менее -180. Долгота: \"{longitude}\"");
			}

			return new Coordinate(latitude, longitude);
		}

		private static string[] SplitCoordinate(string coordinate)
		{
			//Запятую проверять в самом конце
			string separators = ";:,";
			string[] result;

			foreach(var separator in separators)
			{
				result = coordinate.Split(separator);
				if(result.Length > 1)
				{
					return result;
				}
			}
			throw new InvalidOperationException($"Невозможно распознать широту и долготу в строке координат. Координаты: \"{coordinate}\"");
		}

		public static bool TryParse(string coordinate, out Coordinate result)
		{
			try
			{
				result = Parse(coordinate);
				return true;
			}
			catch(InvalidOperationException)
			{
				result = new Coordinate();
				return false;
			}
		}

		#endregion Parsing
	}
}
