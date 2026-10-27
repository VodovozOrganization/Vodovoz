using System;
using System.Linq;
using Xunit;
using static Comparer.Tests.TestHelpers;

namespace Comparer.Tests
{
	/// <summary>
	/// Сравнение простых свойств: результат, преобразование значений в строки, null-значения.
	/// </summary>
	public class ObjectComparerCompareTests
	{
		private readonly ObjectComparerInstance<SimpleEntity> _comparer =
			new ObjectPropertyComparer<SimpleEntity>().DefaultInclude().Build();

		#region Результат сравнения

		/// <summary>
		/// Для одинаковых объектов возвращаются все сравниваемые свойства, ни одно не отличается.
		/// </summary>
		[Fact]
		public void Compare_EqualObjects_ReturnsAllPropertiesWithoutDiff()
		{
			var result = _comparer.Compare(CreateSimple(), CreateSimple());

			Assert.Equal(7, result.Count);
			Assert.All(result, x => Assert.False(x.HasDiff));
		}

		/// <summary>
		/// Отличие помечается только у изменённого свойства.
		/// </summary>
		[Fact]
		public void Compare_OneChangedProperty_OnlyThisPropertyHasDiff()
		{
			var first = CreateSimple();
			var second = CreateSimple();
			second.Name = "Кулер";

			var result = _comparer.Compare(first, second);

			var name = Find(result, "Name");
			Assert.True(name.HasDiff);
			Assert.Equal("Вода", name.FirstValue);
			Assert.Equal("Кулер", name.SecondValue);
			Assert.Single(result, x => x.HasDiff);
		}

		/// <summary>
		/// Простые свойства: PropertyName — имя свойства, IsCollectionItem = false, дочерних различий нет.
		/// </summary>
		[Fact]
		public void Compare_SimpleProperty_HasExpectedShape()
		{
			var result = _comparer.Compare(CreateSimple(), CreateSimple());

			var id = Find(result, "Id");
			Assert.False(id.IsCollectionItem);
			Assert.Empty(id.ChildDifferences);
		}

		/// <summary>
		/// DisplayName берётся из [Display(Name)], без атрибута — имя свойства.
		/// </summary>
		[Fact]
		public void Compare_DisplayName_TakenFromDisplayAttributeOrPropertyName()
		{
			var result = _comparer.Compare(CreateSimple(), CreateSimple());

			Assert.Equal("Название", Find(result, "Name").DisplayName);
			Assert.Equal("Id", Find(result, "Id").DisplayName);
		}

		/// <summary>
		/// Сравнение строк регистрозависимое.
		/// </summary>
		[Fact]
		public void Compare_StringsDifferingOnlyByCase_HasDiff()
		{
			var first = CreateSimple();
			var second = CreateSimple();
			second.Name = "ВОДА";

			var result = _comparer.Compare(first, second);

			Assert.True(Find(result, "Name").HasDiff);
		}

		#endregion

		#region Преобразование значений в строки

		/// <summary>
		/// bool выводится как «Да»/«Нет».
		/// </summary>
		[Fact]
		public void Compare_Bool_ConvertedToYesNo()
		{
			var first = CreateSimple();
			var second = CreateSimple();
			first.IsActive = true;
			second.IsActive = false;

			var result = _comparer.Compare(first, second);

			var isActive = Find(result, "IsActive");
			Assert.Equal("Да", isActive.FirstValue);
			Assert.Equal("Нет", isActive.SecondValue);
			Assert.True(isActive.HasDiff);
		}

		/// <summary>
		/// enum выводится по [Display(Name)].
		/// </summary>
		[Fact]
		public void Compare_EnumWithDisplay_ConvertedToDisplayName()
		{
			var first = CreateSimple();
			var second = CreateSimple();
			first.Status = TestStatus.New;
			second.Status = TestStatus.Closed;

			var result = _comparer.Compare(first, second);

			var status = Find(result, "Status");
			Assert.Equal("Новый", status.FirstValue);
			Assert.Equal("Закрыт", status.SecondValue);
		}

		/// <summary>
		/// enum без [Display] выводится именем значения.
		/// </summary>
		[Fact]
		public void Compare_EnumWithoutDisplay_ConvertedToMemberName()
		{
			var first = CreateSimple();
			first.Status = TestStatus.WithoutDisplay;

			var result = _comparer.Compare(first, CreateSimple());

			Assert.Equal("WithoutDisplay", Find(result, "Status").FirstValue);
		}

		/// <summary>
		/// Одиночный флаг [Flags]-enum выводится по [Display(Name)].
		/// </summary>
		[Fact]
		public void Compare_SingleFlag_ConvertedToDisplayName()
		{
			var comparer = new ObjectPropertyComparer<FlagsEntity>().DefaultInclude().Build();

			var result = comparer.Compare(
				new FlagsEntity { Flags = TestFlags.First },
				new FlagsEntity { Flags = TestFlags.None });

			var flags = Find(result, "Flags");
			Assert.Equal("Первый", flags.FirstValue);
			Assert.Equal("None", flags.SecondValue);
		}

		/// <summary>
		/// Числа и даты выводятся через ToString() текущей культуры.
		/// </summary>
		[Fact]
		public void Compare_NumbersAndDates_ConvertedWithToString()
		{
			var entity = CreateSimple();

			var result = _comparer.Compare(entity, CreateSimple());

			Assert.Equal(entity.Id.ToString(), Find(result, "Id").FirstValue);
			Assert.Equal(entity.Amount.ToString(), Find(result, "Amount").FirstValue);
			Assert.Equal(entity.Date.ToString(), Find(result, "Date").FirstValue);
			Assert.Equal(entity.NullableCount.ToString(), Find(result, "NullableCount").FirstValue);
		}

		/// <summary>
		/// 1.5m и 1.50m — одно и то же число, отличия нет.
		/// </summary>
		[Fact]
		public void Compare_DecimalsWithDifferentScale_NoDiff()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>().DefaultExclude().Include(x => x.Amount).Build();
			var first = CreateSimple();
			var second = CreateSimple();
			first.Amount = 1.5m;
			second.Amount = 1.50m;

			var result = comparer.Compare(first, second);

			Assert.False(Find(result, "Amount").HasDiff);
		}

		/// <summary>
		/// Даты, отличающиеся миллисекундами, — разные значения.
		/// </summary>
		[Fact]
		public void Compare_DatesDifferingByMilliseconds_HasDiff()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>().DefaultExclude().Include(x => x.Date).Build();
			var first = CreateSimple();
			var second = CreateSimple();
			first.Date = new DateTime(2026, 10, 19, 12, 0, 0, 0);
			second.Date = new DateTime(2026, 10, 19, 12, 0, 0, 500);

			var result = comparer.Compare(first, second);

			Assert.True(Find(result, "Date").HasDiff);
		}

		/// <summary>
		/// Комбинация флагов [Flags]-enum не приводит к исключению.
		/// </summary>
		[Fact]
		public void Compare_FlagsCombination_DoesNotThrow()
		{
			var comparer = new ObjectPropertyComparer<FlagsEntity>().DefaultInclude().Build();

			var exception = Record.Exception(() => comparer.Compare(
				new FlagsEntity { Flags = TestFlags.First | TestFlags.Second },
				new FlagsEntity { Flags = TestFlags.First }));

			Assert.Null(exception);
		}

		/// <summary>
		/// Комбинация флагов выводится названиями флагов из [Display(Name)] через запятую.
		/// </summary>
		[Fact]
		public void Compare_FlagsCombination_ConvertedToDisplayNames()
		{
			var comparer = new ObjectPropertyComparer<FlagsEntity>().DefaultInclude().Build();

			var result = comparer.Compare(
				new FlagsEntity { Flags = TestFlags.First | TestFlags.Second },
				new FlagsEntity { Flags = TestFlags.First });

			var flags = Find(result, "Flags");
			Assert.Equal("Первый, Второй", flags.FirstValue);
			Assert.True(flags.HasDiff);
		}

		/// <summary>
		/// Значение enum, не объявленное в типе, выводится числом.
		/// </summary>
		[Fact]
		public void Compare_UndefinedEnumValue_ConvertedToNumber()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>().DefaultExclude().Include(x => x.Status).Build();
			var first = CreateSimple();
			first.Status = (TestStatus)42;

			var result = comparer.Compare(first, CreateSimple());

			Assert.Equal("42", Find(result, "Status").FirstValue);
		}

		/// <summary>
		/// У даты с долями секунды они видны в значении, поэтому отличие по миллисекундам заметно.
		/// </summary>
		[Fact]
		public void Compare_DatesDifferingByMilliseconds_ValuesDiffer()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>().DefaultExclude().Include(x => x.Date).Build();
			var first = CreateSimple();
			var second = CreateSimple();
			second.Date = first.Date.AddMilliseconds(500);

			var date = Find(comparer.Compare(first, second), "Date");

			Assert.NotEqual(date.FirstValue, date.SecondValue);
		}

		/// <summary>
		/// Незначащие нули дробной части decimal не выводятся.
		/// </summary>
		[Fact]
		public void Compare_DecimalWithTrailingZeros_ConvertedWithoutThem()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>().DefaultExclude().Include(x => x.Amount).Build();
			var first = CreateSimple();
			var second = CreateSimple();
			first.Amount = 1.50m;
			second.Amount = 1.5m;

			var amount = Find(comparer.Compare(first, second), "Amount");

			Assert.Equal(1.5m.ToString(), amount.FirstValue);
			Assert.Equal(amount.SecondValue, amount.FirstValue);
		}

		/// <summary>
		/// Значение enum, не объявленное в типе, не приводит к исключению.
		/// </summary>
		[Fact]
		public void Compare_UndefinedEnumValue_DoesNotThrow()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>().DefaultExclude().Include(x => x.Status).Build();
			var first = CreateSimple();
			first.Status = (TestStatus)42;

			var exception = Record.Exception(() => comparer.Compare(first, CreateSimple()));

			Assert.Null(exception);
		}

		#endregion

		#region null-значения

		/// <summary>
		/// null-значение свойства превращается в null, отличие от заполненного значения фиксируется.
		/// </summary>
		[Fact]
		public void Compare_NullVersusValue_HasDiff()
		{
			var first = CreateSimple();
			var second = CreateSimple();
			first.Name = null;
			first.NullableCount = null;
			second.NullableCount = 5;

			var result = _comparer.Compare(first, second);

			var name = Find(result, "Name");
			Assert.Null(name.FirstValue);
			Assert.True(name.HasDiff);

			var count = Find(result, "NullableCount");
			Assert.Null(count.FirstValue);
			Assert.Equal("5", count.SecondValue);
			Assert.True(count.HasDiff);
		}

		/// <summary>
		/// null и null — нет отличия.
		/// </summary>
		[Fact]
		public void Compare_NullVersusNull_NoDiff()
		{
			var first = CreateSimple();
			var second = CreateSimple();
			first.Name = null;
			second.Name = null;

			var result = _comparer.Compare(first, second);

			Assert.False(Find(result, "Name").HasDiff);
		}

		/// <summary>
		/// null и пустая строка считаются разными значениями.
		/// </summary>
		[Fact]
		public void Compare_NullVersusEmptyString_HasDiff()
		{
			var first = CreateSimple();
			var second = CreateSimple();
			first.Name = null;
			second.Name = string.Empty;

			var result = _comparer.Compare(first, second);

			Assert.True(Find(result, "Name").HasDiff);
		}

		#endregion

		#region Аргументы Compare

		/// <summary>
		/// Один и тот же экземпляр — пустой результат, свойства не перечисляются.
		/// </summary>
		[Fact]
		public void Compare_SameInstance_ReturnsEmptyResult()
		{
			var entity = CreateSimple();

			var result = _comparer.Compare(entity, entity);

			Assert.Empty(result);
		}

		/// <summary>
		/// Типизированный Compare: первый аргумент null — ArgumentNullException("first").
		/// </summary>
		[Fact]
		public void Compare_FirstIsNull_ThrowsArgumentNullException()
		{
			var exception = Assert.Throws<ArgumentNullException>(() => _comparer.Compare(null, CreateSimple()));

			Assert.Equal("first", exception.ParamName);
		}

		/// <summary>
		/// Типизированный Compare: второй аргумент null — ArgumentNullException("second").
		/// </summary>
		[Fact]
		public void Compare_SecondIsNull_ThrowsArgumentNullException()
		{
			var exception = Assert.Throws<ArgumentNullException>(() => _comparer.Compare(CreateSimple(), null));

			Assert.Equal("second", exception.ParamName);
		}

		/// <summary>
		/// Типизированный Compare(null, null) не бросает исключение: проверка ReferenceEquals идёт раньше проверки на null.
		/// </summary>
		[Fact]
		public void Compare_BothNull_ReturnsEmptyResultWithoutException()
		{
			var result = _comparer.Compare(null, null);

			Assert.Empty(result);
		}

		#endregion

		#region IObjectComparerInstance

		/// <summary>
		/// Через интерфейс null-аргумент допустим: свойства второго объекта выводятся полностью,
		/// значения со стороны null равны null.
		/// </summary>
		[Fact]
		public void InterfaceCompare_NullArgument_ReturnsAllPropertiesWithNullOnThatSide()
		{
			IObjectComparerInstance comparer = _comparer;

			var removed = comparer.Compare(CreateSimple(), null);
			var added = comparer.Compare(null, CreateSimple());

			Assert.Equal(7, removed.Count);
			Assert.All(removed, x => Assert.Null(x.SecondValue));
			Assert.Equal("Вода", Find(removed, "Name").FirstValue);
			Assert.Equal(7, added.Count);
			Assert.All(added, x => Assert.Null(x.FirstValue));
			Assert.All(added, x => Assert.True(x.HasDiff));
		}

		/// <summary>
		/// Через интерфейс оба аргумента null — пустой результат.
		/// </summary>
		[Fact]
		public void InterfaceCompare_BothNull_ReturnsEmptyResult()
		{
			IObjectComparerInstance comparer = _comparer;

			Assert.Empty(comparer.Compare(null, null));
		}

		/// <summary>
		/// Через интерфейс результат совпадает с типизированным вызовом.
		/// </summary>
		[Fact]
		public void InterfaceCompare_SameResultAsTypedCompare()
		{
			IObjectComparerInstance comparer = _comparer;
			var first = CreateSimple();
			var second = CreateSimple();
			second.Name = "Кулер";

			var typed = _comparer.Compare(first, second);
			var untyped = comparer.Compare(first, second);

			Assert.Equal(
				typed.Select(x => (x.PropertyName, x.FirstValue, x.SecondValue, x.HasDiff)),
				untyped.Select(x => (x.PropertyName, x.FirstValue, x.SecondValue, x.HasDiff)));
			Assert.Equal(typeof(SimpleEntity), comparer.ObjectType);
		}

		/// <summary>
		/// Через интерфейс объекты чужого типа — InvalidCastException.
		/// </summary>
		[Fact]
		public void InterfaceCompare_WrongObjectType_ThrowsInvalidCastException()
		{
			IObjectComparerInstance comparer = _comparer;

			Assert.Throws<InvalidCastException>(() => comparer.Compare("a", "b"));
		}

		#endregion
	}
}
