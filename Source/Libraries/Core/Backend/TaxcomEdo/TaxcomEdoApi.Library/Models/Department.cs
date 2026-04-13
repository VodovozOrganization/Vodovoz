using System;
using System.Collections.Generic;

namespace TaxcomEdoApi.Library.Models
{
	public class Department
	{
		public Department()
		{
			SubDepartments = new List<Department>();
			Employees = new List<Employee>();
		}

		public Guid Id { get; set; }

		public string Name { get; set; }

		public IList<Department> SubDepartments { get; }

		public IList<Employee> Employees { get; }
	}
}
