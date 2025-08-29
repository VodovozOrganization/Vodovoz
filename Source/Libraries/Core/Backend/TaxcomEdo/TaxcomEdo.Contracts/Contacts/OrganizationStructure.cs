using System;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Contacts
{
	[XmlRoot(ElementName = "OrganizationStructure")]
	[Serializable]
	public class OrganizationStructure
	{
		[XmlElement(ElementName = "RootDepartment", IsNullable = false)]
		public DepartmentStructuredInfo RootDepartment { get; set; }
	}
}
