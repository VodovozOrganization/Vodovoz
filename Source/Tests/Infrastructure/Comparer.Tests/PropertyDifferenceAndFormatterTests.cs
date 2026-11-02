using System.Collections.Generic;
using Xunit;
using static Comparer.Tests.TestHelpers;

namespace Comparer.Tests
{
	/// <summary>
	/// Поведение <see cref="PropertyDifference"/> и применение <see cref="IDiffFormatter"/>.
	/// </summary>
	public class PropertyDifferenceAndFormatterTests
	{
		#region PropertyDifference

		/// <summary>
		/// Конструктор заполняет свойства; отформатированные значения по умолчанию равны исходным.
		/// </summary>
		[Fact]
		public void Constructor_SetsPropertiesAndDefaultsFormattedValuesToRaw()
		{
			var difference = new PropertyDifference("Name", "Название", "a", "b");

			Assert.Equal("Name", difference.PropertyName);
			Assert.Equal("Название", difference.DisplayName);
			Assert.Equal("a", difference.FirstValue);
			Assert.Equal("b", difference.SecondValue);
			Assert.Equal("a", difference.FormattedFirstValue);
			Assert.Equal("b", difference.FormattedSecondValue);
			Assert.False(difference.IsCollectionItem);
			Assert.NotNull(difference.ChildDifferences);
			Assert.Empty(difference.ChildDifferences);
		}

		/// <summary>
		/// HasDiff без дочерних различий — ординальное сравнение значений.
		/// </summary>
		[Theory]
		[InlineData("a", "a", false)]
		[InlineData("a", "b", true)]
		[InlineData("a", "A", true)]
		[InlineData(null, null, false)]
		[InlineData(null, "", true)]
		[InlineData("", "", false)]
		public void HasDiff_WithoutChildren_ComparesValuesOrdinal(string first, string second, bool expected)
		{
			var difference = new PropertyDifference("P", "P", first, second);

			Assert.Equal(expected, difference.HasDiff);
		}

		/// <summary>
		/// При наличии дочерних различий собственные значения игнорируются.
		/// </summary>
		[Fact]
		public void HasDiff_WithChildrenWithoutDiff_IgnoresOwnValues()
		{
			var difference = new PropertyDifference(
				"P", "P", "x", "y",
				new List<PropertyDifference> { new PropertyDifference("C", "C", "1", "1") });

			Assert.False(difference.HasDiff);
		}

		/// <summary>
		/// Отличие у любого потомка (в том числе глубже одного уровня) даёт HasDiff = true.
		/// </summary>
		[Fact]
		public void HasDiff_DiffInGrandChild_PropagatesToRoot()
		{
			var grandChild = new PropertyDifference("G", "G", "1", "2");
			var child = new PropertyDifference("C", "C", "", "", new List<PropertyDifference> { grandChild });
			var root = new PropertyDifference(
				"R", "R", "", "",
				new List<PropertyDifference> { new PropertyDifference("S", "S", "1", "1"), child });

			Assert.True(root.HasDiff);
		}

		#endregion

		#region NullDiffFormatter

		/// <summary>
		/// NullDiffFormatter возвращает значения без изменений.
		/// </summary>
		[Theory]
		[InlineData("a", "b")]
		[InlineData(null, "b")]
		[InlineData(null, null)]
		public void NullDiffFormatter_ReturnsValuesUnchanged(string oldValue, string newValue)
		{
			NullDiffFormatter.Instance.SideBySideDiff(oldValue, newValue, out var oldDiff, out var newDiff);

			Assert.Equal(oldValue, oldDiff);
			Assert.Equal(newValue, newDiff);
		}

		/// <summary>
		/// NullDiffFormatter.Instance — один экземпляр.
		/// </summary>
		[Fact]
		public void NullDiffFormatter_Instance_IsSingleton()
		{
			Assert.Same(NullDiffFormatter.Instance, NullDiffFormatter.Instance);
		}

		#endregion

		#region Применение форматтера в Compare

		/// <summary>
		/// Без форматтера отформатированные значения совпадают с исходными.
		/// </summary>
		[Fact]
		public void Compare_WithoutFormatter_FormattedValuesEqualRaw()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>().DefaultInclude().Build();
			var second = CreateSimple();
			second.Name = "Кулер";

			var result = comparer.Compare(CreateSimple(), second);

			var name = Find(result, "Name");
			Assert.Equal(name.FirstValue, name.FormattedFirstValue);
			Assert.Equal(name.SecondValue, name.FormattedSecondValue);
		}

		/// <summary>
		/// Форматтер применяется к каждому простому свойству, в том числе без отличий;
		/// исходные значения не меняются.
		/// </summary>
		[Fact]
		public void Compare_WithFormatter_AppliedToEverySimpleProperty()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>().DefaultInclude().Build();
			var formatter = new RecordingFormatter();
			var second = CreateSimple();
			second.Name = "Кулер";

			var result = comparer.Compare(CreateSimple(), second, formatter);

			Assert.Equal(7, formatter.Calls.Count);
			var name = Find(result, "Name");
			Assert.Equal("Вода", name.FirstValue);
			Assert.Equal("<Вода>", name.FormattedFirstValue);
			Assert.Equal("<Кулер>", name.FormattedSecondValue);
		}

		/// <summary>
		/// Форматтер получает null, если значение свойства null.
		/// </summary>
		[Fact]
		public void Compare_WithFormatter_NullValuePassedAsNull()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>().DefaultExclude().Include(x => x.Name).Build();
			var formatter = new RecordingFormatter();
			var first = CreateSimple();
			first.Name = null;

			comparer.Compare(first, CreateSimple(), formatter);

			var call = Assert.Single(formatter.Calls);
			Assert.Null(call.Old);
			Assert.Equal("Вода", call.New);
		}

		/// <summary>
		/// Форматтер передаётся во вложенные компараторы и применяется к дочерним свойствам,
		/// а к записи самого вложенного объекта не применяется.
		/// </summary>
		[Fact]
		public void Compare_WithFormatter_AppliedToNestedChildrenButNotToParentEntry()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultExclude()
				.Include(x => x.Address)
				.CompareObject(x => x.Address, AddressComparer())
				.Build();
			var formatter = new RecordingFormatter();

			var result = comparer.Compare(
				new Document { Address = CreateAddress("Москва") },
				new Document { Address = CreateAddress("Казань") },
				formatter);

			var address = Find(result, "Address");
			Assert.Equal(string.Empty, address.FormattedFirstValue);
			Assert.Equal(string.Empty, address.FormattedSecondValue);
			var city = Find(address.ChildDifferences, "City");
			Assert.Equal("<Москва>", city.FormattedFirstValue);
			Assert.Equal("<Казань>", city.FormattedSecondValue);
			Assert.Equal(2, formatter.Calls.Count);
		}

		/// <summary>
		/// Форматтер передаётся в компаратор элементов коллекции.
		/// </summary>
		[Fact]
		public void Compare_WithFormatter_AppliedToCollectionItems()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.Lines, LineComparer())
				.Build();
			var formatter = new RecordingFormatter();

			var result = comparer.Compare(
				new Document { Lines = new List<DocumentLine> { CreateLine(1, "Вода", 100) } },
				new Document { Lines = new List<DocumentLine> { CreateLine(1, "Кулер", 100) } },
				formatter);

			var item = Find(result, "Lines").ChildDifferences[0];
			Assert.Equal("<Кулер>", Find(item.ChildDifferences, "Name").FormattedSecondValue);
		}

		#endregion
	}
}
