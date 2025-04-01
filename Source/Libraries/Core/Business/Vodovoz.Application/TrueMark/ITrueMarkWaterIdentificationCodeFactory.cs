using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.Application.TrueMark
{
	public interface ITrueMarkWaterIdentificationCodeFactory
	{
		TrueMarkWaterIdentificationCode CreateFromParsedCode(TrueMarkWaterCode parsedCode);
		TrueMarkWaterIdentificationCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus);
	}
}
