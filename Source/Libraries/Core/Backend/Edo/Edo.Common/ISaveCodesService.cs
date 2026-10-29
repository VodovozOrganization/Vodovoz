using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Edo.Common
{
	public interface ISaveCodesService
	{
		Task SaveCodesToPool(SaveCodesEdoTask edoTask, CancellationToken cancellationToken);
		Task SaveCodesToPool(ReceiptEdoTask receiptEdoTask, CancellationToken cancellationToken);

		/// <summary>
		/// Сохраняет код ЧЗ товара в пул кодов
		/// </summary>
		/// <param name="productCode">Код ЧЗ товара</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task SaveCodeToPool(TrueMarkProductCode productCode, CancellationToken cancellationToken);
	}
}
