using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Comparer
{
	/// <summary>
	/// Построитель компаратора объектов типа <typeparamref name="T"/>.
	/// Определяет, какие свойства сравниваются (режим по умолчанию, Include/Exclude),
	/// и какими компараторами сравниваются вложенные объекты и элементы коллекций (CompareObject).
	/// Методы настройки возвращают этот же построитель, итоговый компаратор создаёт <see cref="Build"/>.
	/// </summary>
	public sealed class ObjectComparerInstanceBuilder<T>
	{
		private readonly DefaultComparisonMode _mode;
		private readonly HashSet<PropertyInfo> _includedProperties = new HashSet<PropertyInfo>();
		private readonly HashSet<PropertyInfo> _excludedProperties = new HashSet<PropertyInfo>();
		private readonly Dictionary<PropertyInfo, IObjectComparerInstance> _nestedComparers = new Dictionary<PropertyInfo, IObjectComparerInstance>();

		/// <summary>
		/// Создаёт построитель компаратора с указанным режимом выбора свойств по умолчанию.
		/// </summary>
		public ObjectComparerInstanceBuilder(DefaultComparisonMode mode)
		{
			_mode = mode;
		}

		/// <summary>
		/// Включает свойство в сравнение. Имеет смысл в режиме <see cref="DefaultComparisonMode.Exclude"/>.
		/// Отменяет предыдущий <see cref="Exclude"/> для этого свойства.
		/// </summary>
		public ObjectComparerInstanceBuilder<T> Include(Expression<Func<T, object>> property)
		{
			SelectProperty(property, include: true);
			return this;
		}

		/// <summary>
		/// Исключает свойство из сравнения, в том числе свойство, настроенное через CompareObject.
		/// Отменяет предыдущий <see cref="Include"/> для этого свойства.
		/// </summary>
		public ObjectComparerInstanceBuilder<T> Exclude(Expression<Func<T, object>> property)
		{
			SelectProperty(property, include: false);
			return this;
		}

		/// <summary>
		/// Задаёт компаратор для вложенного объекта. Свойство сравнивается по свойствам вложенного объекта
		/// и попадает в сравнение в любом режиме, если не исключено через <see cref="Exclude"/>.
		/// Повторный вызов для того же свойства заменяет компаратор.
		/// Выбрасывает <see cref="InvalidOperationException"/>, если свойство не является сложным типом
		/// или компаратор не подходит к типу свойства.
		/// </summary>
		public ObjectComparerInstanceBuilder<T> CompareObject<TProperty>(Expression<Func<T, TProperty>> property, ObjectComparerInstance<TProperty> comparer)
		{
			if(comparer == null)
			{
				throw new ArgumentNullException(nameof(comparer));
			}

			var propertyInfo = PropertyExpressionHelper.GetProperty(property);

			if(!PropertyExpressionHelper.IsComplexType(propertyInfo.PropertyType))
			{
				throw new InvalidOperationException($"Property '{propertyInfo.Name}' must be a complex type to use CompareObject().");
			}

			if(!comparer.ObjectType.IsAssignableFrom(propertyInfo.PropertyType))
			{
				throw new InvalidOperationException(
					$"Comparer type '{comparer.ObjectType.Name}' is incompatible with property '{propertyInfo.Name}' of type '{propertyInfo.PropertyType.Name}'.");
			}

			_nestedComparers[propertyInfo] = comparer;
			return this;
		}

		/// <summary>
		/// Задаёт компаратор для элементов коллекции. Элементы сравниваются попарно по индексу
		/// и попадают в сравнение в любом режиме, если свойство не исключено через <see cref="Exclude"/>.
		/// Повторный вызов для того же свойства заменяет компаратор.
		/// Выбрасывает <see cref="InvalidOperationException"/>, если свойство не является коллекцией
		/// или компаратор не подходит к типу элементов.
		/// </summary>
		public ObjectComparerInstanceBuilder<T> CompareObject<TItem>(Expression<Func<T, IEnumerable<TItem>>> property, ObjectComparerInstance<TItem> comparer)
		{
			if(comparer == null)
			{
				throw new ArgumentNullException(nameof(comparer));
			}

			var propertyInfo = PropertyExpressionHelper.GetProperty(property);
			var itemType = PropertyExpressionHelper.GetEnumerableItemType(propertyInfo.PropertyType);

			if(!PropertyExpressionHelper.IsEnumerableType(propertyInfo.PropertyType))
			{
				throw new InvalidOperationException($"Property '{propertyInfo.Name}' must be an IEnumerable to use CompareObject().");
			}

			if(!comparer.ObjectType.IsAssignableFrom(itemType))
			{
				throw new InvalidOperationException(
					$"Comparer type '{comparer.ObjectType.Name}' is incompatible with enumerable item type '{itemType.Name}' of property '{propertyInfo.Name}'.");
			}

			_nestedComparers[propertyInfo] = comparer;
			return this;
		}

		/// <summary>
		/// Создаёт компаратор с текущими настройками.
		/// Последующие изменения построителя на созданный компаратор не влияют.
		/// </summary>
		public ObjectComparerInstance<T> Build()
		{
			return new ObjectComparerInstance<T>(
				_mode,
				_includedProperties,
				_excludedProperties,
				_nestedComparers);
		}

		/// <summary>
		/// Помечает свойство как включённое или исключённое; последний вызов для свойства побеждает.
		/// </summary>
		private void SelectProperty(Expression<Func<T, object>> propertyExpression, bool include)
		{
			var property = PropertyExpressionHelper.GetProperty(propertyExpression);

			if(include)
			{
				_includedProperties.Add(property);
				_excludedProperties.Remove(property);
				return;
			}

			_excludedProperties.Add(property);
			_includedProperties.Remove(property);
		}
	}
}
