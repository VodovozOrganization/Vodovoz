using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Employees
{
	public class User: QS.Project.Domain.UserBase
	{
		public virtual string WarehouseAccess { get; set; }
	}
}

