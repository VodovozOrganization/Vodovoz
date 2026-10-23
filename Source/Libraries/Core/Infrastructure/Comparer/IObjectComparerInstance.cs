using System;
using System.Collections.Generic;

namespace Comparer
{
	/// <summary>
	/// Нетипизированный компаратор объектов. Позволяет хранить компараторы вложенных объектов
	/// и элементов коллекций разных типов в одной настройке и вызывать их для значений типа object.
	/// </summary>
	public interface IObjectComparerInstance
	{
		/// <summary>
		/// Тип объектов, которые сравнивает компаратор.
		/// </summary>
		Type ObjectType { get; }

		/// <summary>
		/// Сравнивает объекты по свойствам. Любой из объектов может быть null:
		/// тогда значения его свойств считаются null. Если оба объекта null или это один экземпляр,
		/// возвращается пустой список. Форматтер применяется к значениям простых свойств.
		/// </summary>
		IReadOnlyList<PropertyDifference> Compare(object first, object second, IDiffFormatter formatter = null);
	}
}
