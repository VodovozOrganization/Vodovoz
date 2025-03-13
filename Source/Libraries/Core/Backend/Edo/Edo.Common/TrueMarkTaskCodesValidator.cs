using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;
using TrueMark.Contracts;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Common
{
	public class TrueMarkTaskCodesValidator : ITrueMarkCodesValidator
	{
		private readonly IEdoRepository _edoRepository;

		public TrueMarkTaskCodesValidator(IEdoRepository edoRepository)
		{
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
		}

		private TrueMarkCodeValidationResult CreateCodeValidationResult(
			ProductInstanceStatus productInstanceStatus,
			IEnumerable<string> ourGtinNumbers,
			IEnumerable<string> ourOrganizationInns,
			string sellerInn
		)
		{
			var codeValidationResult = new TrueMarkCodeValidationResult();
			
			codeValidationResult.IdentificationCode = productInstanceStatus.IdentificationCode;

			// проверка на наш GTIN
			if(productInstanceStatus.Gtin.IsNotIn(ourGtinNumbers))
			{
				codeValidationResult.IsOurGtin = false;
				codeValidationResult.IsValid = false;
				codeValidationResult.ReadyToSell = false;
			}

			// проверка на нашу организацию
			if(productInstanceStatus.OwnerInn.IsNotIn(ourOrganizationInns))
			{
				codeValidationResult.IsOwnedByOurOrganization = false;
				codeValidationResult.IsValid = false;
				codeValidationResult.ReadyToSell = false;
			}

			// проверка на то что код в обороте
			if(productInstanceStatus.Status != ProductInstanceStatusEnum.Introduced)
			{
				codeValidationResult.IsIntroduced = false;
				codeValidationResult.IsValid = false;
				codeValidationResult.ReadyToSell = false;
			}

			// проверка на то что код на балансе продавца
			if(productInstanceStatus.OwnerInn != sellerInn)
			{
				// не влияет на валидность, просто дополнительная информация
				codeValidationResult.IsOwnedBySeller = false;
				codeValidationResult.ReadyToSell = false;
			}

			return codeValidationResult;
		}

		public async Task<TrueMarkTaskValidationResult> ValidateAsync(
			OrderEdoTask edoTask,
			EdoTaskItemTrueMarkStatusProvider edoTaskItemTrueMarkStatusProvider,
			CancellationToken cancellationToken)
		{
			var gtins = await _edoRepository.GetGtinsAsync(cancellationToken);
			var gtinNumbers = gtins.Select(x => x.GtinNumber);

			var edoOrganizations = await _edoRepository.GetEdoOrganizationsAsync(cancellationToken);
			var ourOrganizationInns = edoOrganizations.Select(x => x.INN);
			var sellerInn = edoTask.OrderEdoRequest.Order.Contract.Organization.INN;

			var checkResults = await edoTaskItemTrueMarkStatusProvider.GetItemsStatusesAsync(cancellationToken);
			var codeResults = new List<TrueMarkCodeValidationResult>();

			foreach(var taskItem in edoTask.Items)
			{
				var code = taskItem.ProductCode.ResultCode;

				if(!checkResults.TryGetValue(code.IdentificationCode, out var checkResult))
				{
					throw new InvalidOperationException($"Не найден код {code.IdentificationCode} " +
					                                    $"из задачи {edoTask.Id} в результатах проверки из ЧЗ.");
				}

				var codeResult = CreateCodeValidationResult(checkResult.ProductInstanceStatus, gtinNumbers, ourOrganizationInns, sellerInn);
				codeResult.Code = code;
				codeResult.EdoTaskItem = taskItem;

				codeResults.Add(codeResult);
			}

			return new TrueMarkTaskValidationResult(codeResults);
		}

		public async Task<IEnumerable<TrueMarkCodeValidationResult>> ValidateAsync(
			IEnumerable<ProductInstanceStatus> productInstanceStatuses,
			string organizationInn,
			CancellationToken cancellationToken)
		{
			var gtins = await _edoRepository.GetGtinsAsync(cancellationToken);
			var gtinNumbers = gtins.Select(x => x.GtinNumber);

			var edoOrganizations = await _edoRepository.GetEdoOrganizationsAsync(cancellationToken);
			var ourOrganizationInns = edoOrganizations.Select(x => x.INN);

			var codeResults =  new List<TrueMarkCodeValidationResult>();
			
			foreach(var productInstanceStatus in productInstanceStatuses)
			{
				var codeResult = CreateCodeValidationResult(productInstanceStatus, gtinNumbers, ourOrganizationInns, organizationInn);
				
				codeResults.Add(codeResult);
			}
			
			return codeResults;
			

			
		}
	}
}
