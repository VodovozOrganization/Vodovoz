using System;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Journals.JournalViewModels.Organization;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintKindViewModel : EntityTabViewModelBase<ComplaintKind>
	{
		private readonly IEntityAutocompleteSelectorFactory _employeeSelectorFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private DelegateCommand<Subdivision> _removeSubdivisionCommand;
		private DelegateCommand _attachSubdivisionCommand;
		private readonly Action _updateJournalAction;
		private readonly IList<Subdivision> _subdivisionsOnStart;
		private readonly ISalesPlanJournalFactory _salesPlanJournalFactory;
		private readonly INomenclatureJournalFactory _nomenclatureSelectorFactory;

		public ComplaintKindViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
			IEntityAutocompleteSelectorFactory employeeSelectorFactory, Action updateJournalAction, ISalesPlanJournalFactory salesPlanJournalFactory,
			INomenclatureJournalFactory nomenclatureSelectorFactory) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_updateJournalAction = updateJournalAction ?? throw new ArgumentNullException(nameof(updateJournalAction));
			_salesPlanJournalFactory = salesPlanJournalFactory ?? throw new ArgumentNullException(nameof(salesPlanJournalFactory));
			_nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));

			ComplaintObjects = UoW.Session.QueryOver<ComplaintObject>().List();
			_subdivisionsOnStart = new List<Subdivision>(Entity.Subdivisions);

			TabName = "Виды рекламаций";
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
					var subdivisionJournalViewModel = new SubdivisionsJournalViewModel(
						subdivisionFilter,
						_unitOfWorkFactory,
						_commonServices,
						_employeeSelectorFactory,
						_salesPlanJournalFactory,
						_nomenclatureSelectorFactory
					);
					subdivisionJournalViewModel.SelectionMode = JournalSelectionMode.Single;
					subdivisionJournalViewModel.OnEntitySelectedResult += (sender, e) =>
					{
						var selectedNode = e.SelectedNodes.FirstOrDefault();
						if(selectedNode == null)
						{
							return;
						}
						Entity.AddSubdivision(UoW.GetById<Subdivision>(selectedNode.Id));
					};
					TabParent.AddSlaveTab(this, subdivisionJournalViewModel);
				},
				() => true
			));


		public DelegateCommand<Subdivision> RemoveSubdivisionCommand => _removeSubdivisionCommand ?? (_removeSubdivisionCommand =
			new DelegateCommand<Subdivision>((subdivision) =>
				{
					Entity.RemoveSubdivision(subdivision);
				},
				(subdivision) => true
			));

		#endregion Commands

	}
}
