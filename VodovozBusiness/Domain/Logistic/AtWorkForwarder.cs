﻿using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic
{
	public class AtWorkForwarder : AtWorkBase
	{
		public AtWorkForwarder(Employee forwarder, DateTime date)
		{
			Employee = forwarder;
			Date = date;
		}

		protected AtWorkForwarder(){}
	}
}
