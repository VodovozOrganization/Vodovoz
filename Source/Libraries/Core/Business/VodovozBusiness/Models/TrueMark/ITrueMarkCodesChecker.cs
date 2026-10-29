using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Models.TrueMark;

namespace VodovozBusiness.Models.TrueMark
{
	public interface ITrueMarkCodesChecker
	{
		/// <summary>
		/// Проверить коды (старая реализация)
		/// </summary>
		/// <param name="codes">Коллекция кодов</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Коллекция результатов проверки</returns>
		Task<IEnumerable<TrueMarkCheckResult>> Check(IEnumerable<TrueMarkWaterIdentificationCode> codes, CancellationToken cancellationToken);

		/// <summary>
		/// Проверить коды 
		/// </summary>
		/// <param name="codes">Коллекция кодов</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Коллекция результатов проверки</returns>
		Task<IDictionary<TrueMarkWaterIdentificationCode, ProductInstanceStatus>> CheckCodes(IEnumerable<TrueMarkWaterIdentificationCode> codes, CancellationToken cancellationToken);
	}
}
