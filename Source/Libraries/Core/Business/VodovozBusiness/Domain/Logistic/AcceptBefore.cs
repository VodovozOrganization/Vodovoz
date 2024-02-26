using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace Vodovoz.Domain.Logistic
{
    [Appellative (Gender = GrammaticalGender.Feminine,
        NominativePlural = "времена приема до",
        Nominative = "время приема до")]
    public class AcceptBefore : PropertyChangedBase, IDomainObject, IValidatableObject
    {
        public virtual int Id { get; set; }
        
        private TimeSpan time;
        [Display (Name = "До часа")]
        public virtual TimeSpan Time {
            get => time;
            set {
                if(SetField(ref time, value, () => Time))
                    Name = value.ToString(@"hh\:mm");
            }
        }

        private string name;
        [Display(Name = "Название")]
        public virtual string Name {
            get => name; 
            set => SetField(ref name, value, () => Name);
        }
        
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
			var uowFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();
            using (IUnitOfWork uow = uowFactory.CreateWithoutRoot())
			{
				var allTimes = uow.GetAll<AcceptBefore>();
                if (allTimes.Any(x => x.Time == Time))
                    yield return new ValidationResult ("Такое время уже присутствует в справочнике",
                        new[] { this.GetPropertyName (o => o.Time) });
            }
           
        }
    }
}
