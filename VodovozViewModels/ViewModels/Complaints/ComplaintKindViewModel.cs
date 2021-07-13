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

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintKindViewModel : EntityTabViewModelBase<ComplaintKind>
	{
		private readonly IEntityAutocompleteSelectorFactory _employeeSelectorFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		public ComplaintKindViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
			IEntityAutocompleteSelectorFactory employeeSelectorFactory) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			CreateAttachSubdivisionCommand();

			ComplaintObjects = UoW.Session.QueryOver<ComplaintObject>().List();

			TabName = "Виды рекламаций";
		}

		public IList<ComplaintObject> ComplaintObjects { get; }

		#region AttachSubdivisionCommand

		public DelegateCommand AttachSubdivisionCommand { get; private set; }

		private void CreateAttachSubdivisionCommand()
		{
			AttachSubdivisionCommand = new DelegateCommand(
				() => {
					var subdivisionFilter = new SubdivisionFilterViewModel();
					var subdivisionJournalViewModel = new SubdivisionsJournalViewModel(
						subdivisionFilter,
						_unitOfWorkFactory,
						_commonServices,
						_employeeSelectorFactory
					);
					subdivisionJournalViewModel.SelectionMode = JournalSelectionMode.Single;
					subdivisionJournalViewModel.OnEntitySelectedResult += (sender, e) => {
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
			);
		}

		#endregion AttachSubdivisionCommand
	}
}
