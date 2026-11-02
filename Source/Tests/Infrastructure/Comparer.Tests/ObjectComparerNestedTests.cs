using System.Collections.Generic;
using Xunit;
using static Comparer.Tests.TestHelpers;

namespace Comparer.Tests
{
	/// <summary>
	/// Сравнение вложенных объектов и коллекций через CompareObject.
	/// </summary>
	public class ObjectComparerNestedTests
	{
		#region Вложенные объекты

		/// <summary>
		/// Вложенный объект даёт запись с дочерними различиями по его свойствам.
		/// </summary>
		[Fact]
		public void Compare_NestedObject_ProducesEntryWithChildDifferences()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.Address, AddressComparer())
				.Build();

			var result = comparer.Compare(
				new Document { Address = CreateAddress() },
				new Document { Address = CreateAddress() });

			AssertPropertyNames(result, "Id", "Number", "Address");
			var address = Find(result, "Address");
			Assert.Equal("Адрес доставки", address.DisplayName);
			Assert.False(address.IsCollectionItem);
			Assert.Equal(string.Empty, address.FirstValue);
			Assert.Equal(string.Empty, address.SecondValue);
			AssertPropertyNames(address.ChildDifferences, "City", "Street");
			Assert.Equal("Город", Find(address.ChildDifferences, "City").DisplayName);
		}

		/// <summary>
		/// Одинаковые вложенные объекты — нет отличия.
		/// </summary>
		[Fact]
		public void Compare_EqualNestedObjects_NoDiff()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.Address, AddressComparer())
				.Build();

			var result = comparer.Compare(
				new Document { Address = CreateAddress() },
				new Document { Address = CreateAddress() });

			Assert.False(Find(result, "Address").HasDiff);
		}

		/// <summary>
		/// Изменение свойства вложенного объекта поднимает HasDiff у родительской записи.
		/// </summary>
		[Fact]
		public void Compare_ChangedNestedProperty_ParentHasDiff()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.Address, AddressComparer())
				.Build();

			var result = comparer.Compare(
				new Document { Address = CreateAddress("Санкт-Петербург") },
				new Document { Address = CreateAddress("Москва") });

			var address = Find(result, "Address");
			Assert.True(address.HasDiff);
			var city = Find(address.ChildDifferences, "City");
			Assert.True(city.HasDiff);
			Assert.Equal("Санкт-Петербург", city.FirstValue);
			Assert.Equal("Москва", city.SecondValue);
			Assert.False(Find(address.ChildDifferences, "Street").HasDiff);
		}

		/// <summary>
		/// Настройки вложенного компаратора (Exclude) применяются к дочерним различиям.
		/// </summary>
		[Fact]
		public void Compare_NestedComparerConfiguration_IsApplied()
		{
			var withoutCity = new ObjectPropertyComparer<Address>().DefaultInclude().Exclude(x => x.City).Build();
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.Address, withoutCity)
				.Build();

			var result = comparer.Compare(
				new Document { Address = CreateAddress("Санкт-Петербург") },
				new Document { Address = CreateAddress("Москва") });

			var address = Find(result, "Address");
			AssertPropertyNames(address.ChildDifferences, "Street");
			Assert.False(address.HasDiff);
		}

		/// <summary>
		/// Один компаратор можно использовать для нескольких свойств.
		/// </summary>
		[Fact]
		public void Compare_SameNestedComparerForSeveralProperties_Works()
		{
			var addressComparer = AddressComparer();
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.Address, addressComparer)
				.CompareObject(x => x.BillingAddress, addressComparer)
				.Build();

			var result = comparer.Compare(
				new Document { Address = CreateAddress(), BillingAddress = CreateAddress("Москва") },
				new Document { Address = CreateAddress(), BillingAddress = CreateAddress("Казань") });

			Assert.False(Find(result, "Address").HasDiff);
			var billing = Find(result, "BillingAddress");
			Assert.True(billing.HasDiff);
			Assert.Equal("BillingAddress", billing.DisplayName);
		}

		/// <summary>
		/// Оба вложенных объекта null — запись есть, дочерних различий нет, отличия нет.
		/// </summary>
		[Fact]
		public void Compare_BothNestedObjectsNull_EntryWithoutChildrenAndDiff()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.Address, AddressComparer())
				.Build();

			var result = comparer.Compare(new Document(), new Document());

			var address = Find(result, "Address");
			Assert.Empty(address.ChildDifferences);
			Assert.False(address.HasDiff);
		}

		/// <summary>
		/// Один и тот же экземпляр вложенного объекта у обоих документов — дочерних различий нет.
		/// </summary>
		[Fact]
		public void Compare_SharedNestedInstance_NoChildrenAndNoDiff()
		{
			var shared = CreateAddress();
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.Address, AddressComparer())
				.Build();

			var result = comparer.Compare(new Document { Address = shared }, new Document { Address = shared });

			var address = Find(result, "Address");
			Assert.Empty(address.ChildDifferences);
			Assert.False(address.HasDiff);
		}

		/// <summary>
		/// Вложенный объект null с одной стороны — исключения нет.
		/// </summary>
		[Fact]
		public void Compare_NestedObjectNullOnOneSide_DoesNotThrow()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.Address, AddressComparer())
				.Build();

			var exception = Record.Exception(() => comparer.Compare(new Document(), new Document { Address = CreateAddress() }));

			Assert.Null(exception);
		}

		/// <summary>
		/// Вложенный объект был null, стал заполнен — есть отличие.
		/// </summary>
		[Fact]
		public void Compare_NestedObjectNullToValue_HasDiff()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.Address, AddressComparer())
				.Build();

			var result = comparer.Compare(
				new Document { Address = null },
				new Document { Address = CreateAddress() });

			var address = Find(result, "Address");
			Assert.True(address.HasDiff);
			Assert.Equal(string.Empty, address.FirstValue);
			Assert.Equal(string.Empty, address.SecondValue);
			AssertPropertyNames(address.ChildDifferences, "City", "Street");
			Assert.Equal("Санкт-Петербург", Find(address.ChildDifferences, "City").SecondValue);
		}

		/// <summary>
		/// Вложенный объект появился, но все его свойства пустые — отличие всё равно есть.
		/// </summary>
		[Fact]
		public void Compare_NestedObjectNullToEmptyObject_HasDiff()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.Address, AddressComparer())
				.Build();

			var result = comparer.Compare(
				new Document { Address = null },
				new Document { Address = new Address() });

			var address = Find(result, "Address");
			Assert.All(address.ChildDifferences, x => Assert.False(x.HasDiff));
			Assert.True(address.HasDiff);
		}

		/// <summary>
		/// Вложенный объект был заполнен, стал null — есть отличие.
		/// </summary>
		[Fact]
		public void Compare_NestedObjectValueToNull_HasDiff()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.Address, AddressComparer())
				.Build();

			var result = comparer.Compare(
				new Document { Address = CreateAddress() },
				new Document { Address = null });

			Assert.True(Find(result, "Address").HasDiff);
		}

		/// <summary>
		/// В режиме DefaultExclude вложенный объект участвует, если свойство явно включено.
		/// </summary>
		[Fact]
		public void DefaultExclude_IncludeAndCompareObject_NestedObjectCompared()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultExclude()
				.Include(x => x.Address)
				.CompareObject(x => x.Address, AddressComparer())
				.Build();

			var result = comparer.Compare(
				new Document { Address = CreateAddress("Санкт-Петербург") },
				new Document { Address = CreateAddress("Москва") });

			AssertPropertyNames(result, "Address");
			Assert.True(Find(result, "Address").HasDiff);
		}

		/// <summary>
		/// В режиме DefaultExclude CompareObject сам включает свойство в сравнение.
		/// </summary>
		[Fact]
		public void DefaultExclude_CompareObjectWithoutInclude_NestedObjectCompared()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultExclude()
				.CompareObject(x => x.Address, AddressComparer())
				.Build();

			var result = comparer.Compare(
				new Document { Address = CreateAddress("Москва") },
				new Document { Address = CreateAddress("Казань") });

			Assert.True(Find(result, "Address").HasDiff);
		}

		/// <summary>
		/// В режиме DefaultInclude Exclude побеждает CompareObject.
		/// </summary>
		[Fact]
		public void DefaultInclude_ExcludeAndCompareObject_PropertyExcluded()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.Address, AddressComparer())
				.Exclude(x => x.Address)
				.Build();

			var result = comparer.Compare(new Document { Address = CreateAddress() }, new Document { Address = CreateAddress() });

			Assert.DoesNotContain(result, x => x.PropertyName == "Address");
		}

		/// <summary>
		/// Многоуровневая вложенность: отличие на втором уровне поднимается до корня.
		/// </summary>
		[Fact]
		public void Compare_TwoLevelNesting_DiffPropagatesToRoot()
		{
			var documentComparer = new ObjectPropertyComparer<Document>()
				.DefaultExclude()
				.Include(x => x.Number)
				.Include(x => x.Address)
				.CompareObject(x => x.Address, AddressComparer())
				.Build();
			var holderComparer = new ObjectPropertyComparer<NestedHolder>()
				.DefaultInclude()
				.CompareObject(x => x.Document, documentComparer)
				.Build();

			var result = holderComparer.Compare(
				new NestedHolder { Document = new Document { Number = "1", Address = CreateAddress("Москва") } },
				new NestedHolder { Document = new Document { Number = "1", Address = CreateAddress("Казань") } });

			var document = Find(result, "Document");
			Assert.True(document.HasDiff);
			Assert.False(Find(document.ChildDifferences, "Number").HasDiff);
			var address = Find(document.ChildDifferences, "Address");
			Assert.True(address.HasDiff);
			Assert.True(Find(address.ChildDifferences, "City").HasDiff);
		}

		public class NestedHolder
		{
			public Document Document { get; set; }
		}

		#endregion

		#region Коллекции

		private static ObjectComparerInstance<Document> LinesComparer()
		{
			return new ObjectPropertyComparer<Document>()
				.DefaultExclude()
				.Include(x => x.Lines)
				.CompareObject(x => x.Lines, LineComparer())
				.Build();
		}

		/// <summary>
		/// Коллекция даёт запись с элементом на каждый индекс; имя элемента — из [Appellative] и индекса.
		/// </summary>
		[Fact]
		public void Compare_Collection_ProducesItemPerIndex()
		{
			var result = LinesComparer().Compare(
				new Document { Lines = new List<DocumentLine> { CreateLine(1, "Вода", 100), CreateLine(2, "Кулер", 5000) } },
				new Document { Lines = new List<DocumentLine> { CreateLine(1, "Вода", 100), CreateLine(2, "Кулер", 5000) } });

			var lines = Find(result, "Lines");
			Assert.Equal("Строки", lines.DisplayName);
			Assert.False(lines.HasDiff);
			Assert.Equal(2, lines.ChildDifferences.Count);

			var item = lines.ChildDifferences[1];
			Assert.Equal("1", item.PropertyName);
			Assert.Equal("Строка документа [1]", item.DisplayName);
			Assert.True(item.IsCollectionItem);
			AssertPropertyNames(item.ChildDifferences, "Id", "Name", "Amount");
		}

		/// <summary>
		/// Изменённое поле элемента коллекции поднимает HasDiff у элемента и коллекции.
		/// </summary>
		[Fact]
		public void Compare_ChangedCollectionItem_ItemAndCollectionHaveDiff()
		{
			var result = LinesComparer().Compare(
				new Document { Lines = new List<DocumentLine> { CreateLine(1, "Вода", 100), CreateLine(2, "Кулер", 5000) } },
				new Document { Lines = new List<DocumentLine> { CreateLine(1, "Вода", 100), CreateLine(2, "Кулер", 4500) } });

			var lines = Find(result, "Lines");
			Assert.True(lines.HasDiff);
			Assert.False(lines.ChildDifferences[0].HasDiff);
			Assert.True(lines.ChildDifferences[1].HasDiff);
			Assert.True(Find(lines.ChildDifferences[1].ChildDifferences, "Amount").HasDiff);
		}

		/// <summary>
		/// Без [Appellative] имя элемента — имя типа и индекс.
		/// </summary>
		[Fact]
		public void Compare_CollectionItemWithoutAppellative_DisplayNameIsTypeName()
		{
			var plainComparer = new ObjectPropertyComparer<PlainLine>().DefaultInclude().Build();
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.PlainLines, plainComparer)
				.Build();

			var result = comparer.Compare(
				new Document { PlainLines = new List<PlainLine> { new PlainLine { Name = "a" } } },
				new Document { PlainLines = new List<PlainLine> { new PlainLine { Name = "a" } } });

			Assert.Equal("PlainLine [0]", Find(result, "PlainLines").ChildDifferences[0].DisplayName);
		}

		/// <summary>
		/// Массив тоже сравнивается как коллекция.
		/// </summary>
		[Fact]
		public void Compare_ArrayProperty_ComparedAsCollection()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.LineArray, LineComparer())
				.Build();

			var result = comparer.Compare(
				new Document { LineArray = new[] { CreateLine(1, "Вода", 100) } },
				new Document { LineArray = new[] { CreateLine(1, "Вода", 200) } });

			var array = Find(result, "LineArray");
			Assert.Single(array.ChildDifferences);
			Assert.True(array.HasDiff);
		}

		/// <summary>
		/// Обе коллекции null — запись без элементов, отличия нет.
		/// </summary>
		[Fact]
		public void Compare_BothCollectionsNull_NoItemsAndNoDiff()
		{
			var result = LinesComparer().Compare(new Document { Lines = null }, new Document { Lines = null });

			var lines = Find(result, "Lines");
			Assert.Empty(lines.ChildDifferences);
			Assert.False(lines.HasDiff);
		}

		/// <summary>
		/// null-коллекция и пустая коллекция считаются одинаковыми.
		/// </summary>
		[Fact]
		public void Compare_NullVersusEmptyCollection_NoDiff()
		{
			var result = LinesComparer().Compare(new Document { Lines = null }, new Document { Lines = new List<DocumentLine>() });

			Assert.False(Find(result, "Lines").HasDiff);
		}

		/// <summary>
		/// null-коллекция против непустой коллекции даёт отличие.
		/// </summary>
		[Fact]
		public void Compare_NullCollectionVersusNonEmpty_HasDiff()
		{
			var result = LinesComparer().Compare(
				new Document { Lines = null },
				new Document { Lines = new List<DocumentLine> { CreateLine(1, "Вода", 100) } });

			Assert.True(Find(result, "Lines").HasDiff);
		}

		/// <summary>
		/// Если на одном индексе в обеих коллекциях null, элемент пропускается.
		/// </summary>
		[Fact]
		public void Compare_NullItemsAtSameIndex_AreSkipped()
		{
			var result = LinesComparer().Compare(
				new Document { Lines = new List<DocumentLine> { null, CreateLine(1, "Вода", 100) } },
				new Document { Lines = new List<DocumentLine> { null, CreateLine(1, "Вода", 100) } });

			var lines = Find(result, "Lines");
			var item = Assert.Single(lines.ChildDifferences);
			Assert.Equal("1", item.PropertyName);
		}

		/// <summary>
		/// При разной длине коллекций создаётся элемент для каждого индекса более длинной коллекции.
		/// </summary>
		[Fact]
		public void Compare_CollectionsOfDifferentLength_ItemCreatedForEachIndex()
		{
			var result = LinesComparer().Compare(
				new Document { Lines = new List<DocumentLine> { CreateLine(1, "Вода", 100), CreateLine(2, "Кулер", 5000) } },
				new Document { Lines = new List<DocumentLine> { CreateLine(1, "Вода", 100) } });

			var lines = Find(result, "Lines");
			Assert.Equal(2, lines.ChildDifferences.Count);
			var extra = lines.ChildDifferences[1];
			Assert.True(extra.IsCollectionItem);
			AssertPropertyNames(extra.ChildDifferences, "Id", "Name", "Amount");
		}

		/// <summary>
		/// У добавленного элемента коллекции нет собственного текстового значения,
		/// но выводятся все его свойства, и каждое помечено как отличие.
		/// </summary>
		[Fact]
		public void Compare_CollectionItemAdded_ItemPropertiesShownWithoutOwnValue()
		{
			var result = LinesComparer().Compare(
				new Document { Lines = new List<DocumentLine>() },
				new Document { Lines = new List<DocumentLine> { CreateLine(1, "Вода", 100) } });

			var item = Assert.Single(Find(result, "Lines").ChildDifferences);
			Assert.Equal(string.Empty, item.FirstValue);
			Assert.Equal(string.Empty, item.SecondValue);
			Assert.True(item.HasDiff);
			var name = Find(item.ChildDifferences, "Name");
			Assert.Null(name.FirstValue);
			Assert.Equal("Вода", name.SecondValue);
			Assert.All(item.ChildDifferences, x => Assert.True(x.HasDiff));
		}

		/// <summary>
		/// Отличие глубоко в иерархии поднимается через записи без текстовых значений:
		/// элемент коллекции → вложенный объект → свойство.
		/// </summary>
		[Fact]
		public void Compare_DiffInsideCollectionItemNestedObject_WholeHierarchyHasDiff()
		{
			var documentComparer = new ObjectPropertyComparer<Document>()
				.DefaultExclude()
				.CompareObject(x => x.Address, AddressComparer())
				.Build();
			var holderComparer = new ObjectPropertyComparer<DocumentsHolder>()
				.DefaultExclude()
				.CompareObject(x => x.Documents, documentComparer)
				.Build();

			var result = holderComparer.Compare(
				new DocumentsHolder { Documents = new List<Document> { new Document { Address = CreateAddress("Москва") } } },
				new DocumentsHolder { Documents = new List<Document> { new Document { Address = CreateAddress("Казань") } } });

			var documents = Find(result, "Documents");
			var document = Assert.Single(documents.ChildDifferences);
			var address = Find(document.ChildDifferences, "Address");
			var city = Find(address.ChildDifferences, "City");

			Assert.True(documents.HasDiff);
			Assert.True(document.HasDiff);
			Assert.True(address.HasDiff);
			Assert.True(city.HasDiff);
			Assert.Equal(string.Empty, documents.FirstValue);
			Assert.Equal(string.Empty, document.FirstValue);
			Assert.Equal(string.Empty, address.FirstValue);
		}

		public class DocumentsHolder
		{
			public List<Document> Documents { get; set; }
		}

		/// <summary>
		/// Удалённый элемент коллекции даёт отличие у элемента и коллекции.
		/// </summary>
		[Fact]
		public void Compare_CollectionItemRemoved_HasDiff()
		{
			var result = LinesComparer().Compare(
				new Document { Lines = new List<DocumentLine> { CreateLine(1, "Вода", 100), CreateLine(2, "Кулер", 5000) } },
				new Document { Lines = new List<DocumentLine> { CreateLine(1, "Вода", 100) } });

			var lines = Find(result, "Lines");
			Assert.True(lines.ChildDifferences[1].HasDiff);
			Assert.True(lines.HasDiff);
		}

		/// <summary>
		/// Добавленный элемент коллекции даёт отличие.
		/// </summary>
		[Fact]
		public void Compare_CollectionItemAdded_HasDiff()
		{
			var result = LinesComparer().Compare(
				new Document { Lines = new List<DocumentLine>() },
				new Document { Lines = new List<DocumentLine> { CreateLine(1, "Вода", 100) } });

			Assert.True(Find(result, "Lines").HasDiff);
		}

		/// <summary>
		/// Элементы сравниваются по индексу: перестановка тех же элементов считается отличием.
		/// </summary>
		[Fact]
		public void Compare_ReorderedCollection_ReportedAsDiffBecauseComparedByIndex()
		{
			var result = LinesComparer().Compare(
				new Document { Lines = new List<DocumentLine> { CreateLine(1, "Вода", 100), CreateLine(2, "Кулер", 5000) } },
				new Document { Lines = new List<DocumentLine> { CreateLine(2, "Кулер", 5000), CreateLine(1, "Вода", 100) } });

			Assert.True(Find(result, "Lines").HasDiff);
		}

		#endregion
	}
}
