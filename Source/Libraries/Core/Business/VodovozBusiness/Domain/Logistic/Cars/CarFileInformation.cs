using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Common;

namespace VodovozBusiness.Domain.Logistic.Cars
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "информация о прикрепляемых файлах автомобилей",
		Nominative = "информация о прикрепленном файле автомобиля")]
	public class CarFileInformation : FileInformation
	{
		private int _carId;

		[Display(Name = "Идентификатор автомобиля")]
		public virtual int CarId
		{
			get => _carId;
			set => SetField(ref _carId, value);
		}
	}
}
