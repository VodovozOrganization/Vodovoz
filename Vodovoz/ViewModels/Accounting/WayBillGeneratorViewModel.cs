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
using Vodovoz.Tools.Logistic;

namespace Vodovoz.ViewModels.Accounting
{
    public class WayBillGeneratorViewModel: DialogTabViewModelBase
    {
        public readonly WayBillDocumentGenerator Entity;

        public WayBillGeneratorViewModel(
            IUnitOfWorkFactory unitOfWorkFactory,
            IInteractiveService interactiveService, 
            INavigationManager navigation,
            IWayBillDocumentRepository wayBillDocumentRepository,
            RouteGeometryCalculator calculator,
            IEntityAutocompleteSelectorFactory entityAutocompleteSelectorFactory,
            IDocTemplateRepository docTemplateRepository) : base(unitOfWorkFactory, interactiveService, navigation)
        {
            EntityAutocompleteSelectorFactory = entityAutocompleteSelectorFactory ?? throw new ArgumentNullException(nameof(entityAutocompleteSelectorFactory));

            if (wayBillDocumentRepository == null)
                throw new ArgumentNullException(nameof(wayBillDocumentRepository));
            
            if (calculator == null)
                throw new ArgumentNullException(nameof(calculator));

            Entity = new WayBillDocumentGenerator(
	            UnitOfWorkFactory.CreateWithoutRoot(), wayBillDocumentRepository, calculator, docTemplateRepository);
            
            TabName = "Путевые листы для ФО";
            CreateCommands();
        }

        #region Properties

        private Employee mechanic;
        public Employee Mechanic {
            get => mechanic;
            set {
                Entity.MechanicFIO = value.FullName;
                Entity.MechanicLastName = value.LastName;
                mechanic = value;
            }
        }
        
        public DateTime StartDate {
            get => Entity.StartDate;
            set => Entity.StartDate = value;
        }

        public DateTime EndDate {
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
                () => {
                    var fileChooser = new FileChooser("");
                    var path = fileChooser.GetExportFolderPath();
                    if (Directory.Exists(path)) {
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