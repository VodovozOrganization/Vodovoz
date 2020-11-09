using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organization;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalSelectors;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.ViewModels.Cash
{
	public class ExpenseCategoryViewModel : EntityTabViewModelBase<ExpenseCategory>
	{
		public ExpenseCategoryViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IFileChooserProvider fileChooserProvider,
			ExpenseCategoryJournalFilterViewModel journalFilterViewModel
			) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			ExpenseCategoryAutocompleteSelectorFactory = 
				new ExpenseCategoryAutoCompleteSelectorFactory(commonServices, journalFilterViewModel, fileChooserProvider);
			
			if(uowBuilder.IsNewEntity)
				TabName = "Создание новой категории расхода";
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

		public readonly IEntityAutocompleteSelectorFactory ExpenseCategoryAutocompleteSelectorFactory;

		
		
		#region Permissions

		public bool CanCreate => PermissionResult.CanCreate;
		public bool CanUpdate => PermissionResult.CanUpdate;

		#endregion
	}
}
