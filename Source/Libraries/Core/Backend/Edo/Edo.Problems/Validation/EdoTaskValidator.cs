using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation
{
	public class EdoTaskValidator
	{
		private readonly ILogger<EdoTaskValidator> _logger;
		private readonly EdoTaskValidatorsProvider _edoTaskValidatorsProvider;
		private readonly IServiceProvider _serviceProvider;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;

		public EdoTaskValidator(
			ILogger<EdoTaskValidator> logger,
			EdoTaskValidatorsProvider edoTaskValidatorsProvider,
			IServiceProvider serviceProvider,
			EdoProblemRegistrar edoProblemRegistrar
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoTaskValidatorsProvider = edoTaskValidatorsProvider ?? throw new ArgumentNullException(nameof(edoTaskValidatorsProvider));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
		}

		public async Task<bool> Validate(
			EdoTask edoTask,
			CancellationToken cancellationToken
			)
		{
			var validationContext = new EdoTaskValidationContext();
			return await Validate(edoTask, validationContext, cancellationToken);
		}

		public async Task<bool> Validate(
			EdoTask edoTask,
			CancellationToken cancellationToken,
			params object[] services
			)
		{
			var validationContext = new EdoTaskValidationContext();
			foreach(var service in services)
			{
				validationContext.AddService(service);
			}
			return await Validate(edoTask, validationContext, cancellationToken);
		}

		public async Task<bool> Validate(
			EdoTask edoTask,
			EdoTaskValidationContext validationContext,
			CancellationToken cancellationToken
			)
		{
			var serviceProvider = _serviceProvider;
			if(validationContext != null)
			{
				validationContext.AddServiceProvider(_serviceProvider);
				serviceProvider = validationContext;
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
					_logger.LogWarning(ex, "Ошибка валидации задачи {TransferEdoTaskType} валидатором {ValidatorName}. " +
						"Возможно в валидаторе не правильно реализован метод {MethodName}",
						edoTask.GetType().Name,
						validator.Name,
						nameof(validator.IsApplicable));
				}
			}

			await _edoProblemRegistrar.UpdateValidationProblems(edoTask, results, cancellationToken);

			return results.All(x => x.IsValid);
		}
	}
}
