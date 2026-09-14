using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace VodovozBusiness.Services.TrueMark
{
	public class TrueMarkCodesPoolCleanupService : ITrueMarkCodesPoolCleanupService
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ITrueMarkCodesPoolManager _trueMarkCodesPoolManager;

		public TrueMarkCodesPoolCleanupService(
			IUnitOfWorkFactory unitOfWorkFactory,
			ITrueMarkCodesPoolManager trueMarkCodesPoolManager)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_trueMarkCodesPoolManager = trueMarkCodesPoolManager
				?? throw new ArgumentNullException(nameof(trueMarkCodesPoolManager));
		}

		public async Task RemoveStagingCodeFromPoolIfPresentAsync(
			StagingTrueMarkCode stagingTrueMarkCode,
			CancellationToken cancellationToken = default)
		{
			if(stagingTrueMarkCode is null)
			{
				throw new ArgumentNullException(nameof(stagingTrueMarkCode));
			}

			var codeIds = new List<int>();

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(TrueMarkCodesPoolCleanupService)))
			{
				foreach(var identificationCode in stagingTrueMarkCode.AllIdentificationCodes)
				{
					if(string.IsNullOrWhiteSpace(identificationCode.Gtin)
						|| string.IsNullOrWhiteSpace(identificationCode.SerialNumber))
					{
						continue;
					}

					var foundIds = await uow.Session.QueryOver<TrueMarkWaterIdentificationCode>()
						.Where(x => x.Gtin == identificationCode.Gtin)
						.Where(x => x.SerialNumber == identificationCode.SerialNumber)
						.Where(x => !x.IsInvalid)
						.Select(x => x.Id)
						.ListAsync<int>(cancellationToken);

					codeIds.AddRange(foundIds);
				}
			}

			await RemoveIdentificationCodesFromPoolIfPresentAsync(codeIds, cancellationToken);
		}

		public async Task RemoveIdentificationCodesFromPoolIfPresentAsync(
			IEnumerable<int> identificationCodeIds,
			CancellationToken cancellationToken = default)
		{
			if(identificationCodeIds is null)
			{
				throw new ArgumentNullException(nameof(identificationCodeIds));
			}

			var codeIds = identificationCodeIds.Where(id => id > 0).Distinct().ToList();

			if(!codeIds.Any())
			{
				return;
			}

			await _trueMarkCodesPoolManager.DeleteCodesAsync(codeIds, cancellationToken);
		}

		public async Task RemoveTrueMarkAnyCodeFromPoolIfPresentAsync(
			TrueMarkAnyCode trueMarkAnyCode,
			CancellationToken cancellationToken = default)
		{
			if(trueMarkAnyCode is null)
			{
				throw new ArgumentNullException(nameof(trueMarkAnyCode));
			}

			IEnumerable<TrueMarkAnyCode> allCodes = trueMarkAnyCode.Match(
				transportCode => transportCode.GetAllCodes(),
				groupCode => groupCode.GetAllCodes(),
				waterCode => new TrueMarkAnyCode[] { waterCode });

			var identificationCodeIds = allCodes
				.Where(code => code.IsTrueMarkWaterIdentificationCode)
				.Select(code => code.TrueMarkWaterIdentificationCode.Id)
				.Where(id => id > 0)
				.Distinct()
				.ToList();

			await RemoveIdentificationCodesFromPoolIfPresentAsync(identificationCodeIds, cancellationToken);
		}
	}
}
