using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Comparer.Tests
{
	internal static class TestHelpers
	{
		public static PropertyDifference Find(IEnumerable<PropertyDifference> differences, string propertyName)
		{
			return Assert.Single(differences, x => x.PropertyName == propertyName);
		}

		public static void AssertPropertyNames(IEnumerable<PropertyDifference> differences, params string[] expected)
		{
			Assert.Equal(
				expected.OrderBy(x => x, StringComparer.Ordinal),
				differences.Select(x => x.PropertyName).OrderBy(x => x, StringComparer.Ordinal));
		}

		public static SimpleEntity CreateSimple()
		{
			return new SimpleEntity
			{
				Id = 1,
				Name = "Вода",
				Amount = 10.5m,
				Date = new DateTime(2026, 10, 19, 12, 0, 0),
				NullableCount = 3,
				IsActive = true,
				Status = TestStatus.New
			};
		}

		public static Address CreateAddress(string city = "Санкт-Петербург", string street = "Невский")
		{
			return new Address { City = city, Street = street };
		}

		public static DocumentLine CreateLine(int id, string name, decimal amount)
		{
			return new DocumentLine { Id = id, Name = name, Amount = amount };
		}

		public static ObjectComparerInstance<Address> AddressComparer()
		{
			return new ObjectPropertyComparer<Address>().DefaultInclude().Build();
		}

		public static ObjectComparerInstance<DocumentLine> LineComparer()
		{
			return new ObjectPropertyComparer<DocumentLine>().DefaultInclude().Build();
		}
	}

	/// <summary>
	/// Форматтер-заглушка: запоминает вызовы и оборачивает значения в угловые скобки.
	/// </summary>
	internal sealed class RecordingFormatter : IDiffFormatter
	{
		public List<(string Old, string New)> Calls { get; } = new List<(string Old, string New)>();

		public void SideBySideDiff(string oldValue, string newValue, out string oldDiff, out string newDiff)
		{
			Calls.Add((oldValue, newValue));
			oldDiff = "<" + oldValue + ">";
			newDiff = "<" + newValue + ">";
		}
	}
}
