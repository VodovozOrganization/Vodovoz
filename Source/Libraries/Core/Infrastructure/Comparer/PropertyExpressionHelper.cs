using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Comparer
{
	/// <summary>
	/// Вспомогательные методы для разбора выражений выбора свойств и классификации типов свойств.
	/// </summary>
	internal static class PropertyExpressionHelper
	{
		/// <summary>
		/// Возвращает свойство типа <typeparamref name="T"/>, на которое ссылается выражение вида x => x.Property.
		/// Приведение типа (x => (object)x.Property) допускается.
		/// Выбрасывает <see cref="ArgumentException"/>, если выражение ссылается не на свойство
		/// или ссылается на вложенное свойство (x => x.Property.Nested).
		/// </summary>
		internal static PropertyInfo GetProperty<T, TProperty>(Expression<Func<T, TProperty>> expression)
		{
			Expression body = expression.Body;

			var unary = body as UnaryExpression;

			if(unary != null && unary.NodeType == ExpressionType.Convert)
			{
				body = unary.Operand;
			}

			var member = body as MemberExpression;

			if(member == null)
			{
				throw new ArgumentException(
					"Expression must reference a property.",
					nameof(expression));
			}

			var property = member.Member as PropertyInfo;

			if(property == null)
			{
				throw new ArgumentException(
					"Expression must reference a property.",
					nameof(expression));
			}

			if(!(member.Expression is ParameterExpression))
			{
				throw new ArgumentException(
					$"Expression must reference a property of '{typeof(T).Name}' directly, nested property paths are not supported.",
					nameof(expression));
			}

			return NormalizeProperty(typeof(T), property);
		}

		/// <summary>
		/// Возвращает <see cref="PropertyInfo"/> того же свойства, полученный из <paramref name="type"/>.
		/// Свойство из выражения для унаследованного свойства имеет ReflectedType базового класса
		/// и не равно свойству из typeof(T).GetProperties().
		/// </summary>
		private static PropertyInfo NormalizeProperty(Type type, PropertyInfo property)
		{
			var key = GetPropertyKey(property);

			var normalized = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				.FirstOrDefault(x => GetPropertyKey(x).Equals(key));

			return normalized ?? property;
		}

		/// <summary>
		/// Возвращает ключ, одинаковый для всех <see cref="PropertyInfo"/> одного свойства
		/// независимо от ReflectedType: модуль и токен метаданных базового определения геттера
		/// (для свойства без геттера — самого свойства).
		/// </summary>
		private static (Module Module, int MetadataToken) GetPropertyKey(PropertyInfo property)
		{
			MemberInfo member = property.GetGetMethod(true)?.GetBaseDefinition();

			if(member == null)
			{
				member = property;
			}

			return (member.Module, member.MetadataToken);
		}

		/// <summary>
		/// Возвращает true, если тип является классом (кроме string),
		/// и такое свойство должно быть исключено из сравнения.
		/// </summary>
		internal static bool IsComplexType(Type type)
		{
			return type.IsClass && type != typeof(string);
		}

		/// <summary>
		/// Возвращает true, если тип является коллекцией (реализует <see cref="IEnumerable"/>), кроме string.
		/// </summary>
		internal static bool IsEnumerableType(Type type)
		{
			return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
		}

		/// <summary>
		/// Возвращает тип элементов коллекции: тип элемента массива или аргумент <see cref="IEnumerable{T}"/>.
		/// Для необобщённой коллекции возвращает <see cref="object"/>.
		/// </summary>
		internal static Type GetEnumerableItemType(Type type)
		{
			if(type.IsArray)
			{
				return type.GetElementType();
			}

			if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				return type.GetGenericArguments()[0];
			}

			var enumerableInterface = type.GetInterfaces()
				.FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

			return enumerableInterface != null ? enumerableInterface.GetGenericArguments()[0] : typeof(object);
		}

		/// <summary>
		/// Возвращает Display(Name) из атрибута свойства.
		/// Если атрибута нет — возвращает <paramref name="property"/>.Name.
		/// </summary>
		internal static string GetPropertyTitle(PropertyInfo property)
		{
			var attrs = (DisplayAttribute[])property.GetCustomAttributes(typeof(DisplayAttribute), true);

			if(attrs != null && attrs.Length > 0)
			{
				var name = attrs[0].GetName();

				if(!string.IsNullOrWhiteSpace(name))
				{
					return name;
				}
			}

			return property.Name;
		}
	}
}
