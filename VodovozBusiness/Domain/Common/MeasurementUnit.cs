using QS.BusinessCommon.Domain;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Common
{
    [Appellative(Gender = GrammaticalGender.Feminine,
        NominativePlural = "единицы измерения",
        Nominative = "единица измерения")]
    [EntityPermission]
    public class MeasurementUnit : MeasurementUnits
    {
        private string bitrixName;
        [Display(Name = "Идентификатор в Битриксе")]
        public virtual string BitrixName {
            get => bitrixName;
            set => SetField(ref bitrixName, value);
        }
    }
}
