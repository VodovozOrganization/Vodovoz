using System;
using QS.Commands;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.ViewModels.Complaints
{
	public class GuiltyItemsViewModel : EntityWidgetViewModelBase<Complaint>
	{
		readonly ISubdivisionRepository subdivisionRepository;
		readonly ICommonServices commonServices;

		public GuiltyItemsViewModel(Complaint entity, ICommonServices commonServices, ISubdivisionRepository subdivisionRepository) : base(entity, commonServices)
		{
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			this.commonServices = commonServices;
			CreateCommands();
			UpdateAcessibility();
		}

		GuiltyItemViewModel currentGuiltyVM;
		public virtual GuiltyItemViewModel CurrentGuiltyVM {
			get => currentGuiltyVM;
			set => SetField(ref currentGuiltyVM, value, () => CurrentGuiltyVM);
		}

		//public bool CanAddSubdivision(ComplaintGuiltyItem guilty) => guilty.GuiltyType == ComplaintGuiltyTypes.Subdivision && AllDepartments.Any();

		public bool CanRemoveGuilty(ComplaintGuiltyItem guilty) => guilty != null;

		bool canEditGuilty;
		public bool CanEditGuilty {
			get => canEditGuilty;
			set => SetField(ref canEditGuilty, value, () => CanEditGuilty);
		}

		bool canAddGuilty;
		public bool CanAddGuilty {
			get => canAddGuilty;
			set => SetField(ref canAddGuilty, value, () => CanAddGuilty);
		}

		void UpdateAcessibility()
		{
			CanAddGuilty = CurrentGuiltyVM == null;
			CanEditGuilty = !CanAddGuilty;
		}

		void CreateItem()
		{
			CurrentGuiltyVM = new GuiltyItemViewModel(new ComplaintGuiltyItem(), commonServices, subdivisionRepository) {
				UoW = UoW
			};
			UpdateAcessibility();
		}

		void ClearItem()
		{
			CurrentGuiltyVM = null;
			UpdateAcessibility();
		}

		#region Commands

		void CreateCommands()
		{
			CreateAddGuiltyCommand();
			CreateRemoveGuiltyCommand();
			CreateSaveGuiltyCommand();
			CreateCancelCommand();
		}

		#region AddGuiltyCommand

		public DelegateCommand AddGuiltyCommand { get; private set; }
		private void CreateAddGuiltyCommand()
		{
			AddGuiltyCommand = new DelegateCommand(
				CreateItem,
				() => CanAddGuilty
			);
		}

		#endregion AddGuiltyCommand

		#region SaveGuiltyCommand

		public DelegateCommand SaveGuiltyCommand { get; private set; }

		private void CreateSaveGuiltyCommand()
		{
			SaveGuiltyCommand = new DelegateCommand(
				() => {
					Entity.ObservableGuilties.Add(CurrentGuiltyVM.Entity);
					ClearItem();
				},
				() => CanEditGuilty
			);
		}

		#endregion SaveGuiltyCommand

		#region CancelCommand

		public DelegateCommand CancelCommand { get; private set; }

		private void CreateCancelCommand()
		{
			CancelCommand = new DelegateCommand(
				ClearItem,
				() => CanEditGuilty
			);
		}

		#endregion CancelCommand

		#region RemoveGuiltyCommand

		public DelegateCommand<ComplaintGuiltyItem> RemoveGuiltyCommand { get; private set; }
		private void CreateRemoveGuiltyCommand()
		{
			RemoveGuiltyCommand = new DelegateCommand<ComplaintGuiltyItem>(
				g => Entity.ObservableGuilties.Remove(g),
				CanRemoveGuilty
			);
		}

		#endregion RemoveGuiltyCommand

		#endregion Commands
	}
}
