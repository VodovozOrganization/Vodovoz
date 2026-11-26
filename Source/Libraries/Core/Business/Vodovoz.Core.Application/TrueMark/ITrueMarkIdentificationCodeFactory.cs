using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Interfaces.TrueMark;

namespace Vodovoz.Core.Application.TrueMark
{
	public interface ITrueMarkIdentificationCodeFactory
	{
		/// <summary>
		/// Создает код Честного Знака экземпляра из распарсенного кода маркировки
		/// </summary>
		/// <param name="parsedCode">Распарсенный код маркировки (<see cref="ITrueMarkCode"/>)</param>
		/// <returns>Код Честного Знака экземпляра</returns>
		TrueMarkIdentificationCode CreateFromParsedCode(ITrueMarkCode parsedCode);

		/// <summary>
		/// Создает код Честного Знака экземпляра из статуса экземпляра, полученного от API Честного Знака
		/// </summary>
		/// <param name="productInstanceStatus">Статус экземпляра продукта из ЧЗ</param>
		/// <returns>Код Честного Знака экземпляра</returns>
		TrueMarkIdentificationCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus);

		/// <summary>
		/// Создает код Честного Знака экземпляра из кода ЧЗ для промежуточного хранения
		/// </summary>
		/// <param name="stagingCode">Код ЧЗ для промежуточного хранения</param>
		/// <returns>Код ЧЗ экземпляра</returns>
		TrueMarkIdentificationCode CreateFromStagingCode(StagingTrueMarkCode stagingCode);
	}
}
