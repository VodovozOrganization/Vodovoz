using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Organizations
{
    [Appellative(Gender = GrammaticalGender.Feminine,
        Nominative = "кассовый аппарат",
        NominativePlural = "кассовые аппараты")]
    public class CashMachine : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }

        private Guid userName;
        [Display(Name = "Имя пользователя")]
        public Guid UserName {
            get => userName;
            set => SetField(ref userName, value);
        }
        
        private string password;
        [Display(Name = "Пароль")]
        public string Password {
            get => password;
            set => SetField(ref password, value);
        }

        private string baseAddress;
        [Display(Name = "Адрес")]
        public string BaseAddress {
            get => baseAddress;
            set => SetField(ref baseAddress, value);
        }
    }
}