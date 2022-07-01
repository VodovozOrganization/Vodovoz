using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.StoredResources;

namespace Vodovoz.Domain.Organizations
{
    [Appellative(Gender = GrammaticalGender.Feminine,
        NominativePlural = "организации",
        Nominative = "организация")]
    [EntityPermission]
	[HistoryTrace]
    public class Organization : AccountOwnerBase, IDomainObject
    {
        public Organization()
        {
            Name = "Новая организация";
            FullName = string.Empty;
            INN = string.Empty;
            KPP = string.Empty;
            OGRN = string.Empty;
            Email = string.Empty;
            Address = string.Empty;
            JurAddress = string.Empty;
        }

        #region Свойства

        public virtual int Id { get; set; }

        private string name;
        [Display(Name = "Название")]
        [Required(ErrorMessage = "Название организации должно быть заполнено.")]
        public virtual string Name {
            get => name;
            set => SetField(ref name, value);
        }

        private string fullName;
        [Display(Name = "Полное название")]
        public virtual string FullName {
            get => fullName;
            set => SetField(ref fullName, value);
        }

        private string iNN;
        [Display(Name = "ИНН")]
        [Digits(ErrorMessage = "ИНН может содержать только цифры.")]
        [StringLength(12, MinimumLength = 0, ErrorMessage = "Номер ИНН не должен превышать 12.")]
        public virtual string INN {
            get => iNN;
            set => SetField(ref iNN, value);
        }

        private string kPP;
        [Display(Name = "КПП")]
        [Digits(ErrorMessage = "КПП может содержать только цифры.")]
        [StringLength(9, MinimumLength = 0, ErrorMessage = "Номер КПП не должен превышать 9 цифр.")]
        public virtual string KPP {
            get => kPP;
            set => SetField(ref kPP, value);
        }

        private string oGRN;
        [Display(Name = "ОГРН/ОГРНИП")]
        [Digits(ErrorMessage = "ОГРН/ОГРНИП может содержать только цифры.")]
        [StringLength(15, MinimumLength = 0, ErrorMessage = "Номер ОГРНИП не должен превышать 15 цифр.")]
        public virtual string OGRN {
            get => oGRN;
            set => SetField(ref oGRN, value);
        }

        private string oKPO;
        [Display(Name = "ОКПО")]
        [Digits(ErrorMessage = "ОКПО может содержать только цифры.")]
        [StringLength(10, MinimumLength = 8, ErrorMessage = "Номер ОКПО не должен превышать 10 цифр.")]
        public virtual string OKPO {
            get => oKPO;
            set => SetField(ref oKPO, value);
        }

        private string oKVED;
        [Display(Name = "ОКВЭД")]
        [StringLength(100, ErrorMessage = "Номера ОКВЭД не должны превышать 100 знаков.")]
        public virtual string OKVED {
            get => oKVED;
            set => SetField(ref oKVED, value);
        }

        private IList<Phone> phones;
        [Display(Name = "Телефоны")]
        public virtual IList<Phone> Phones {
            get => phones;
            set => SetField(ref phones, value);
        }

        private string email;
        [Display(Name = "E-mail адреса")]
        public virtual string Email {
            get => email;
            set => SetField(ref email, value);
        }

        private string address;
        [Display(Name = "Фактический адрес")]
        public virtual string Address {
            get => address;
            set => SetField(ref address, value);
        }

        private string jurAddress;
        [Display(Name = "Юридический адрес")]
        public virtual string JurAddress {
            get => jurAddress;
            set => SetField(ref jurAddress, value);
        }

        private Employee leader;
        [Display(Name = "Руководитель")]
        public virtual Employee Leader {
            get => leader;
            set => SetField(ref leader, value);
        }

        private Employee buhgalter;
        [Display(Name = "Бухгалтер")]
        public virtual Employee Buhgalter {
            get => buhgalter;
            set => SetField(ref buhgalter, value);
        }
        
        private int? cashBoxId;
        [Display(Name = "ID Кассового аппарата")]
        public virtual int? CashBoxId {
            get => cashBoxId;
            set => SetField(ref cashBoxId, value);
        }
        
        private bool withoutVAT;
        [Display(Name = "Без НДС")]
        public virtual bool WithoutVAT {
            get => withoutVAT;
            set => SetField(ref withoutVAT, value);
        }

        private StoredResource stamp;
        [Display(Name = "Печать")]
        public virtual StoredResource Stamp
        {
            get => stamp;
            set => SetField(ref stamp, value);
        }

        #endregion
    }
    
}
