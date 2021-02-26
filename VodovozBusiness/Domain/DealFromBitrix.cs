using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain {
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "телефоны",
        Nominative = "телефон")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DealFromBitrix: PropertyChangedBase, IDomainObject {
        public virtual int Id { get; set; }
        
        private uint bitrixId;
        [Display(Name = "Id в Битриксе")]
        public virtual uint BitrixId {
            get => bitrixId;
            set { SetField(ref bitrixId, value); }
        }
        
        private DateTime createDate;
        [Display(Name = "Id в Битриксе")]
        public virtual DateTime CreateDate {
            get => createDate;
            set { SetField(ref createDate, value); }
        }
        
        private DateTime? processedDate;
        [Display(Name = "Id в Битриксе")]
        public virtual DateTime? ProcessedDate {
            get => processedDate;
            set { SetField(ref processedDate, value); }
        }
        
        private bool success;
        [Display(Name = "Id в Битриксе")]
        public virtual bool Success {
            get => success;
            set { SetField(ref success, value); }
        }
        
        private Order order;
        [Display(Name = "Id в Битриксе")]
        public virtual Order Order {
            get => order;
            set { SetField(ref order, value); }
        }
        
        private string extensionText;
        [Display(Name = "Текст ошибки")]
        public virtual string ExtensionText {
            get => extensionText;
            set { SetField(ref extensionText, value); }
        }
    }
}