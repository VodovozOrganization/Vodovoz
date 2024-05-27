using System.Collections.Generic;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using Vodovoz.Core.Data.Goods;
using Vodovoz.Core.Data.Orders;

namespace TaxcomEdoApi.Converters
{
	public interface IUpdProductConverter
	{
		FajlDokumentTablSchFaktSvedTov[] ConvertOrderItemsToUpdProducts(
			IList<OrderItem> orderItems, IEnumerable<SpecialNomenclature> counterpartySpecialNomenclatures);
	}
}
