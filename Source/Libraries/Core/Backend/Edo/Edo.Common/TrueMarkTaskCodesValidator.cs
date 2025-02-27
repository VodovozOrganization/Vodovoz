using Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Common
{
	public class TrueMarkTaskCodesValidator
	{
		private readonly IEdoRepository _edoRepository;

		public TrueMarkTaskCodesValidator(IEdoRepository edoRepository)
		{
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
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
				var codeResult = new TrueMarkCodeValidationResult(code, taskItem);

				if(!checkResults.TryGetValue(code.IdentificationCode, out var checkResult))
				{
					throw new InvalidOperationException($"Не найден код {code.IdentificationCode} " +
						$"из задачи {edoTask.Id} в результатах проверки из ЧЗ.");
				}

				// проверка на наш GTIN
				if(code.GTIN.IsNotIn(gtinNumbers))
				{
					codeResult.IsOurGtin = false;
					codeResult.IsValid = false;
					codeResult.ReadyToSell = false;
				}

				// проверка на нашу организацию
				if(checkResult.ProductInstanceStatus.OwnerInn.IsNotIn(ourOrganizationInns))
				{
					codeResult.IsOwnedByOurOrganization = false;
					codeResult.IsValid = false;
					codeResult.ReadyToSell = false;
				}

				// проверка на то что код в обороте
				if(checkResult.ProductInstanceStatus.Status != ProductInstanceStatusEnum.Introduced)
				{
					codeResult.IsIntroduced = false;
					codeResult.IsValid = false;
					codeResult.ReadyToSell = false;
				}

				// проверка на то что код на балансе продавца
				if(checkResult.ProductInstanceStatus.OwnerInn != sellerInn)
				{
					// не влияет на валидность, просто дополнительная информация
					codeResult.IsOwnedBySeller = false;
					codeResult.ReadyToSell = false;
				}

				codeResults.Add(codeResult);
			}

			return new TrueMarkTaskValidationResult(codeResults);
		}
	}
}
