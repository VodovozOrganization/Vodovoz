using Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMarkApi.Client;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Common
{
	public class TrueMarkTaskCodesValidator : ITrueMarkCodesValidator
	{
		private readonly IEdoRepository _edoRepository;
		private ITrueMarkApiClient _trueMarkApiClient;

		public TrueMarkTaskCodesValidator(IEdoRepository edoRepository, ITrueMarkApiClient trueMarkApiClient)
		{
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_trueMarkApiClient = trueMarkApiClient ?? throw new ArgumentNullException(nameof(trueMarkApiClient));
		}
		
		private TrueMarkCodeValidationResult FillCodeValidateResult(
			TrueMarkCodeValidationResult codeCalidationResult,
			ProductInstanceStatus productInstanceStatus,
			TrueMarkWaterIdentificationCode code,
			IEnumerable<string> ourGtinNumbers,
			IEnumerable<string> ourOrganizationInns,
			string sellerInn
		)
		{
			// проверка на наш GTIN
			if(productInstanceStatus.Gtin.IsNotIn(ourGtinNumbers))
			{
				codeCalidationResult.IsOurGtin = false;
				codeCalidationResult.IsValid = false;
				codeCalidationResult.ReadyToSell = false;
			}
		
			// проверка на нашу организацию
			if(productInstanceStatus.OwnerInn.IsNotIn(ourOrganizationInns))
			{
				codeCalidationResult.IsOwnedByOurOrganization = false;
				codeCalidationResult.IsValid = false;
				codeCalidationResult.ReadyToSell = false;
			}
		
			// проверка на то что код в обороте
			if(productInstanceStatus.Status != ProductInstanceStatusEnum.Introduced)
			{
				codeCalidationResult.IsIntroduced = false;
				codeCalidationResult.IsValid = false;
				codeCalidationResult.ReadyToSell = false;
			}
		
			// проверка на то что код на балансе продавца
			if(productInstanceStatus.OwnerInn != sellerInn)
			{
				// не влияет на валидность, просто дополнительная информация
				codeCalidationResult.IsOwnedBySeller = false;
				codeCalidationResult.ReadyToSell = false;
			}
			
			return codeCalidationResult;
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
				
				var codeResult = new TrueMarkCodeValidationResult(code, taskItem);
				
				FillCodeValidateResult(codeResult, checkResult.ProductInstanceStatus, code, gtinNumbers, ourOrganizationInns, sellerInn);

				codeResults.Add(codeResult);
			}

			return new TrueMarkTaskValidationResult(codeResults);
		}
		
		public async Task<TrueMarkTaskValidationResult> ValidateAsync(IEnumerable<TrueMarkWaterIdentificationCode> codes, string organizationInn, CancellationToken cancellationToken)
		{
			var gtins = await _edoRepository.GetGtinsAsync(cancellationToken);
			var gtinNumbers = gtins.Select(x => x.GtinNumber);

			var edoOrganizations = await _edoRepository.GetEdoOrganizationsAsync(cancellationToken);
			var ourOrganizationInns = edoOrganizations.Select(x => x.INN);

			var productInstancesInfo = await _trueMarkApiClient.GetProductInstanceInfoAsync(codes.Select(x=>x.IdentificationCode), cancellationToken);
			
			if(productInstancesInfo is null)
			{
				return new TrueMarkTaskValidationResult(null);
			}
			
			var codeResults = new List<TrueMarkCodeValidationResult>();
			
			foreach(var productInstanceStatus in productInstancesInfo.InstanceStatuses)
			{
				var code = codes.Where(x=>x.IdentificationCode == productInstanceStatus.IdentificationCode).First();
				
				var codeResult = new TrueMarkCodeValidationResult(code);

				FillCodeValidateResult(codeResult, productInstanceStatus, code, gtinNumbers, ourOrganizationInns, organizationInn);
		
				codeResults.Add(codeResult);
			}
		
			return new TrueMarkTaskValidationResult(codeResults);
		}
	}
}
