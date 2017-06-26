using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	public class AtWorkDriver : AtWorkBase
	{
		private Car car;

		[Display(Name = "Автомобиль")]
		public virtual Car Car {
			get { return car; }
			set { SetField(ref car, value, () => Car); }
		}

		public AtWorkDriver()
		{
		}
	}
}
