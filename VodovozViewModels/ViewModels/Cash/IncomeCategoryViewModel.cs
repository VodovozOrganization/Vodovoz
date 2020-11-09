using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalSelectors;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
    public class IncomeCategoryViewModel: EntityTabViewModelBase<IncomeCategory>
    {
        public IncomeCategoryViewModel(IEntityUoWBuilder uowBuilder,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
            IFileChooserProvider fileChooserProvider,
            IncomeCategoryJournalFilterViewModel journalFilterViewModel
        ) : base(uowBuilder, unitOfWorkFactory, commonServices)
        {
            IncomeCategoryAutocompleteSelectorFactory = 
                new IncomeCategoryAutoCompleteSelectorFactory(commonServices, journalFilterViewModel, fileChooserProvider);

            if(uowBuilder.IsNewEntity)
                TabName = "Создание новой категории дохода";
            else
                TabName = $"{Entity.Title}";
        }
        
        public bool IsArchive
        {
            get { return Entity.IsArchive; }
            set
            {
                Entity.SetIsArchiveRecursively(value);
            }
        }
        
        public IEntityAutocompleteSelectorFactory IncomeCategoryAutocompleteSelectorFactory;
        
        #region Permissions
        public bool CanCreate => PermissionResult.CanCreate;
        public bool CanRead => PermissionResult.CanRead;
        public bool CanUpdate => PermissionResult.CanUpdate;
        public bool CanDelete => PermissionResult.CanDelete;

        public bool CanCreateOrUpdate => Entity.Id == 0 ? CanCreate : CanUpdate;

        #endregion
    }
}