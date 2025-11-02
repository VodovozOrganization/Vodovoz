using System;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Documents.Events
{
	/// <summary>
	/// Данные сертификата подписанта
	/// </summary>
	[XmlType(AnonymousType = true)]
	[Serializable]
	public class SignerCertificate
	{
		/// <summary>
		/// Отпечаток
		/// </summary>
		[XmlAttribute]
		public string Thumbprint { get; set; }
		/// <summary>
		/// Серийный номер
		/// </summary>
		[XmlAttribute]
		public string SerialNumber { get; set; }
	}
}
