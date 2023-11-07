using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Complaints;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintKindViewModel : EntityTabViewModelBase<ComplaintKind>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private DelegateCommand<Subdivision> _removeSubdivisionCommand;
		private DelegateCommand _attachSubdivisionCommand;
		private readonly Action _updateJournalAction;
		private readonly IList<Subdivision> _subdivisionsOnStart;
		private readonly ISalesPlanJournalFactory _salesPlanJournalFactory;
		private readonly INomenclatureJournalFactory _nomenclatureSelectorFactory;
		private readonly ILifetimeScope _scope;

		public ComplaintKindViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IEmployeeJournalFactory employeeJournalFactory,
			Action updateJournalAction,
			ISalesPlanJournalFactory salesPlanJournalFactory,
			INomenclatureJournalFactory nomenclatureSelectorFactory,
			ILifetimeScope scope) : base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_updateJournalAction = updateJournalAction ?? throw new ArgumentNullException(nameof(updateJournalAction));
			_salesPlanJournalFactory = salesPlanJournalFactory ?? throw new ArgumentNullException(nameof(salesPlanJournalFactory));
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			ComplaintObjects = UoW.Session.QueryOver<ComplaintObject>().List();
			_subdivisionsOnStart = new List<Subdivision>(Entity.Subdivisions);

			TabName = "Вид рекламаций";
		}

		protected override void AfterSave()
		{
			var isEqualSubdivisionLists = new HashSet<Subdivision>(_subdivisionsOnStart).SetEquals(Entity.Subdivisions);

			if(!isEqualSubdivisionLists)
			{
				_updateJournalAction.Invoke();
			}

			base.AfterSave();
		}

		public IList<ComplaintObject> ComplaintObjects { get; }

		#region Commands

		public DelegateCommand AttachSubdivisionCommand => _attachSubdivisionCommand ?? (_attachSubdivisionCommand = new DelegateCommand(() =>
				{
					var subdivisionFilter = new SubdivisionFilterViewModel();

					var page = NavigationManager.OpenViewModel<SubdivisionsJournalViewModel>(this);

					page.ViewModel.SelectionMode = JournalSelectionMode.Single;
					page.ViewModel.OnSelectResult += (sender, e) =>
					{
						var selectedNode = e.SelectedObjects.FirstOrDefault();
						if(selectedNode == null)
						{
							return;
						}
						Entity.AddSubdivision(UoW.GetById<Subdivision>(((SubdivisionJournalNode)selectedNode).Id));
					};
				},
				() => true));

		public DelegateCommand<Subdivision> RemoveSubdivisionCommand => _removeSubdivisionCommand ?? (_removeSubdivisionCommand =
			new DelegateCommand<Subdivision>((subdivision) =>
				{
					Entity.RemoveSubdivision(subdivision);
				},
				(subdivision) => true));

		#endregion Commands

		protected override bool BeforeSave()
		{
			if(Entity.IsArchive && UoW.HasChanges)
			{
				if(!AskQuestion("Будут архивированы все детализации привязанные к этому виду рекламаций, вы уверены?", "Внимание!!"))
				{
					return false;
				}

				foreach(var detalizationst in UoW.Query<ComplaintDetalization>()
					.Where(x => x.ComplaintKind.Id == Entity.Id).List())
				{
					detalizationst.IsArchive = true;
				}
			}

			return base.BeforeSave();
		}
	}
}
