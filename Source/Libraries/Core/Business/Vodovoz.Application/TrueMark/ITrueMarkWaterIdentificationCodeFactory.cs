using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Models.TrueMark;

namespace Vodovoz.Application.TrueMark
{
	public interface ITrueMarkWaterIdentificationCodeFactory
	{
		TrueMarkWaterIdentificationCode CreateFromParsedCode(TrueMarkWaterCode parsedCode);
		TrueMarkWaterIdentificationCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus);

		/// <summary>
		/// Создает код Честного Знака экземпляра из кода ЧЗ для промежуточного хранения
		/// </summary>
		/// <param name="stagingCode">Код ЧЗ для промежуточного хранения</param>
		/// <returns>Код ЧЗ экземпляра</returns>
		TrueMarkWaterIdentificationCode CreateFromStagingCode(StagingTrueMarkCode stagingCode);
	}
}
