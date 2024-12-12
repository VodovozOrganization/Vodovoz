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
		IList<SourceProductCodeStatus> ProductCodesStatusesToCheckDuplicates { get; }
		Task<Result> IsAllTrueMarkCodesIntroducedAndHasCorrectInns(IEnumerable<TrueMarkWaterIdentificationCode> trueMarkWaterIdentificationCodes, CancellationToken cancellationToken);
		Task<Result> IsTrueMarkCodeIntroducedAndHasCorrectInn(TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode, CancellationToken cancellationToken);
		Result IsTrueMarkWaterIdentificationCodeNotUsed(TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode);
		TrueMarkWaterIdentificationCode LoadOrCreateTrueMarkWaterIdentificationCode(IUnitOfWork uow, string scannedCode);
	}
}
