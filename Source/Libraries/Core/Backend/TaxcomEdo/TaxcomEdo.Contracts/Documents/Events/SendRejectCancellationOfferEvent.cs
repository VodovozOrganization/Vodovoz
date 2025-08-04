using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Documents.Events
{
	/// <summary>
	/// Обертка для xml файла действия на отказ от предложения об аннулировании
	/// </summary>
	[DesignerCategory("code")]
	[XmlRoot("Document", Namespace = "", IsNullable = false)]
	[Serializable]
	public class SendRejectCancellationOfferEvent
	{
		/// <summary>
		/// Идентификатор документооборота, по которому происходит отказ на ПОА
		/// </summary>
		[XmlAttribute]
		public string InternalId { get; set; }

		/// <summary>
		/// Действие(отправка титула покупателя)
		/// </summary>
		[XmlAttribute]
		public string Action { get; set; } = "RejectCancellationOffer";

		/// <summary>
		/// Комментарий, который будет указан в отрицательном ответе на ПОА
		/// </summary>
		public string Comment { get; set; }

		/// <summary>
		/// Подписанты
		/// </summary>
		public Signer[] Signers { get; set; }

		/// <summary>
		/// Опциональная секция, содержащая регистрационный номер ранее 
		/// загруженной в систему доверенности для подписанта
		/// </summary>
		public Warrant[] Warrants { get; set; }
	}
}
