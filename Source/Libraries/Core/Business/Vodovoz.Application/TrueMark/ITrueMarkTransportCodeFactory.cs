using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Application.TrueMark
{
	public interface ITrueMarkTransportCodeFactory
	{
		TrueMarkTransportCode CreateFromRawCode(string scannedCode);
		TrueMarkTransportCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus);

		/// <summary>
		/// Создает транспортный код Честного Знака из кода ЧЗ для промежуточного хранения
		/// </summary>
		/// <param name="stagingCode">Код ЧЗ для промежуточного хранения</param>
		/// <returns>Транспортный код ЧЗ</returns>
		TrueMarkTransportCode CreateFromStagingCode(StagingTrueMarkCode stagingCode);
	}
}
