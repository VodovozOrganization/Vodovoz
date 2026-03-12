using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.Application.TrueMark
{
	public interface ITrueMarkWaterGroupCodeFactory
	{
		TrueMarkWaterGroupCode CreateFromParsedCode(TrueMarkWaterCode parsedCode);
		TrueMarkWaterGroupCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus);

		/// <summary>
		/// Создает групповой код Честного Знака из кода ЧЗ для промежуточного хранения
		/// </summary>
		/// <param name="stagingCode">Код ЧЗ для промежуточного хранения</param>
		/// <returns>Групповой код ЧЗ</returns>
		TrueMarkWaterGroupCode CreateFromStagingCode(StagingTrueMarkCode stagingCode);
	}
}
