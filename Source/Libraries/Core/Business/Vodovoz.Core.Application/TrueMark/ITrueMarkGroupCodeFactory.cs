using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Interfaces.TrueMark;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Application.TrueMark
{
	public interface ITrueMarkGroupCodeFactory
	{
		/// <summary>
		/// Создает групповой код Честного Знака из распарсенного кода маркировки
		/// </summary>
		/// <param name="parsedCode">Распарсенный код маркировки (<see cref="ITrueMarkCode"/>)</param>
		/// <returns>Групповой код Честного Знака</returns>
		TrueMarkGroupCode CreateFromParsedCode(ITrueMarkCode parsedCode);

		/// <summary>
		/// Создает групповой код Честного Знака из статуса экземпляра, полученного от API Честного Знака
		/// </summary>
		/// <param name="productInstanceStatus">Статус экземпляра продукта из ЧЗ</param>
		/// <returns>Групповой код Честного Знака</returns>
		TrueMarkGroupCode CreateFromProductInstanceStatus(ProductInstanceStatus productInstanceStatus);

		/// <summary>
		/// Создает групповой код Честного Знака из кода ЧЗ для промежуточного хранения
		/// </summary>
		/// <param name="stagingCode">Код ЧЗ для промежуточного хранения</param>
		/// <returns>Групповой код ЧЗ</returns>
		TrueMarkGroupCode CreateFromStagingCode(StagingTrueMarkCode stagingCode);
	}
}
