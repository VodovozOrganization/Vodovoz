using System;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Contacts
{
	[Serializable]
	public class PersonName
	{
		[XmlAttribute(AttributeName = "LastName")]
		public string LastName { get; set; }

		[XmlAttribute(AttributeName = "FirstName")]
		public string FirstName { get; set; }

		[XmlAttribute(AttributeName = "MiddleName")]
		public string MiddleName { get; set; }
	}
}
