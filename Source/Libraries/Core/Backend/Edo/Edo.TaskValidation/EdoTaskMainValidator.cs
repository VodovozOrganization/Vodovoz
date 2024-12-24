using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.TaskValidation
{
	public class EdoTaskMainValidator
	{
		private readonly ILogger<EdoTaskMainValidator> _logger;
		private readonly EdoTaskValidatorsProvider _edoTaskValidatorsProvider;
		private readonly IServiceProvider _serviceProvider;

		public EdoTaskMainValidator(
			ILogger<EdoTaskMainValidator> logger,
			EdoTaskValidatorsProvider edoTaskValidatorsProvider,
			IServiceProvider serviceProvider
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoTaskValidatorsProvider = edoTaskValidatorsProvider ?? throw new ArgumentNullException(nameof(edoTaskValidatorsProvider));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public async Task<EdoValidationResults> ValidateAsync(EdoTask edoTask, CancellationToken cancellationToken, EdoTaskValidationContext context = null)
		{
			IServiceProvider serviceProvider = _serviceProvider;
			if(context != null)
			{
				context.AddServiceProvider(_serviceProvider);
				serviceProvider = context;
			}

			var validators = _edoTaskValidatorsProvider.GetValidatorsFor(edoTask);
			var results = new List<EdoValidationResult>();
			foreach(var validator in validators)
			{
				try
				{
					var result = await validator.ValidateAsync(edoTask, serviceProvider, cancellationToken);
					results.Add(result);
				}
				catch(EdoTaskValidationException ex)
				{
					_logger.LogWarning(ex, $"Ошибка валидации задачи {edoTask.GetType().Name} валидатором {validator.Name}. " +
						$"Возможно в валидаторе не правильно реализован метод {nameof(validator.IsApplicable)}");
				}
			}

			return new EdoValidationResults(results);
		}
	}
}
