using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace VodovozHealthCheck.Extensions
{
	/// <summary>
	///		Набор расширений для преобразования объектов в содержимое HTTP-запросов и работы с URI/запросными параметрами.
	/// </summary>
	public static class HttpContentExtensions
	{
		/// <summary>
		///		Возвращает базовый URL запроса (схема + хост).
		/// </summary>
		/// <param name="request">Текущий HTTP-запрос.</param>
		/// <returns>Базовый URL в виде строки, например "https://example.com".</returns>
		public static string BaseUrl(this HttpRequest request)
		{
			return $"{request.Scheme}://{request.Host}";
		}

		/// <summary>
		///		Преобразует объект в JSON-контент для использования в HTTP-запросе.
		/// </summary>
		/// <typeparam name="T">Тип передаваемого объекта.</typeparam>
		/// <param name="obj">Объект для сериализации в JSON.</param>
		/// <param name="options">Опции сериализации JSON. Если null — используются опции по умолчанию.</param>
		/// <returns>Экземпляр <see cref="HttpContent"/>, содержащий сериализованный JSON.</returns>
		public static HttpContent ToJsonContent<T>(this T obj, JsonSerializerOptions options = null) =>
			JsonContent.Create(obj, options: options);

		/// <summary>
		///		Преобразует объект в XML-контент для использования в HTTP-запросе.
		/// </summary>
		/// <typeparam name="T">Тип передаваемого объекта.</typeparam>
		/// <param name="obj">Объект для сериализации в XML.</param>
		/// <returns>Экземпляр <see cref="HttpContent"/>, содержащий сериализованный XML с кодировкой UTF-8 и media-type "application/xml".</returns>
		public static HttpContent ToXmlContent<T>(this T obj)
		{
			var serializer = new XmlSerializer(typeof(T));
			using var writer = new StringWriter();
			serializer.Serialize(writer, obj);
			return new StringContent(writer.ToString(), Encoding.UTF8, "application/xml");
		}

		/// <summary>
		///		Преобразует объект в <see cref="FormUrlEncodedContent"/>, исключая свойства со значением null.
		/// </summary>
		/// <typeparam name="T">Тип передаваемого объекта.</typeparam>
		/// <param name="obj">Объект, свойства которого будут преобразованы в пары ключ/значение.</param>
		/// <returns>Экземпляр <see cref="FormUrlEncodedContent"/> со значениями свойств объекта.</returns>
		public static HttpContent ToFormUrlEncodedContent<T>(this T obj)
		{
			var properties = typeof(T)
				.GetProperties()
				.Where(p => p.GetValue(obj) != null)
				.ToDictionary(
					p => p.Name,
					p => p.GetValue(obj)?.ToString() ?? string.Empty);

			return new FormUrlEncodedContent(properties);
		}

		/// <summary>
		///		Добавляет к URI параметры query string, полученные из объекта.
		/// </summary>
		/// <typeparam name="T">Тип объекта с параметрами.</typeparam>
		/// <param name="uri">Исходный URI.</param>
		/// <param name="obj">Объект, преобразуемый в query string.</param>
		/// <returns>URI с добавленной строкой запроса, если в объекте есть параметры; иначе оригинальный URI.</returns>
		public static string AppendQueryString<T>(this string uri, T obj)
		{
			if(obj == null)
			{
				return uri;
			}

			var queryString = obj.ToQueryString();

			if(string.IsNullOrEmpty(queryString))
			{
				return uri;
			}

			var separator = uri.Contains('?') ? '&' : '?';

			return $"{uri}{separator}{queryString}";
		}

		/// <summary>
		///		Преобразует объект в строку query string, экранируя значения.
		///		Игнорирует свойства со значением null.
		/// </summary>
		/// <typeparam name="T">Тип объекта с параметрами.</typeparam>
		/// <param name="obj">Объект, который нужно преобразовать в query string.</param>
		/// <returns>Строка query string без ведущего символа '?', либо пустая строка если параметров нет.</returns>
		public static string ToQueryString<T>(this T obj)
		{
			if(obj == null)
			{
				return string.Empty;
			}

			var properties = typeof(T)
				.GetProperties()
				.Where(p => p.CanRead && p.GetValue(obj) != null)
				.Select(p =>
				{
					var name = GetPropertyName(p);
					var value = p.GetValue(obj);

					// Специальная обработка для Guid
					var stringValue = /*value is Guid guid ? guid.ToString() :*/ value?.ToString();

					return $"{name}={Uri.EscapeDataString(stringValue ?? string.Empty)}";
				});

			return string.Join("&", properties);
		}

		/// <summary>
		///		Возвращает имя свойства, учитывая атрибуты <see cref="BindPropertyAttribute"/> и <see cref="JsonPropertyNameAttribute"/>.
		///		Если атрибуты отсутствуют — возвращает фактическое имя свойства.
		/// </summary>
		/// <param name="property">Информация о свойстве.</param>
		/// <returns>Имя параметра для использования в query string или формах.</returns>
		private static string GetPropertyName(PropertyInfo property)
		{
			var bindPropertyAttr = property.GetCustomAttribute<BindPropertyAttribute>();

			if(bindPropertyAttr?.Name != null)
			{
				return bindPropertyAttr.Name;
			}

			var jsonPropertyAttr = property.GetCustomAttribute<JsonPropertyNameAttribute>();

			return jsonPropertyAttr?.Name != null ? jsonPropertyAttr.Name : property.Name;
		}
	}
}
