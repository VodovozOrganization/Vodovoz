using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Validation.Sources
{
	public class ClientWithoutOgrnOrOgrnDateValidator : EdoTaskProblemValidatorSource
	{
		public override string Name => "Client.WithoutOGRNIPOrOGRNIPDate";
		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
		public override string Message => "У участника документооборота с формой собственности ИП должно быть заполнено ОГРНИП и его дата";
		public override string Description => "Проверяет целостность данных для ИП";
		public override string Recommendation => "Проверьте ОГРНИП и его дату у продавца и покупателя, если они ИП";

		public override bool IsApplicable(EdoTask edoTask)
		{
			//пока отключаем, но оставляю, если вдруг налоговая передумает и ОГРНИП и его дата станут обязательными
			return false; //edoTask is DocumentEdoTask docTask && docTask.DocumentType == EdoDocumentType.UPD;
		}

		public override string GetTemplatedMessage(EdoTask edoTask)
		{
			var edoRequest = GetEdoRequest(edoTask);

			if(edoRequest == null)
			{
				return Message;
			}

			var client = edoRequest.Order.Client;
			var organization = edoRequest.Order.Contract.Organization;

			if(client is null || organization is null)
			{
				return "Что-то пошло не так, клиент или организация, от которой идет продажа не должны быть null";
			}

			if(client.IsPrivateBusinessmanWithoutOgrnOrOgrnDate())
			{
				return $"У клиента с идентификатором {client.Id} должно быть заполнено ОГРНИП и его дата";
			}

			if(organization.IsPrivateBusinessmanWithoutOgrnOrOgrnDate())
			{
				return $"У организации с идентификатором {organization.Id} должно быть заполнено ОГРНИП и его дата";
			}

			return "Что-то пошло не так, мы не должны были попасть сюда";
		}

		public override Task<EdoValidationResult> ValidateAsync(
			EdoTask edoTask,
			IServiceProvider serviceProvider,
			CancellationToken cancellationToken)
		{
			var edoRequest = GetEdoRequest(edoTask);

			if(edoRequest is null)
			{
				return Task.FromResult(EdoValidationResult.Invalid(this));
			}
			
			var client = edoRequest.Order.Client;
			var organization = edoRequest.Order.Contract.Organization;
			
			if(client is null
				|| organization is null
				|| client.IsPrivateBusinessmanWithoutOgrnOrOgrnDate()
				|| organization.IsPrivateBusinessmanWithoutOgrnOrOgrnDate())
			{
				return Task.FromResult(EdoValidationResult.Invalid(this));
			}

			return Task.FromResult(EdoValidationResult.Valid(this));
		}

		private FormalEdoRequest GetEdoRequest(EdoTask edoTask)
		{
			return ((OrderEdoTask)edoTask).FormalEdoRequest;
		}
	}
}
