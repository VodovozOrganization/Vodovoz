using QS.DomainModel.Entity;
using System;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic.Drivers
{
	[Appellative(Gender = GrammaticalGender.Feminine,
        Nominative = "Запись о действии водителя в мобильном приложении",
        NominativePlural = "Записи о действиях водителей в мобильном приложении")]
    public class DriverMobileAppActionRecord : IDomainObject
    {
        public virtual int Id { get; set; }
        public virtual Employee Driver { get; set; }
        public virtual DriverMobileAppActionType Action { get; set; }
		public virtual string Result { get; set; }
        public virtual DateTime ActionDatetime { get; set; }
        public virtual DateTime RecievedDatetime { get; set; }
    }
}
