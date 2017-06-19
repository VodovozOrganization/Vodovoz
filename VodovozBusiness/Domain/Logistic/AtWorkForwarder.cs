using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic
{
	public class AtWorkForwarder : AtWorkBase
	{

		private Employee withDriver;

		[Display(Name = "notset")]
		public virtual Employee WithDriver {
			get { return withDriver; }
			set { SetField(ref withDriver, value, () => WithDriver); }
		}
	}
}
