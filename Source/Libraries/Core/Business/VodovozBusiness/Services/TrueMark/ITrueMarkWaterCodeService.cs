using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Errors;

namespace VodovozBusiness.Services.TrueMark
{
	public interface ITrueMarkWaterCodeService
	{
		IList<SourceProductCodeStatus> SuccessfullyUsedProductCodesStatuses { get; }

		Result<TrueMarkAnyCode> TryGetSavedTrueMarkCodeByScannedCode(IUnitOfWork uow, string scannedCode);
		Task<Result<TrueMarkAnyCode>> GetTrueMarkCodeByScannedCode(IUnitOfWork uow, string scannedCode, CancellationToken cancellationToken = default);

		Task<Result> IsAllTrueMarkCodesIntroducedAndHasCorrectInns(IEnumerable<TrueMarkWaterIdentificationCode> trueMarkWaterIdentificationCodes, CancellationToken cancellationToken);
		Task<Result> IsTrueMarkCodeIntroducedAndHasCorrectInn(TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode, CancellationToken cancellationToken);
		Result IsTrueMarkWaterIdentificationCodeNotUsed(TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode);
		TrueMarkWaterIdentificationCode LoadOrCreateTrueMarkWaterIdentificationCode(IUnitOfWork uow, string scannedCode);
	}
}
