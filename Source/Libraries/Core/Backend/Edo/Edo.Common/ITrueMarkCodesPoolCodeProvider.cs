using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;

namespace Edo.Common
{
	public interface ITrueMarkCodesPoolCodeProvider
	{
		/// <summary>
		/// Забирает из пула первый актуальный валидный код по GTIN.
		/// </summary>
		/// <param name="codesPool">Пул кодов маркировки.</param>
		/// <param name="gtin">GTIN номенклатуры.</param>
		/// <param name="organizationInn">ИНН организации заказа.</param>
		/// <param name="cancellationToken">Токен отмены операции.</param>
		/// <returns>Валидный код маркировки.</returns>
		Task<TrueMarkWaterIdentificationCode> TakeValidCodeAsync(
			ITrueMarkCodesPool codesPool,
			GtinEntity gtin,
			string organizationInn,
			CancellationToken cancellationToken);

		/// <summary>
		/// Забирает из пула первый актуальный валидный код по одному из переданных GTIN.
		/// </summary>
		/// <param name="codesPool">Пул кодов маркировки.</param>
		/// <param name="gtins">Коллекция GTIN'ов для подбора кода.</param>
		/// <param name="organizationInn">ИНН организации заказа.</param>
		/// <param name="cancellationToken">Токен отмены операции.</param>
		/// <returns>Валидный код маркировки.</returns>
		Task<TrueMarkWaterIdentificationCode> TakeValidCodeAsync(
			ITrueMarkCodesPool codesPool,
			IEnumerable<GtinEntity> gtins,
			string organizationInn,
			CancellationToken cancellationToken);
	}
}
