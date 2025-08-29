using System;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.Contacts
{
	[Serializable]
	public class DepartmentStructuredInfo
	{
		[XmlArrayItem("Department", IsNullable = false)]
		public DepartmentStructuredInfo[] SubDepartments { get; set; }

		[XmlArrayItem("Employee", IsNullable = false)]
		public EmployeeShortInfo[] Employees { get; set; }

		[XmlAttribute(AttributeName = "ID")]
		public string ID { get; set; }

		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }
	}
}
