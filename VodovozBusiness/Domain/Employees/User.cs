using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Employees
{
	public class User: QSOrmProject.Domain.User
	{
		public virtual string WarehouseAccess { get; set; }
	}
}

