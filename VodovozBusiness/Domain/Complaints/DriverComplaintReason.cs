using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Complaints
{
    [Appellative(Gender = GrammaticalGender.Feminine,
        NominativePlural = "причины оценки",
        Nominative = "причина оценки"
    )]
    [HistoryTrace]
    [EntityPermission]
    public class DriverComplaintReason : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }

        private string name;
        [Display(Name = "Название")]
        public virtual string Name
        {
            get => name;
            set => SetField(ref name, value);
        }

        private bool isPopular;
        [Display(Name = "Популярная причина")]
        public virtual bool IsPopular
        {
            get => isPopular;
            set => SetField(ref isPopular, value);
        }
    }
}
