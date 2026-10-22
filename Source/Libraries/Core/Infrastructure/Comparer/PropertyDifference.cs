using System;
using System.Collections.Generic;
using System.Linq;

namespace Comparer
{
	/// <summary>
	/// Результат сравнения одного свойства, вложенного объекта, коллекции или элемента коллекции.
	/// Образует дерево: у вложенного объекта и коллекции текстовых значений нет,
	/// их содержимое представлено в <see cref="ChildDifferences"/>.
	/// Отличие на любом уровне поднимается до корня через <see cref="HasDiff"/>.
	/// </summary>
	public sealed class PropertyDifference
	{
		/// <summary>
		/// Отличие собственных значений, вычисленное компаратором по исходным объектам.
		/// null — отличие определяется сравнением строковых значений.
		/// </summary>
		private readonly bool? _valuesDiffer;

		/// <summary>
		/// Имя свойства; для элемента коллекции — его индекс.
		/// </summary>
		public string PropertyName { get; private set; }

		/// <summary>
		/// Отображаемое название: Display(Name) свойства или его имя;
		/// для элемента коллекции — Appellative(Nominative) типа элемента или имя типа и индекс в квадратных скобках.
		/// </summary>
		public string DisplayName { get; private set; }

		/// <summary>
		/// Текстовое значение у первого объекта; null, если значения нет.
		/// Для вложенного объекта, коллекции и элемента коллекции — пустая строка.
		/// </summary>
		public string FirstValue { get; private set; }

		/// <summary>
		/// Текстовое значение у второго объекта; null, если значения нет.
		/// Для вложенного объекта, коллекции и элемента коллекции — пустая строка.
		/// </summary>
		public string SecondValue { get; private set; }

		/// <summary>
		/// Дочерние записи: свойства вложенного объекта или элементы коллекции. Для простого свойства пуст.
		/// </summary>
		public IReadOnlyList<PropertyDifference> ChildDifferences { get; private set; } = new List<PropertyDifference>();

		/// <summary>
		/// true, если запись — элемент коллекции.
		/// </summary>
		public bool IsCollectionItem { get; private set; }

		/// <summary>
		/// true, если есть отличие: отличаются собственные значения записи
		/// (для вложенного объекта и элемента — объект есть только с одной стороны)
		/// или отличие есть у любой дочерней записи.
		/// </summary>
		public bool HasDiff
		{
			get
			{
				if(_valuesDiffer.HasValue)
				{
					return _valuesDiffer.Value || ChildDifferences.Any(x => x.HasDiff);
				}

				if(ChildDifferences.Any())
				{
					return ChildDifferences.Any(x => x.HasDiff);
				}

				return !string.Equals(FirstValue, SecondValue, StringComparison.Ordinal);
			}
		}

		/// <summary>
		/// Отформатированное первое значение. Заполняется через <see cref="ApplyFormatter"/>.
		/// До вызова равно <see cref="FirstValue"/>.
		/// </summary>
		public string FormattedFirstValue { get; private set; }

		/// <summary>
		/// Отформатированное второе значение. Заполняется через <see cref="ApplyFormatter"/>.
		/// До вызова равно <see cref="SecondValue"/>.
		/// </summary>
		public string FormattedSecondValue { get; private set; }

		/// <summary>
		/// Создаёт запись сравнения. Отличие определяется ординальным сравнением строковых значений,
		/// а при наличии дочерних различий — только ими.
		/// </summary>
		public PropertyDifference(
			string propertyName,
			string displayName,
			string firstValue,
			string secondValue,
			IReadOnlyList<PropertyDifference> childDifferences = null,
			bool isCollectionItem = false
		)
		{
			PropertyName = propertyName;
			DisplayName = displayName;
			FirstValue = firstValue;
			SecondValue = secondValue;
			ChildDifferences = childDifferences ?? new List<PropertyDifference>();
			IsCollectionItem = isCollectionItem;
			FormattedFirstValue = firstValue;
			FormattedSecondValue = secondValue;
		}

		/// <summary>
		/// Создаёт запись сравнения с отличием собственных значений, вычисленным по исходным объектам.
		/// <see cref="HasDiff"/> = <paramref name="valuesDiffer"/> или наличие отличия у дочерних записей.
		/// </summary>
		internal PropertyDifference(
			string propertyName,
			string displayName,
			string firstValue,
			string secondValue,
			IReadOnlyList<PropertyDifference> childDifferences,
			bool isCollectionItem,
			bool valuesDiffer
		) : this(propertyName, displayName, firstValue, secondValue, childDifferences, isCollectionItem)
		{
			_valuesDiffer = valuesDiffer;
		}

		/// <summary>
		/// Применяет форматтер к <see cref="FirstValue"/> и <see cref="SecondValue"/>,
		/// заполняя <see cref="FormattedFirstValue"/> и <see cref="FormattedSecondValue"/>.
		/// </summary>
		internal void ApplyFormatter(IDiffFormatter formatter)
		{
			if(formatter == null)
			{
				throw new ArgumentNullException(nameof(formatter));
			}

			if(ChildDifferences.Any())
			{
				return;
			}

			formatter.SideBySideDiff(FirstValue, SecondValue, out string formattedFirst, out string formattedSecond);
			FormattedFirstValue = formattedFirst;
			FormattedSecondValue = formattedSecond;
		}
	}
}
