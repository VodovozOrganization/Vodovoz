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
			var propertyDisplayName = GetPropertyDisplayName(propertyName);

			_logger.LogWarning($"Ошибка валидации: Не заполнено свойство {propertyDisplayName}");

			return new ValidationResult($"Не заполнено свойство {propertyDisplayName}", new[] { propertyDisplayName });
		}

		public ValidationResult CreateForDateNotInRange(string propertyName, DateTime minimalDate, DateTime maximalDate, DateTime? date)
		{
			var propertyDisplayName = GetPropertyDisplayName(propertyName);

			_logger.LogWarning($"Ошибка валидации: Указанная в свойстве {propertyDisplayName} дата {date:g} находится вне диапазона {minimalDate:g} - {maximalDate:g}");

			return new ValidationResult($"Указанная в свойстве {propertyDisplayName} дата {date:g} находится вне диапазона {minimalDate:g} - {maximalDate:g}", new[] { propertyDisplayName });
		}

		public ValidationResult CreateForLeZero(string propertyName)
		{
			var propertyDisplayName = GetPropertyDisplayName(propertyName);

			_logger.LogWarning($"Ошибка валидации: Значение свойства {propertyDisplayName} должно быть больше нуля");

			return new ValidationResult($"Значение свойства {propertyDisplayName} должно быть больше нуля", new[] { propertyDisplayName });
		}

		private string GetPropertyDisplayName(string propertyName)
		{
			if(TryGetPropertyDisplayName(propertyName, out var propertyDisplayName))
			{
				return propertyDisplayName;
			}

			var exception = new ArgumentException("Некорректное значение названия свойства или не установлен аттрибут DisplayAttribute с параметром Name");

			_logger.LogCritical(exception, "Ошибка настройки валидации");

			throw exception;
		}

		private bool TryGetPropertyDisplayName(string propertyName, out string propertyDisplayName)
		{
			propertyDisplayName = typeof(TEntity)
				.GetProperty(propertyName)?
				.GetCustomAttribute<DisplayAttribute>()?.Name;

			if(string.IsNullOrWhiteSpace(propertyDisplayName))
			{
				return false;
			}

			return true;
		}
	}
}
