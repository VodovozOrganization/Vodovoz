using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Comparer.Tests
{
	public enum TestStatus
	{
		[Display(Name = "Новый")]
		New,
		[Display(Name = "Закрыт")]
		Closed,
		WithoutDisplay
	}

	[Flags]
	public enum TestFlags
	{
		None = 0,
		[Display(Name = "Первый")]
		First = 1,
		[Display(Name = "Второй")]
		Second = 2
	}

	/// <summary>
	/// Объект только с простыми свойствами (7 штук).
	/// </summary>
	public class SimpleEntity
	{
		public int Id { get; set; }

		[Display(Name = "Название")]
		public string Name { get; set; }

		public decimal Amount { get; set; }

		public DateTime Date { get; set; }

		public int? NullableCount { get; set; }

		public bool IsActive { get; set; }

		public TestStatus Status { get; set; }
	}

	public class FlagsEntity
	{
		public TestFlags Flags { get; set; }
	}

	/// <summary>
	/// Объект с разными видами членов: поле, вычисляемое свойство, write-only, индексатор,
	/// статическое и непубличные свойства.
	/// </summary>
	public class MembersEntity
	{
		public string Field;

		public string ReadWrite { get; set; }

		public string Computed => "computed:" + ReadWrite;

		public string WriteOnly { set { } }

		public string this[int index] => index.ToString();

		public static string Static { get; set; }

		protected string Protected { get; set; }

		internal string Internal { get; set; }

		private string Private { get; set; }
	}

	public class Address
	{
		[Display(Name = "Город")]
		public string City { get; set; }

		public string Street { get; set; }
	}

	public interface ITitled
	{
		string Title { get; }
	}

	[Appellative(Nominative = "Строка документа")]
	public class DocumentLine
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public decimal Amount { get; set; }
	}

	/// <summary>
	/// Элемент коллекции без <see cref="AppellativeAttribute"/>.
	/// </summary>
	public class PlainLine
	{
		public string Name { get; set; }
	}

	public class BaseEntity
	{
		public int Id { get; set; }

		public string BaseName { get; set; }
	}

	public class DerivedEntity : BaseEntity
	{
		public string Name { get; set; }
	}

	/// <summary>
	/// Объект со сложными свойствами и коллекциями.
	/// Простые свойства: только Id и Number.
	/// </summary>
	public class Document
	{
		public int Id { get; set; }

		[Display(Name = "Номер")]
		public string Number { get; set; }

		[Display(Name = "Адрес доставки")]
		public Address Address { get; set; }

		public Address BillingAddress { get; set; }

		[Display(Name = "Строки")]
		public IList<DocumentLine> Lines { get; set; } = new List<DocumentLine>();

		public DocumentLine[] LineArray { get; set; }

		public IEnumerable<PlainLine> PlainLines { get; set; }

		public List<string> Tags { get; set; }

		public int[] Numbers { get; set; }

		public object Payload { get; set; }

		public BaseEntity Entity { get; set; }

		public DocumentLine MainLine { get; set; }
	}
}
