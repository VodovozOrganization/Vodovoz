using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Documents.Events
{
	/// <summary>
	/// Обертка для xml файла действия на отправку титула покупателя для подписания входящего документа
	/// </summary>
	[DesignerCategory("code")]
	[XmlRoot("Document", Namespace = "", IsNullable = false)]
	[Serializable]
	public class SendCustomerInformationEvent
	{
		/// <summary>
		/// Id документооборота, который принимается
		/// </summary>
		[XmlAttribute]
		public string InternalId { get; set; }
		/// <summary>
		/// Действие(отправка титула покупателя)
		/// </summary>
		[XmlAttribute]
		public string Action { get; set; } = "SendCustomerInformation";
		/// <summary>
		/// Дополнительные параметры
		/// </summary>
		public AdditionalParameter[] AdditionalData { get; set; }
		/// <summary>
		/// Подписанты
		/// </summary>
		public Signer[] Signers { get; set; }
	}
}
