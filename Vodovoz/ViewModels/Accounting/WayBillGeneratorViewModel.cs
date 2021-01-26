using System;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Additions.Accounting;
using Vodovoz.Domain.Employees;
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
			RouteGeometryCalculator calculator)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			if (wayBillDocumentRepository == null)
				throw new ArgumentNullException(nameof(wayBillDocumentRepository));
			
			if (calculator == null)
				throw new ArgumentNullException(nameof(calculator));

			this.Entity = new WayBillDocumentGenerator(UnitOfWorkFactory.CreateWithoutRoot(), wayBillDocumentRepository, calculator);
			TabName = "Путевые листы для ФО";
			CreateCommands();
		}

        private Employee mechanic;
        public Employee Mechanic {
            get => mechanic;
            set {
                Entity.MechanicFIO = value.FullName;
                Entity.MechanicLastName = value.LastName;
                mechanic = value;
            }
        }

		#region Properties
		public DateTime StartDate {
			get => Entity.StartDate;
			set => Entity.StartDate = value;
		}

		public DateTime EndDate {
			get => Entity.EndDate;
			set => Entity.EndDate = value;
		}
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
				() => throw new NotImplementedException(nameof(UnloadCommand)),
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