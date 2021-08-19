using System;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Cash;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalSelectors;
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
			ExpenseCategoryJournalFilterViewModel journalFilterViewModel,
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory
			) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			if(employeeJournalFactory == null)
			{
				throw new ArgumentNullException(nameof(employeeJournalFactory));
			}
			
			if(subdivisionJournalFactory == null)
			{
				throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			}
			
			ExpenseCategoryAutocompleteSelectorFactory = 
				new ExpenseCategoryAutoCompleteSelectorFactory(
					commonServices, journalFilterViewModel, fileChooserProvider, employeeJournalFactory, subdivisionJournalFactory);

			SubdivisionAutocompleteSelectorFactory =
				subdivisionJournalFactory.CreateDefaultSubdivisionAutocompleteSelectorFactory(
					employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory());
			
			if(uowBuilder.IsNewEntity)
				TabName = "Создание новой категории расхода";
			else
				TabName = $"{Entity.Title}";
			
		}
		
		public IEntityAutocompleteSelectorFactory SubdivisionAutocompleteSelectorFactory { get; }

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
