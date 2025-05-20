using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;

namespace Edo.Problems.Validation.Transfer
{
	public class TransferEdoTaskTransferOrderValidator : EdoTaskProblemValidatorSource
	{
		public override string Name => throw new NotImplementedException();

		public override bool IsApplicable(EdoTask edoTask)
		{
			throw new NotImplementedException();
		}

		public override Task<EdoValidationResult> ValidateAsync(EdoTask edoTask, IServiceProvider serviceProvider, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}


	namespace Edo.Problems.Validation.Sources
	{
		public abstract class TransferOrderValidatorBase : EdoTaskProblemValidatorSource
		{
			private readonly EdoTaskValidationContext _serviceProvider;
			private readonly IEnumerable<ITransferOrderValidator> _transferOrderValidators;

			public TransferOrderValidatorBase(
				EdoTaskValidationContext serviceProvider,
				IEnumerable<ITransferOrderValidator> transferOrderValidators)
			{
				_serviceProvider = serviceProvider
					?? throw new ArgumentNullException(nameof(serviceProvider));
				_transferOrderValidators = transferOrderValidators
					?? throw new ArgumentNullException(nameof(transferOrderValidators));
			}

			public override bool IsApplicable(EdoTask edoTask)
			{
				return edoTask is TransferEdoTask;
			}

			public override async Task<EdoValidationResult> ValidateAsync(
				EdoTask edoTask,
				IServiceProvider serviceProvider,
				CancellationToken cancellationToken)
			{
				if(!(edoTask is TransferEdoTask transferOrderTask))
				{
					throw new InvalidOperationException("Проверка может быть выполнена только для TransferEdoTask");
				}

				var transferOrder = GetTransferOrder(transferOrderTask);

				foreach(var transferOrderValidator in _transferOrderValidators)
				{
					if(transferOrderValidator.IsApplicable(transferOrder))
					{
						var validationResult = await transferOrderValidator.Validate(transferOrder);

						if(validationResult.IsFailure)
						{
							return EdoValidationResult.Invalid(this);
						}
					}
				}

				return EdoValidationResult.Valid(this);
			}

			protected virtual TransferOrder GetTransferOrder(TransferEdoTask transferEdoTask)
			{
				var unitOfWorkFactory = _serviceProvider
					.GetRequiredService<IUnitOfWorkFactory>();

				using(var unitOfWork = unitOfWorkFactory.CreateWithoutRoot("OrderEdoValidatorBase"))
				{
					return _serviceProvider
							.GetRequiredService<IGenericRepository<TransferOrder>>()
							.GetFirstOrDefault(unitOfWork, x => x.Id == transferEdoTask.TransferOrderId);
				}
			}
		}
	}

}
