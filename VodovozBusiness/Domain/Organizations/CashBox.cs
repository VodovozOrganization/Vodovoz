using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Organizations
{
    [Appellative(Gender = GrammaticalGender.Feminine,
        Nominative = "кассовый аппарат",
        NominativePlural = "кассовые аппараты")]
    public class CashBox : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }

        private Guid userName;
        [Display(Name = "Имя пользователя")]
        public virtual Guid UserName {
            get => userName;
            set => SetField(ref userName, value);
        }
        
        private string password;
        [Display(Name = "Пароль")]
        public virtual string Password {
            get => password;
            set => SetField(ref password, value);
        }
    }
}