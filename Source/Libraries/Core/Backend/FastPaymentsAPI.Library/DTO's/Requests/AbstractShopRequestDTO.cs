using System.Xml.Serialization;

namespace FastPaymentsAPI.Library.DTO_s.Requests
{
	public abstract class AbstractShopRequestDTO
	{
		[XmlElement(ElementName = "shop_id")]
		public long ShopId { get; set; }

		[XmlElement(ElementName = "shop_passwd")]
		public string ShopPasswd { get; set; }
	}
}
