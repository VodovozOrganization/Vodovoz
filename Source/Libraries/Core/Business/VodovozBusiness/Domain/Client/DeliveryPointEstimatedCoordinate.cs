using QS.DomainModel.Entity;
using System;

namespace Vodovoz.Domain.Client
{
    [Appellative(
		Gender = GrammaticalGender.Feminine,
		NominativePlural = "предположительные координаты точки доставки",
		Nominative = "предположительная координата точки доставки"
	)]
	public class DeliveryPointEstimatedCoordinate : IDomainObject
    {
        public virtual int Id { get; set; }
        public virtual int DeliveryPointId { get; set; }
        public virtual decimal Latitude { get; set; }
        public virtual decimal Longitude { get; set; }
        public virtual DateTime RegistrationTime { get; set; }
    }
}
