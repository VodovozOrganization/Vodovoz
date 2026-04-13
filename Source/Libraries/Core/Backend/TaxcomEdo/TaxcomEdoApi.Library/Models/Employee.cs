using System;
using System.Collections.Generic;

namespace TaxcomEdoApi.Library.Models
{
	public class Employee
	{
		public Guid Id { get; set; }

		public string Name { get; set; }

		public Department Department { get; set; }

		public EmployeePermissionData Permissions { get; set; }

		public IEnumerable<WarrantMeta> Warrants { get; set; }
	}
}
