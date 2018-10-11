using System;
using QS.DomainModel.Entity;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Employees
{
	public class User: QS.Project.Domain.UserBase
	{
		public virtual string WarehouseAccess { get; set; }
	}
}

