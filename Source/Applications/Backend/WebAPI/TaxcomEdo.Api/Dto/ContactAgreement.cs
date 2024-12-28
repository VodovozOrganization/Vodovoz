using System.Xml.Serialization;

namespace TaxcomEdo.Api.Dto
{
	[XmlRoot(ElementName = "Agreement")]
	[Serializable]
	public class ContactAgreement
	{
		[XmlAttribute(AttributeName = "Number")]
		public string AgreementNumber { get; set; }
	}
}
