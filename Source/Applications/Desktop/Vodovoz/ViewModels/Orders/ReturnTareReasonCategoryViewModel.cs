using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;
using QS.Commands;
using Vodovoz.JournalViewModels;
using System.Linq;

namespace Vodovoz.ViewModels.Orders
{
    public class ReturnTareReasonCategoryViewModel : EntityTabViewModelBase<ReturnTareReasonCategory>
    {
        public ReturnTareReasonCategoryViewModel(IEntityUoWBuilder uowBuilder, 
                                        IUnitOfWorkFactory unitOfWorkFactory,
                                        ICommonServices commonServices) : base(uowBuilder, unitOfWorkFactory, commonServices)
        {
            if(uowBuilder.IsNewEntity)
                TabName = "Создание новой категории причины забора тары";
			else
			    TabName = $"{Entity.Title}";

			CreateCommands();
        }

		private ReturnTareReason selectedReason;
		public ReturnTareReason SelectedReason {
			get => selectedReason;
			set => SetField(ref selectedReason, value);
		}

		private string reasonName;
		public string ReasonName {
			get => reasonName;
			set => SetField(ref reasonName, value);
		}

		#region Commands

		public DelegateCommand AddReasonCommand { get; private set; }

		public DelegateCommand RemoveReasonCommand { get; private set; }

		#endregion


		public void CreateCommands()
		{
			CreateAddReasonCommand();
			CreateRemoveReasonCommand();
		}

		private void CreateRemoveReasonCommand()
		{
			RemoveReasonCommand = new DelegateCommand(
				() => Entity.RemoveChildReason(SelectedReason),
				() => SelectedReason != null
			);
		}

		private void CreateAddReasonCommand()
		{
			AddReasonCommand = new DelegateCommand(
				() => {

					var reasonsSelector = new ReturnTareReasonsJournalViewModel(UnitOfWorkFactory, CommonServices) 
					{
						SelectionMode = QS.Project.Journal.JournalSelectionMode.Single
					};

					reasonsSelector.OnEntitySelectedResult += (sender, e) => {
						var selectedNode = e.SelectedNodes.FirstOrDefault();

						if(selectedNode == null) {
							return;
						}

						ReturnTareReason subdivision = UoW.GetById<ReturnTareReason>(selectedNode.Id);
						Entity.AddChildReason(subdivision);
					};

					TabParent.AddSlaveTab(this, reasonsSelector);
				},
				() => true
			);
		}
	}
}