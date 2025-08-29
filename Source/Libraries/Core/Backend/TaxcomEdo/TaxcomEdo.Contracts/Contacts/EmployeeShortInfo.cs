using System;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Contacts
{
	[Serializable]
	public class EmployeeShortInfo
	{
		[XmlElement("Name")]
		public PersonName Name { get; set; }

		[XmlAttribute(AttributeName = "ID")]
		public string ID { get; set; }

		[XmlElement("Position")]
		public string Position { get; set; }
	}
}
