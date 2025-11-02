using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Proposal
{
    [Appellative(Gender = GrammaticalGender.Neuter,
        NominativePlural = "предложения по разработке приложения ВВ",
        Nominative = "предложение по разработке приложения ВВ",
        Prepositional = "предложении по разработке приложения ВВ",
        PrepositionalPlural = "предложениях по разработке приложения ВВ")]
    [EntityPermission]
    public class ApplicationDevelopmentProposal : PropertyChangedBase, IDomainObject, IValidatableObject
    {
        public virtual int Id { get; set; }

        private DateTime creationDate;
        [Display(Name = "Дата создания")]
        public virtual DateTime CreationDate
        {
            get => creationDate;
            set => SetField(ref creationDate, value);
        }

        private Employee author;
        [Display(Name = "Автор предложения")]
        public virtual Employee Author
        {
            get => author;
            set => SetField(ref author, value);
        }
        
        private string title;
        [Display(Name = "Тема предложения")]
        public virtual string Title
        {
            get => title;
            set => SetField(ref title, value);
        }
        
        private string location = string.Empty;
        [Display(Name = "Расположение")]
        public virtual string Location
        {
            get => location;
            set => SetField(ref location, value);
        }
        
        private string description;
        [Display(Name = "Описание")]
        public virtual string Description
        {
            get => description;
            set => SetField(ref description, value);
        }
        
        private string proposalResponse = string.Empty;
        [Display(Name = "Ответ на предложение")]
        public virtual string ProposalResponse
        {
            get => proposalResponse;
            set => SetField(ref proposalResponse, value);
        }

        private ApplicationDevelopmentProposalStatus status;
        [Display(Name = "Статус")]
        public virtual ApplicationDevelopmentProposalStatus Status
        {
            get => status;
            set => SetField(ref status, value);
        }

        public ApplicationDevelopmentProposal()
        {
            Status = ApplicationDevelopmentProposalStatus.New;
        }

        public virtual void ChangeStatus(ApplicationDevelopmentProposalStatus newStatus)
        {
            Status = newStatus;
        }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Title)) {
                yield return new ValidationResult("Тема должна быть заполнена.",
                    new[] { nameof(Title) });
            }
            
            if (string.IsNullOrEmpty(Description)) {
                yield return new ValidationResult("Описание должно быть заполнено.",
                    new[] { nameof(Description) });
            }

            if (Status == ApplicationDevelopmentProposalStatus.Rejected && string.IsNullOrEmpty(ProposalResponse)) {
                yield return new ValidationResult("Необходимо заполнить ответ по предложению.",
                    new[] { nameof(ProposalResponse) });
            }
        }
    }
}
