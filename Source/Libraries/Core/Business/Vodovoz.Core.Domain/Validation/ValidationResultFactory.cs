using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Vodovoz.Core.Domain.Validation
{
	public class ValidationResultFactory<TEntity> : IValidationResultFactory<TEntity>
		where TEntity : class
	{
		private readonly ILogger<ValidationResultFactory<TEntity>> _logger;

		public ValidationResultFactory(ILogger<ValidationResultFactory<TEntity>> logger)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
		}

		public ValidationResult CreateForNullProperty(string propertyName)
		{
			var propertyDisplayName = typeof(TEntity)
				.GetProperty(propertyName)?
				.GetCustomAttribute<DisplayAttribute>()?.Name;

			if(string.IsNullOrWhiteSpace(propertyDisplayName))
			{
				var exception = new ArgumentException("Некорректное значение названия свойства или не установлен аттрибут DisplayAttribute с параметром Name");

				_logger.LogCritical(exception, "Ошибка настройки валидации");
				throw exception;
			}

			_logger.LogWarning($"Ошибка валидации: Не заполнено свойство {propertyDisplayName}");

			return new ValidationResult($"Не заполнено свойство {propertyDisplayName}", new[] { propertyDisplayName });
		}
	}
}
