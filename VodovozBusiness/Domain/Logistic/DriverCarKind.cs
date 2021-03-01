using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Logistic
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        Nominative = "Вид наёмного автомобиля",
        NominativePlural = "Виды наёмных автомобилей"
    )]
    public class DriverCarKind : PropertyChangedBase, IDomainObject
    {
        public virtual int Id { get; set; }
        
        private string name;
        [Display(Name = "Название")]
        public virtual string Name
        {
            get => name;
            set => SetField(ref name, value);
        }
        
        private bool isArchive;
        [Display(Name = "Архивный")]
        public virtual bool IsArchive
        {
            get => isArchive;
            set => SetField(ref isArchive, value);
        }
    }
}
