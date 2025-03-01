using Edo.Common;
using Edo.Problems.Validation;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Organizations;

namespace Edo.Problems.Validation.Sources
{
	public class CodeTransferedEdoValidator : EdoTaskProblemValidatorSource, IEdoTaskValidator
	{
		public override string Name
		{
			get => "Transfer.CodesTransfered";
		}

		public override EdoProblemImportance Importance
		{
			get => EdoProblemImportance.Waiting;
		}

		public override string Message
		{
			get => "Перемещение кодов маркировки не завершено на стороне честного знака";
		}

		public override string Description
		{
			get => "Проверяет что все коды для трансфера поменяли приадлежность в честном знаке";
		}
		public override string Recommendation
		{
			get => "Подождать перемещения кодов.";
		}

		public string GetTemplateMessage(EdoTask edoTask)
		{
			return $"Перемещение кодов маркировки для трансфера №{edoTask.Id} не завершено на стороне честного знака";
		}

		public override bool IsApplicable(EdoTask edoTask)
		{
			var transferTask = edoTask as TransferEdoTask;
			if(transferTask == null)
			{
				return false;
			}

			return transferTask.TransferStatus == TransferEdoTaskStatus.InProgress;
		}

		public override async Task<bool> NotValidCondition(EdoTask edoTask, IServiceProvider serviceProvider, CancellationToken cancellationToken)
		{
			var trueMarkStatusProvider = serviceProvider.GetRequiredService<EdoTaskItemTrueMarkStatusProvider>();

			var codesResults = await trueMarkStatusProvider.GetItemsStatusesAsync(cancellationToken);

			var transferTask = edoTask as TransferEdoTask;

			var uowFactory = serviceProvider.GetRequiredService<IUnitOfWorkFactory>();
			using(var uow = uowFactory.CreateWithoutRoot())
			{
				var toOrg = uow.GetById<OrganizationEntity>(transferTask.ToOrganizationId);
				var allTransfered = codesResults.All(x => x.Value.ProductInstanceStatus.OwnerInn == toOrg.INN);
				return !allTransfered;
			}
		}
	}
}
