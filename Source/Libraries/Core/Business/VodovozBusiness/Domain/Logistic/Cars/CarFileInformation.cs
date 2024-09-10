﻿using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Logistic.Cars;

namespace VodovozBusiness.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах автомобилей",
		Nominative = "информация о прикрепленном файле автомобиля")]
	[HistoryTrace]
	public class CarFileInformation : FileInformation
	{
		private int _carId;

		[Display(Name = "Идентификатор автомобиля")]
		[HistoryIdentifier(TargetType = typeof(Car))]
		public virtual int CarId
		{
			get => _carId;
			set => SetField(ref _carId, value);
		}
	}
}
