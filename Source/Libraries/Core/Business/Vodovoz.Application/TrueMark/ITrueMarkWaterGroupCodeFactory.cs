using TrueMark.Contracts;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.Application.TrueMark
{
	public interface ITrueMarkWaterGroupCodeFactory
	{
		TrueMarkWaterGroupCode CreateFromParsedCode(TrueMarkWaterCode parsedCode);
		TrueMarkWaterGroupCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus);
	}
}
