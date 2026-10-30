using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;

namespace WarehouseApi.Library.Services
{
	public interface IWarehouseTrueMarkCodePreloader
	{
		Task PreloadCarLoadDocumentItems(
			IEnumerable<CarLoadDocumentItem> documentItems,
			CancellationToken cancellationToken);

		Task PreloadSelfDelivery(
			Order order,
			SelfDeliveryDocument selfDeliveryDocument,
			CancellationToken cancellationToken);

		Task PreloadProductCodes(
			IEnumerable<TrueMarkProductCode> productCodes,
			CancellationToken cancellationToken);

		Task PreloadCodes(
			IEnumerable<TrueMarkWaterIdentificationCode> codes,
			CancellationToken cancellationToken);

		TrueMarkAnyCode GetParentGroupCode(TrueMarkAnyCode trueMarkAnyCode);

		IEnumerable<TrueMarkAnyCode> GetAllCodes(TrueMarkAnyCode parentCode);
	}
}