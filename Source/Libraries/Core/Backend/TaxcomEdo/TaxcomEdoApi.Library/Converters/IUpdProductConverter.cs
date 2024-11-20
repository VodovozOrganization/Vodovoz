using System.Collections.Generic;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;
using TaxcomEdo.Contracts.Goods;
using TaxcomEdo.Contracts.Orders;

namespace TaxcomEdoApi.Library.Converters
{
	public interface IUpdProductConverter
	{
		FajlDokumentTablSchFaktSvedTov[] ConvertOrderItemsToUpdProducts(
			IList<OrderItemInfoForEdo> orderItems, IEnumerable<SpecialNomenclatureInfoForEdo> counterpartySpecialNomenclatures);
	}
}
