using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain
{
    [Appellative(Gender = GrammaticalGender.Feminine,
        NominativePlural = "записи о действиях водителей в приложении",
        Nominative = "запись о действии водителя в приложении")]
    public class DriverAppActionRecord
    {
        public virtual int Id { get; set; }
        
        private int actionType;
        [Display(Name = "Тип действия")]
        public virtual int ActionType
        {
            get { return actionType; }
            set { actionType = value; }
        }

        private DateTime actionDateTime;
        [Display(Name = "Время действия")]
        public virtual DateTime ActionDateTime
        {
            get { return actionDateTime; }
            set { actionDateTime = value; }
        }
    }
}
