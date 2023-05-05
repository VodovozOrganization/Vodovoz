using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Cash;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.TempAdapters
{
	public class IncomeCategorySelectorFactory : IIncomeCategorySelectorFactory
	{
		public IEntityAutocompleteSelectorFactory CreateSimpleIncomeCategoryAutocompleteSelectorFactory()
		{
			var commonServices = ServicesConfig.CommonServices;
			var employeeJournalFactory = new EmployeeJournalFactory();
			var subdivisionJournalFactory = new SubdivisionJournalFactory();
			var incomeFactory = new IncomeCategorySelectorFactory();

			var incomeCategoryAutocompleteSelectorFactory =
				new SimpleEntitySelectorFactory<IncomeCategory, IncomeCategoryViewModel>(
					() =>
					{
						var incomeCategoryJournalViewModel =
							new SimpleEntityJournalViewModel<IncomeCategory, IncomeCategoryViewModel>(
								x => x.Name,
								() => new IncomeCategoryViewModel(
									EntityUoWBuilder.ForCreate(),
									UnitOfWorkFactory.GetDefaultFactory,
									commonServices,
									employeeJournalFactory,
									subdivisionJournalFactory,
									incomeFactory
								),
								node => new IncomeCategoryViewModel(
									EntityUoWBuilder.ForOpen(node.Id),
									UnitOfWorkFactory.GetDefaultFactory,
									commonServices,
									employeeJournalFactory,
									subdivisionJournalFactory,
									incomeFactory
								),
								UnitOfWorkFactory.GetDefaultFactory,
								commonServices
							)
							{
								SelectionMode = JournalSelectionMode.Single
							};
						return incomeCategoryJournalViewModel;
					});
			return incomeCategoryAutocompleteSelectorFactory;
		}

		public IEntityAutocompleteSelectorFactory CreateDefaultIncomeCategoryAutocompleteSelectorFactory()
		{
			var incomeCategoryFilter = new IncomeCategoryJournalFilterViewModel();
			IFileChooserProvider chooserIncomeProvider = new FileChooser();
			var employeeJournalFactory = new EmployeeJournalFactory();
			var subdivisionJournalFactory = new SubdivisionJournalFactory();
			var incomeFactory = new IncomeCategorySelectorFactory();

			return new EntityAutocompleteSelectorFactory<IncomeCategoryJournalViewModel>(
				typeof(IncomeCategory),
				() => new IncomeCategoryJournalViewModel(
					incomeCategoryFilter,
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					chooserIncomeProvider,
					employeeJournalFactory,
					subdivisionJournalFactory,
					incomeFactory)
				{
					SelectionMode = JournalSelectionMode.Single
				});
		}
	}
}
