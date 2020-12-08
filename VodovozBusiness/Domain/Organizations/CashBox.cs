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

        private string baseAddress;
        [Display(Name = "Базовый адрес")]
        public virtual string BaseAddress {
            get => baseAddress;
            set => SetField(ref baseAddress, value);
        }
        
        private string statusPath;
        [Display(Name = "Адрес проверки статуса")]
        public virtual string StatusPath {
            get => statusPath;
            set => SetField(ref statusPath, value);
        }
        
        private string sendDocumentPath;
        [Display(Name = "Адрес отправки документов")]
        public virtual string SendDocumentPath {
            get => sendDocumentPath;
            set => SetField(ref sendDocumentPath, value);
        }
    }
}