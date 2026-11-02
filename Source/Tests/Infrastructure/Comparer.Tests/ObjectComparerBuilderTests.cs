using System;
using System.Collections.Generic;
using Xunit;
using static Comparer.Tests.TestHelpers;

namespace Comparer.Tests
{
	/// <summary>
	/// Режимы сравнения, Include/Exclude, разбор выражений и конфигурация CompareObject.
	/// </summary>
	public class ObjectComparerBuilderTests
	{
		#region Режимы DefaultInclude / DefaultExclude

		/// <summary>
		/// DefaultInclude без настроек сравнивает все публичные простые свойства.
		/// </summary>
		[Fact]
		public void DefaultInclude_WithoutConfiguration_ComparesAllSimpleProperties()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>().DefaultInclude().Build();

			var result = comparer.Compare(CreateSimple(), CreateSimple());

			AssertPropertyNames(result, "Id", "Name", "Amount", "Date", "NullableCount", "IsActive", "Status");
		}

		/// <summary>
		/// DefaultInclude + Exclude убирает указанные свойства.
		/// </summary>
		[Fact]
		public void DefaultInclude_Exclude_RemovesExcludedProperties()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>()
				.DefaultInclude()
				.Exclude(x => x.Id)
				.Exclude(x => x.Amount)
				.Build();

			var result = comparer.Compare(CreateSimple(), CreateSimple());

			AssertPropertyNames(result, "Name", "Date", "NullableCount", "IsActive", "Status");
		}

		/// <summary>
		/// В режиме DefaultInclude вызов Include ничего не меняет.
		/// </summary>
		[Fact]
		public void DefaultInclude_Include_HasNoEffect()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>()
				.DefaultInclude()
				.Include(x => x.Name)
				.Build();

			var result = comparer.Compare(CreateSimple(), CreateSimple());

			Assert.Equal(7, result.Count);
		}

		/// <summary>
		/// DefaultExclude без настроек не сравнивает ничего.
		/// </summary>
		[Fact]
		public void DefaultExclude_WithoutConfiguration_ReturnsEmptyResult()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>().DefaultExclude().Build();

			var result = comparer.Compare(CreateSimple(), CreateSimple());

			Assert.Empty(result);
		}

		/// <summary>
		/// DefaultExclude + Include сравнивает только указанные свойства.
		/// </summary>
		[Fact]
		public void DefaultExclude_Include_ComparesOnlyIncludedProperties()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>()
				.DefaultExclude()
				.Include(x => x.Name)
				.Include(x => x.Id)
				.Build();

			var result = comparer.Compare(CreateSimple(), CreateSimple());

			AssertPropertyNames(result, "Id", "Name");
		}

		/// <summary>
		/// В режиме DefaultExclude вызов Exclude ничего не меняет.
		/// </summary>
		[Fact]
		public void DefaultExclude_Exclude_HasNoEffect()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>()
				.DefaultExclude()
				.Exclude(x => x.Name)
				.Build();

			var result = comparer.Compare(CreateSimple(), CreateSimple());

			Assert.Empty(result);
		}

		/// <summary>
		/// Если свойство сначала включено, а затем исключено, побеждает последний вызов.
		/// </summary>
		[Fact]
		public void IncludeThenExclude_SameProperty_LastCallWins()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>()
				.DefaultExclude()
				.Include(x => x.Name)
				.Exclude(x => x.Name)
				.Build();

			var result = comparer.Compare(CreateSimple(), CreateSimple());

			Assert.Empty(result);
		}

		/// <summary>
		/// Если свойство сначала исключено, а затем включено, побеждает последний вызов.
		/// </summary>
		[Fact]
		public void ExcludeThenInclude_SameProperty_LastCallWins()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>()
				.DefaultInclude()
				.Exclude(x => x.Name)
				.Include(x => x.Name)
				.Build();

			var result = comparer.Compare(CreateSimple(), CreateSimple());

			Find(result, "Name");
		}

		/// <summary>
		/// Exclude свойства, объявленного в базовом классе, исключает его.
		/// </summary>
		[Fact]
		public void DefaultInclude_ExcludeInheritedProperty_PropertyExcluded()
		{
			var comparer = new ObjectPropertyComparer<DerivedEntity>()
				.DefaultInclude()
				.Exclude(x => x.Id)
				.Build();

			var result = comparer.Compare(new DerivedEntity { Id = 1 }, new DerivedEntity { Id = 2 });

			Assert.DoesNotContain(result, x => x.PropertyName == "Id");
		}

		/// <summary>
		/// Include свойства, объявленного в базовом классе, включает его.
		/// </summary>
		[Fact]
		public void DefaultExclude_IncludeInheritedProperty_PropertyIncluded()
		{
			var comparer = new ObjectPropertyComparer<DerivedEntity>()
				.DefaultExclude()
				.Include(x => x.BaseName)
				.Build();

			var result = comparer.Compare(new DerivedEntity { BaseName = "a" }, new DerivedEntity { BaseName = "b" });

			Assert.True(Find(result, "BaseName").HasDiff);
		}

		#endregion

		#region Выбор свойств по рефлексии

		/// <summary>
		/// Сложные типы (классы, кроме string, включая object) и коллекции без CompareObject пропускаются.
		/// </summary>
		[Fact]
		public void DefaultInclude_ComplexAndEnumerableProperties_AreSkippedWithoutCompareObject()
		{
			var comparer = new ObjectPropertyComparer<Document>().DefaultInclude().Build();
			var first = new Document { Id = 1, Number = "A", Address = CreateAddress(), Tags = new List<string> { "x" }, Payload = "p1" };
			var second = new Document { Id = 1, Number = "A", Address = CreateAddress("Москва"), Tags = new List<string> { "y" }, Payload = "p2" };

			var result = comparer.Compare(first, second);

			AssertPropertyNames(result, "Id", "Number");
		}

		/// <summary>
		/// В сравнение попадают только публичные читаемые свойства экземпляра без индексатора:
		/// поля, write-only, статические, непубличные свойства и индексатор не участвуют.
		/// </summary>
		[Fact]
		public void DefaultInclude_OnlyPublicReadableInstanceNonIndexedPropertiesAreCompared()
		{
			var comparer = new ObjectPropertyComparer<MembersEntity>().DefaultInclude().Build();

			var result = comparer.Compare(
				new MembersEntity { ReadWrite = "a", Field = "f1" },
				new MembersEntity { ReadWrite = "b", Field = "f2" });

			AssertPropertyNames(result, "ReadWrite", "Computed");
		}

		/// <summary>
		/// Вычисляемые свойства только для чтения сравниваются.
		/// </summary>
		[Fact]
		public void Compare_ComputedProperty_IsCompared()
		{
			var comparer = new ObjectPropertyComparer<MembersEntity>().DefaultInclude().Build();

			var result = comparer.Compare(new MembersEntity { ReadWrite = "a" }, new MembersEntity { ReadWrite = "b" });

			var computed = Find(result, "Computed");
			Assert.Equal("computed:a", computed.FirstValue);
			Assert.Equal("computed:b", computed.SecondValue);
			Assert.True(computed.HasDiff);
		}

		#endregion

		#region Разбор выражений

		/// <summary>
		/// Свойство значимого типа (обёрнутое в Convert до object) корректно распознаётся.
		/// </summary>
		[Fact]
		public void Include_ValueTypeProperty_IsRecognized()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>()
				.DefaultExclude()
				.Include(x => x.Id)
				.Include(x => x.IsActive)
				.Build();

			var result = comparer.Compare(CreateSimple(), CreateSimple());

			AssertPropertyNames(result, "Id", "IsActive");
		}

		/// <summary>
		/// Константа вместо свойства — ArgumentException.
		/// </summary>
		[Fact]
		public void Include_ConstantExpression_ThrowsArgumentException()
		{
			var builder = new ObjectPropertyComparer<SimpleEntity>().DefaultExclude();

			Assert.Throws<ArgumentException>(() => builder.Include(x => "const"));
		}

		/// <summary>
		/// Вызов метода вместо свойства — ArgumentException.
		/// </summary>
		[Fact]
		public void Exclude_MethodCallExpression_ThrowsArgumentException()
		{
			var builder = new ObjectPropertyComparer<SimpleEntity>().DefaultInclude();

			Assert.Throws<ArgumentException>(() => builder.Exclude(x => x.ToString()));
		}

		/// <summary>
		/// Поле вместо свойства — ArgumentException.
		/// </summary>
		[Fact]
		public void Include_FieldExpression_ThrowsArgumentException()
		{
			var builder = new ObjectPropertyComparer<MembersEntity>().DefaultExclude();

			Assert.Throws<ArgumentException>(() => builder.Include(x => x.Field));
		}

		/// <summary>
		/// Вложенный путь (x => x.Address.City) не является свойством T — ArgumentException.
		/// </summary>
		[Fact]
		public void Include_NestedPropertyPath_ThrowsArgumentException()
		{
			var builder = new ObjectPropertyComparer<Document>().DefaultExclude();

			Assert.Throws<ArgumentException>(() => builder.Include(x => x.Address.City));
		}

		/// <summary>
		/// В режиме DefaultExclude явный Exclude убирает свойство, настроенное через CompareObject.
		/// </summary>
		[Fact]
		public void DefaultExclude_CompareObjectThenExclude_PropertyExcluded()
		{
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultExclude()
				.CompareObject(x => x.Address, AddressComparer())
				.Exclude(x => x.Address)
				.Build();

			var result = comparer.Compare(new Document { Address = CreateAddress() }, new Document { Address = CreateAddress("Москва") });

			Assert.Empty(result);
		}

		#endregion

		#region CompareObject — валидация конфигурации

		/// <summary>
		/// CompareObject для объекта с null-компаратором — ArgumentNullException.
		/// </summary>
		[Fact]
		public void CompareObject_NullComparer_ThrowsArgumentNullException()
		{
			var builder = new ObjectPropertyComparer<Document>().DefaultInclude();

			var exception = Assert.Throws<ArgumentNullException>(
				() => builder.CompareObject(x => x.Address, (ObjectComparerInstance<Address>)null));

			Assert.Equal("comparer", exception.ParamName);
		}

		/// <summary>
		/// CompareObject для коллекции с null-компаратором — ArgumentNullException.
		/// </summary>
		[Fact]
		public void CompareObject_Enumerable_NullComparer_ThrowsArgumentNullException()
		{
			var builder = new ObjectPropertyComparer<Document>().DefaultInclude();

			var exception = Assert.Throws<ArgumentNullException>(
				() => builder.CompareObject(x => x.Lines, (ObjectComparerInstance<DocumentLine>)null));

			Assert.Equal("comparer", exception.ParamName);
		}

		/// <summary>
		/// CompareObject для простого свойства (string) — InvalidOperationException.
		/// </summary>
		[Fact]
		public void CompareObject_SimpleProperty_ThrowsInvalidOperationException()
		{
			var stringComparer = new ObjectPropertyComparer<string>().DefaultInclude().Build();
			var builder = new ObjectPropertyComparer<Document>().DefaultInclude();

			Assert.Throws<InvalidOperationException>(() => builder.CompareObject(x => x.Number, stringComparer));
		}

		/// <summary>
		/// Компаратор несовместимого типа для объекта — InvalidOperationException.
		/// </summary>
		[Fact]
		public void CompareObject_IncompatibleComparerType_ThrowsInvalidOperationException()
		{
			var titledComparer = new ObjectPropertyComparer<ITitled>().DefaultInclude().Build();
			var builder = new ObjectPropertyComparer<Document>().DefaultInclude();

			Assert.Throws<InvalidOperationException>(() => builder.CompareObject(x => (ITitled)x.MainLine, titledComparer));
		}

		/// <summary>
		/// Компаратор несовместимого типа для элементов коллекции — InvalidOperationException.
		/// </summary>
		[Fact]
		public void CompareObject_Enumerable_IncompatibleItemType_ThrowsInvalidOperationException()
		{
			var titledComparer = new ObjectPropertyComparer<ITitled>().DefaultInclude().Build();
			var builder = new ObjectPropertyComparer<Document>().DefaultInclude();

			Assert.Throws<InvalidOperationException>(
				() => builder.CompareObject(x => (IEnumerable<ITitled>)x.Lines, titledComparer));
		}

		/// <summary>
		/// Перегрузка для коллекций на свойстве, которое не является коллекцией, — InvalidOperationException.
		/// </summary>
		[Fact]
		public void CompareObject_Enumerable_OnNonEnumerableProperty_ThrowsInvalidOperationException()
		{
			var builder = new ObjectPropertyComparer<Document>().DefaultInclude();

			Assert.Throws<InvalidOperationException>(
				() => builder.CompareObject(x => (IEnumerable<DocumentLine>)x.Payload, LineComparer()));
		}

		/// <summary>
		/// Компаратор базового типа можно назначить свойству производного типа.
		/// </summary>
		[Fact]
		public void CompareObject_ComparerOfBaseType_IsAccepted()
		{
			var objectComparer = new ObjectPropertyComparer<object>().DefaultInclude().Build();
			var builder = new ObjectPropertyComparer<Document>().DefaultInclude();

			var exception = Record.Exception(() => builder.CompareObject<object>(x => x.Address, objectComparer));

			Assert.Null(exception);
		}

		/// <summary>
		/// Компаратор производного типа на свойстве базового типа: конфигурация либо отклоняется сразу,
		/// либо сравнение объектов базового типа проходит без InvalidCastException.
		/// </summary>
		[Fact]
		public void CompareObject_DerivedComparerOnBaseTypedProperty_DoesNotThrowInvalidCastOnCompare()
		{
			var derivedComparer = new ObjectPropertyComparer<DerivedEntity>().DefaultInclude().Build();

			var exception = Record.Exception(() =>
			{
				var comparer = new ObjectPropertyComparer<Document>()
					.DefaultInclude()
					.CompareObject(x => (DerivedEntity)x.Entity, derivedComparer)
					.Build();

				comparer.Compare(
					new Document { Entity = new BaseEntity { Id = 1 } },
					new Document { Entity = new BaseEntity { Id = 2 } });
			});

			Assert.False(exception is InvalidCastException, "Ожидалось отклонение конфигурации или корректное сравнение, а не InvalidCastException.");
		}

		/// <summary>
		/// Повторный CompareObject для того же свойства заменяет компаратор.
		/// </summary>
		[Fact]
		public void CompareObject_CalledTwiceForSameProperty_LastComparerWins()
		{
			var withoutCity = new ObjectPropertyComparer<Address>().DefaultInclude().Exclude(x => x.City).Build();
			var comparer = new ObjectPropertyComparer<Document>()
				.DefaultInclude()
				.CompareObject(x => x.Address, withoutCity)
				.CompareObject(x => x.Address, AddressComparer())
				.Build();

			var result = comparer.Compare(
				new Document { Address = CreateAddress() },
				new Document { Address = CreateAddress() });

			AssertPropertyNames(Find(result, "Address").ChildDifferences, "City", "Street");
		}

		#endregion

		#region Build

		/// <summary>
		/// Методы билдера возвращают тот же экземпляр (fluent API).
		/// </summary>
		[Fact]
		public void BuilderMethods_ReturnSameBuilderInstance()
		{
			var builder = new ObjectPropertyComparer<Document>().DefaultInclude();

			Assert.Same(builder, builder.Include(x => x.Id));
			Assert.Same(builder, builder.Exclude(x => x.Id));
			Assert.Same(builder, builder.CompareObject(x => x.Address, AddressComparer()));
			Assert.Same(builder, builder.CompareObject(x => x.Lines, LineComparer()));
		}

		/// <summary>
		/// ObjectType построенного компаратора — typeof(T).
		/// </summary>
		[Fact]
		public void Build_ObjectType_IsGenericArgument()
		{
			var comparer = new ObjectPropertyComparer<SimpleEntity>().DefaultInclude().Build();

			Assert.Equal(typeof(SimpleEntity), comparer.ObjectType);
		}

		/// <summary>
		/// Изменения билдера после Build не влияют на уже построенный компаратор.
		/// </summary>
		[Fact]
		public void Build_ChangesAfterBuild_DoNotAffectBuiltComparer()
		{
			var builder = new ObjectPropertyComparer<SimpleEntity>().DefaultInclude();
			var comparer = builder.Build();

			builder.Exclude(x => x.Name);
			var result = comparer.Compare(CreateSimple(), CreateSimple());

			Find(result, "Name");
		}

		#endregion
	}
}
