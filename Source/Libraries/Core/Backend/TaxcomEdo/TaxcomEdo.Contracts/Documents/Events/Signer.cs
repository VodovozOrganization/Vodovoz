using System;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Documents.Events
{
	/// <summary>
	/// Подписант
	/// </summary>
	[XmlType(AnonymousType = true)]
	[Serializable]
	public class Signer
	{
		/// <summary>
		/// Данные о подписанте
		/// </summary>
		[XmlElement("Person", typeof (SignerPerson), Form = XmlSchemaForm.Unqualified)]
		[XmlElement("Certificate", typeof (SignerCertificate), Form = XmlSchemaForm.Unqualified)]
		public object Item { get; set; }
	}
}
