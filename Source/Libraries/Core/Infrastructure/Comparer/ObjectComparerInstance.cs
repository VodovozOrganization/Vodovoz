using Gamma.Utilities;
using QS.DomainModel.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Comparer
{
	/// <summary>
	/// Готовый компаратор объектов типа <typeparamref name="T"/>, созданный
	/// <see cref="ObjectComparerInstanceBuilder{T}.Build"/>. Настройки после создания не меняются,
	/// поэтому один экземпляр можно использовать многократно, в том числе как вложенный компаратор.
	/// Простые свойства (не классы и не коллекции, а также string) сравниваются по значению;
	/// сложные свойства и коллекции сравниваются только при настроенном CompareObject, иначе пропускаются.
	/// </summary>
	public sealed class ObjectComparerInstance<T> : IObjectComparerInstance
	{
		private readonly DefaultComparisonMode _mode;
		private readonly HashSet<PropertyInfo> _includedProperties;
		private readonly HashSet<PropertyInfo> _excludedProperties;
		private readonly Dictionary<PropertyInfo, IObjectComparerInstance> _nestedComparers;

		/// <summary>
		/// Создаёт компаратор. Коллекции настроек копируются.
		/// </summary>
		internal ObjectComparerInstance(
			DefaultComparisonMode mode,
			IEnumerable<PropertyInfo> includedProperties,
			IEnumerable<PropertyInfo> excludedProperties,
			IEnumerable<KeyValuePair<PropertyInfo, IObjectComparerInstance>> nestedComparers)
		{
			_mode = mode;
			_includedProperties = new HashSet<PropertyInfo>(includedProperties ?? Enumerable.Empty<PropertyInfo>());
			_excludedProperties = new HashSet<PropertyInfo>(excludedProperties ?? Enumerable.Empty<PropertyInfo>());
			_nestedComparers = nestedComparers != null
				? nestedComparers.ToDictionary(x => x.Key, x => x.Value)
				: new Dictionary<PropertyInfo, IObjectComparerInstance>();
		}

		/// <summary>
		/// Тип объектов, которые сравнивает компаратор.
		/// </summary>
		public Type ObjectType => typeof(T);

		/// <summary>
		/// Сравнение через интерфейс: используется для вложенных объектов и элементов коллекций,
		/// поэтому null с любой стороны допустим.
		/// </summary>
		IReadOnlyList<PropertyDifference> IObjectComparerInstance.Compare(object first, object second, IDiffFormatter formatter)
		{
			return CompareCore((T)first, (T)second, formatter, allowNullValues: true);
		}

		/// <summary>
		/// Сравнивает объекты по свойствам и возвращает запись на каждое сравниваемое свойство.
		/// Для одного и того же экземпляра возвращает пустой список.
		/// Выбрасывает <see cref="ArgumentNullException"/>, если один из объектов null.
		/// </summary>
		public IReadOnlyList<PropertyDifference> Compare(T first, T second, IDiffFormatter formatter = null)
		{
			return CompareCore(first, second, formatter, allowNullValues: false);
		}

		/// <summary>
		/// Сравнивает объекты по свойствам.
		/// При <paramref name="allowNullValues"/> = true один из объектов может быть null:
		/// тогда значения его свойств считаются null, а свойства второго объекта выводятся полностью.
		/// </summary>
		private IReadOnlyList<PropertyDifference> CompareCore(T first, T second, IDiffFormatter formatter, bool allowNullValues)
		{
			if(ReferenceEquals(first, second))
			{
				return new List<PropertyDifference>();
			}

			if(!allowNullValues)
			{
				if(first == null)
				{
					throw new ArgumentNullException(nameof(first));
				}

				if(second == null)
				{
					throw new ArgumentNullException(nameof(second));
				}
			}

			var effectiveFormatter = formatter ?? NullDiffFormatter.Instance;
			var result = new List<PropertyDifference>();

			foreach(var property in GetProperties())
			{
				var firstValue = first == null ? null : property.GetValue(first, null);
				var secondValue = second == null ? null : property.GetValue(second, null);

				if(_nestedComparers.TryGetValue(property, out IObjectComparerInstance nestedComparer))
				{
					if(PropertyExpressionHelper.IsEnumerableType(property.PropertyType))
					{
						result.Add(BuildEnumerableDifference(property, firstValue, secondValue, nestedComparer, effectiveFormatter));
						continue;
					}

					result.Add(BuildObjectDifference(
						property.Name,
						PropertyExpressionHelper.GetPropertyTitle(property),
						firstValue,
						secondValue,
						nestedComparer,
						effectiveFormatter,
						isCollectionItem: false));
					continue;
				}

				if(PropertyExpressionHelper.IsComplexType(property.PropertyType) || PropertyExpressionHelper.IsEnumerableType(property.PropertyType))
				{
					continue;
				}

				var diff = new PropertyDifference(
					property.Name,
					PropertyExpressionHelper.GetPropertyTitle(property),
					GetValueString(firstValue),
					GetValueString(secondValue),
					null,
					false,
					valuesDiffer: !Equals(firstValue, secondValue)
				);

				diff.ApplyFormatter(effectiveFormatter);
				result.Add(diff);
			}

			return result;
		}

		/// <summary>
		/// Текстовое представление значения простого свойства: bool — «Да»/«Нет»,
		/// enum — название из [Display(Name)], decimal — без незначащих нулей,
		/// дата — с долями секунды, если они есть, остальное — через ToString().
		/// </summary>
		private static string GetValueString(object value)
		{
			switch(value)
			{
				case null:
					return null;
				case bool boolValue:
					return boolValue ? "Да" : "Нет";
				case Enum enumValue:
					return GetEnumString(enumValue);
				case decimal decimalValue:
					// 1.5m и 1.50m — одно число, убираем незначащие нули дробной части
					return (decimalValue / 1.000000000000000000000000000000000m).ToString();
				case DateTime dateTimeValue:
					return dateTimeValue.Ticks % TimeSpan.TicksPerSecond == 0
						? dateTimeValue.ToString()
						: dateTimeValue.ToString(GetPreciseDateTimePattern());
				case DateTimeOffset dateTimeOffsetValue:
					return dateTimeOffsetValue.Ticks % TimeSpan.TicksPerSecond == 0
						? dateTimeOffsetValue.ToString()
						: dateTimeOffsetValue.ToString(GetPreciseDateTimePattern() + " zzz");
				default:
					return value.ToString();
			}
		}

		/// <summary>
		/// Название значения enum из [Display(Name)].
		/// Для комбинации флагов — названия флагов через запятую,
		/// для значения, не объявленного в типе, — его числовое представление.
		/// </summary>
		private static string GetEnumString(Enum value)
		{
			var enumType = value.GetType();

			if(Enum.IsDefined(enumType, value))
			{
				return value.GetEnumTitle();
			}

			var names = value.ToString().Split(new[] { ", " }, StringSplitOptions.None);

			if(enumType.IsDefined(typeof(FlagsAttribute), false) && names.All(x => Enum.IsDefined(enumType, x)))
			{
				return string.Join(", ", names.Select(x => ((Enum)Enum.Parse(enumType, x)).GetEnumTitle()));
			}

			return value.ToString();
		}

		/// <summary>
		/// Шаблон даты и времени текущей культуры с долями секунды,
		/// чтобы значения, отличающиеся миллисекундами, не выглядели одинаково.
		/// </summary>
		private static string GetPreciseDateTimePattern()
		{
			var format = CultureInfo.CurrentCulture.DateTimeFormat;
			return format.ShortDatePattern + " " + format.LongTimePattern.Replace("ss", "ss.FFFFFFF");
		}

		/// <summary>
		/// Публичные читаемые свойства экземпляра без индексатора, отобранные по режиму и настройкам.
		/// </summary>
		private IEnumerable<PropertyInfo> GetProperties()
		{
			var allProperties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.CanRead && x.GetIndexParameters().Length == 0);

			if(_mode == DefaultComparisonMode.Include)
			{
				return allProperties.Where(x => !_excludedProperties.Contains(x));
			}

			// Свойство с CompareObject сравнивается и без отдельного Include, если не исключено явно
			return allProperties.Where(x =>
				(_includedProperties.Contains(x) || _nestedComparers.ContainsKey(x))
				&& !_excludedProperties.Contains(x));
		}

		/// <summary>
		/// Запись вложенного объекта. Текстовых значений у неё нет: отличие определяется
		/// наличием объекта с каждой стороны и дочерними различиями.
		/// Если объект есть только с одной стороны, его свойства выводятся полностью.
		/// </summary>
		private static PropertyDifference BuildObjectDifference(
			string propertyName,
			string displayName,
			object firstValue,
			object secondValue,
			IObjectComparerInstance nestedComparer,
			IDiffFormatter formatter,
			bool isCollectionItem)
		{
			var childDifferences = nestedComparer.Compare(firstValue, secondValue, formatter);

			return new PropertyDifference(
				propertyName,
				displayName,
				string.Empty,
				string.Empty,
				childDifferences,
				isCollectionItem,
				valuesDiffer: (firstValue == null) != (secondValue == null));
		}

		/// <summary>
		/// Запись коллекции. Текстовых значений у неё нет; элементы сравниваются по индексу,
		/// индексы, где элементы null с обеих сторон, пропускаются.
		/// null-коллекция считается пустой.
		/// </summary>
		private PropertyDifference BuildEnumerableDifference(PropertyInfo property, object firstValue, object secondValue, IObjectComparerInstance nestedComparer, IDiffFormatter formatter)
		{
			var firstItems = ToEnumerable(firstValue).ToList();
			var secondItems = ToEnumerable(secondValue).ToList();
			var childDifferences = new List<PropertyDifference>();
			var maxCount = Math.Max(firstItems.Count, secondItems.Count);

			var appellativeAttribute = nestedComparer.ObjectType.GetAttribute<AppellativeAttribute>(false);
			var itemName = appellativeAttribute?.Nominative ?? nestedComparer.ObjectType.Name;

			for(var index = 0; index < maxCount; index++)
			{
				var firstItem = index < firstItems.Count ? firstItems[index] : null;
				var secondItem = index < secondItems.Count ? secondItems[index] : null;

				if(firstItem == null && secondItem == null)
				{
					continue;
				}

				childDifferences.Add(BuildObjectDifference(
					index.ToString(),
					$"{itemName} [{index}]",
					firstItem,
					secondItem,
					nestedComparer,
					formatter,
					isCollectionItem: true));
			}

			return new PropertyDifference(
				property.Name,
				PropertyExpressionHelper.GetPropertyTitle(property),
				string.Empty,
				string.Empty,
				childDifferences);
		}

		/// <summary>
		/// Возвращает элементы коллекции, для null — пустую последовательность.
		/// </summary>
		private static IEnumerable<object> ToEnumerable(object value)
		{
			if(value == null)
			{
				return Enumerable.Empty<object>();
			}

			return ((IEnumerable)value).Cast<object>();
		}
	}
}
