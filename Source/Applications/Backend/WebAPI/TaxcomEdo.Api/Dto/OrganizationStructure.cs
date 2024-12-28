using System.Xml.Serialization;

namespace TaxcomEdo.Api.Dto
{
	[XmlRoot(ElementName = "OrganizationStructure")]
	[Serializable]
	public class OrganizationStructure
	{
		[XmlElement(ElementName = "RootDepartment", IsNullable = false)]
		public DepartmentStructuredInfo RootDepartment { get; set; }
	}
}
