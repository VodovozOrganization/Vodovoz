using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QSOrmProject;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "стажеры",
		Nominative = "стажер")]
	[EntityPermission]
	public class Trainee : Personnel, ITrainee
	{
		public override EmployeeType EmployeeType {
			get { return EmployeeType.Trainee; }
			set {}
		}
	}

	public interface ITrainee : IPersonnel
	{
		
	}
}
