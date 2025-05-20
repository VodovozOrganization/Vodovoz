using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Repositories;

namespace Edo.Problems.Validation.Sources
{
	public abstract class TransferOrderValidatorBase : EdoTaskProblemValidatorSource
	{
		protected TransferOrder _transferOrder;
		protected bool _transferOrderWasSet;

		public override bool IsApplicable(EdoTask edoTask)
		{
			return edoTask is TransferEdoTask;
		}

		public void SetTransferOrder(TransferOrder transferOrder)
		{
			_transferOrder = transferOrder;
			_transferOrderWasSet = true;
		}

		protected virtual TransferOrder GetTransferOrder(TransferEdoTask transferEdoTask, IServiceProvider serviceProvider = default)
		{
			if(_transferOrderWasSet)
			{
				return _transferOrder;
			}

			if(serviceProvider is null)
			{
				throw new InvalidOperationException("Не был установлен TransferOrder и не был передан контейнер зависимостей");
			}

			var unitOfWorkFactory = serviceProvider.GetRequiredService<IUnitOfWorkFactory>();

			using(var unitOfWork = unitOfWorkFactory.CreateWithoutRoot("OrderEdoValidatorBase"))
			{
				return serviceProvider
						.GetRequiredService<IGenericRepository<TransferOrder>>()
						.GetFirstOrDefault(unitOfWork, x => x.Id == transferEdoTask.TransferOrderId);
			}
		}
	}
}
