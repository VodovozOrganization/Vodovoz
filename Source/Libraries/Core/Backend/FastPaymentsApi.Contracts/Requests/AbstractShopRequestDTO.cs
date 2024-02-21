using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	public abstract class AbstractShopRequestDTO
	{
		[XmlElement(ElementName = "shop_id")]
		public long ShopId { get; set; }

		[XmlElement(ElementName = "shop_passwd")]
		public string ShopPasswd { get; set; }
	}
}
