using System.Runtime.Serialization;
using Vodovoz.Domain.Goods;

namespace VodovozMobileService.DTO
{
	[DataContract]
	public class NomenclaturePriceDTO
	{
		[DataMember]
		public int MinCount { get; private set; }

		[DataMember]
		public decimal Price { get; private set; }

		public NomenclaturePriceDTO(int min, decimal price)
		{
			MinCount = min;
			Price = price;
		}

		public NomenclaturePriceDTO(NomenclaturePrice price)
		{
			MinCount = price.MinCount;
			Price = price.Price;
		}
	}
}
