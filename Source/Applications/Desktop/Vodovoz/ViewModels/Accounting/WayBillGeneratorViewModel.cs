using System;
using System.IO;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal.EntitySelector;
using QS.ViewModels;
using Vodovoz.Additions.Accounting;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Logistic;
using VodovozInfrastructure.Interfaces;

namespace Vodovoz.ViewModels.Accounting
{
	public class WayBillGeneratorViewModel : DialogTabViewModelBase
	{
		public readonly WayBillDocumentGenerator Entity;
		private readonly IFileChooserProvider _fileChooser;
		private Employee _mechanic;

		public WayBillGeneratorViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IWayBillDocumentRepository wayBillDocumentRepository,
			RouteGeometryCalculator calculator,
			IEmployeeJournalFactory employeeJournalFactory,
			IDocTemplateRepository docTemplateRepository,
			IFileChooserProvider fileChooserProvider) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			EntityAutocompleteSelectorFactory = employeeJournalFactory?.CreateEmployeeAutocompleteSelectorFactory()
			                                 ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_fileChooser = fileChooserProvider ?? throw new ArgumentNullException(nameof(fileChooserProvider));

			if(wayBillDocumentRepository == null)
			{
				throw new ArgumentNullException(nameof(wayBillDocumentRepository));
			}

			if(calculator == null)
			{
				throw new ArgumentNullException(nameof(calculator));
			}

			Entity = new WayBillDocumentGenerator(
				UnitOfWorkFactory.CreateWithoutRoot(), wayBillDocumentRepository, calculator, docTemplateRepository);

			TabName = "Путевые листы для ФО";
			CreateCommands();
		}

		#region Properties

		public Employee Mechanic
		{
			get => _mechanic;
			set
			{
				if(value == null)
				{
					Entity.MechanicFIO = null;
					Entity.MechanicLastName = null;
				}
				else
				{
					Entity.MechanicFIO = value.FullName;
					Entity.MechanicLastName = value.LastName;
				}
				
				_mechanic = value;
			}
		}

		public DateTime StartDate
		{
			get => Entity.StartDate;
			set => Entity.StartDate = value;
		}

		public DateTime EndDate
		{
			get => Entity.EndDate;
			set => Entity.EndDate = value;
		}

		public IEntityAutocompleteSelectorFactory EntityAutocompleteSelectorFactory { get; }

		#endregion

		#region Commands

		void CreateCommands()
		{
			CreateGenerateCommand();
			CreateUnloadCommand();
			CreatePrintCommand();
		}

		private void CreateGenerateCommand()
		{
			GenerateCommand = new DelegateCommand(
				Entity.GenerateDocuments,
				() => true
			);
		}

		private void CreateUnloadCommand()
		{
			UnloadCommand = new DelegateCommand(
				() =>
				{
					var path = _fileChooser.GetExportFolderPath();
					if(Directory.Exists(path))
					{
						Entity.ExportODTDocuments(path);
					}
				},
				() => true
			);
		}

		private void CreatePrintCommand()
		{
			PrintCommand = new DelegateCommand(
				Entity.PrintDocuments,
				() => true
			);
		}

		public DelegateCommand GenerateCommand { get; private set; }
		public DelegateCommand UnloadCommand { get; private set; }
		public DelegateCommand PrintCommand { get; private set; }

		#endregion
	}
}
