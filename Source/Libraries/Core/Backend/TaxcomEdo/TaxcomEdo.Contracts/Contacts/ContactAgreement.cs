using System;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Contacts
{
	[XmlRoot(ElementName = "Agreement")]
	[Serializable]
	public class ContactAgreement
	{
		[XmlAttribute(AttributeName = "Number")]
		public string AgreementNumber { get; set; }
	}
}
