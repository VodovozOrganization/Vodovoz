using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;

namespace Vodovoz.Domain.Employees
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "стажеры",
		Nominative = "стажер")]
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
