using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Client;
using Vodovoz.Factories;
using Vodovoz.ViewModels.Dialogs.Organizations;

namespace Vodovoz.ViewModels.Dialogs.Counterparty
{
	public class RoboAtsCounterpartyNameViewModel : EntityTabViewModelBase<RoboAtsCounterpartyName>
	{
		private RoboatsEntityViewModel _roboatsEntityViewModel;

		public RoboAtsCounterpartyNameViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, RoboatsViewModelFactory roboatsViewModelFactory, ICommonServices commonServices)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			if(roboatsViewModelFactory is null)
			{
				throw new ArgumentNullException(nameof(roboatsViewModelFactory));
			}

			TabName = "Имя контрагента RoboATS";

			RoboatsEntityViewModel = roboatsViewModelFactory.CreateViewModel(Entity);
		}

		public RoboatsEntityViewModel RoboatsEntityViewModel
		{
			get => _roboatsEntityViewModel;
			set => SetField(ref _roboatsEntityViewModel, value);
		}

		protected override bool BeforeSave()
		{
			return RoboatsEntityViewModel.Save();
		}

		public void Cancel()
		{
			Close(false, CloseSource.Cancel);
		}

		public override void Close(bool askSave, CloseSource source)
		{
			if(TabParent == null)
			{
				OnTabClosed();
			}
			else
			{
				base.Close(askSave, source);
			}
		}
	}
}
