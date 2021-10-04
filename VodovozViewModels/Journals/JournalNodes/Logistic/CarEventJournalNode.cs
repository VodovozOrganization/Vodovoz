﻿using System;
using Gamma.Utilities;
using QS.Project.Journal;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Logistic
{
	public class CarEventJournalNode : JournalEntityNodeBase<CarEvent>
	{
		public DateTime CreateDate { get; set; }
		public string AuthorFullName { get; set; }
		public string CarEventTypeName { get; set; }
		public string CarRegistrationNumber { get; set; }
		public int? CarOrderNumber { get; set; }
		public string DriverFullName { get; set; }
		public string GeographicGroups { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Comment { get; set; }
		public CarTypeOfUse CarTypeOfUse { get; set; }
		public bool IsRaskat { get; set; }
		public RaskatType CarRaskatType { get; set; }


		public string CarTypeOfUseString
		{
			get
			{
				switch(CarTypeOfUse)
				{
					case CarTypeOfUse.GAZelle:
						return "ГК";

					case CarTypeOfUse.Largus:
						return "ЛК";
				}

				return CarTypeOfUse.GetEnumTitle();
			}
		}
	}
}
