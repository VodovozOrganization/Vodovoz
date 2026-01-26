using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	/// <summary>
	/// Учетные данные магазина
	/// </summary>
	public abstract class AbstractShopRequestDTO
	{
		/// <summary>
		/// Id магазина
		/// </summary>
		[XmlElement(ElementName = "shop_id")]
		public long ShopId { get; set; }
		/// <summary>
		/// Пароль магазина
		/// </summary>
		[XmlElement(ElementName = "shop_passwd")]
		public string ShopPasswd { get; set; }
	}
}
