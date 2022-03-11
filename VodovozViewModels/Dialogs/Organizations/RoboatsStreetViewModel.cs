using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Organizations;
using Vodovoz.Factories;

namespace Vodovoz.ViewModels.Dialogs.Organizations
{
	public class RoboatsStreetViewModel : EntityTabViewModelBase<RoboatsStreet>
	{
		private RoboatsEntityViewModel _roboatsEntityViewModel;
		private readonly RoboatsViewModelFactory _roboatsViewModelFactory;

		public RoboatsStreetViewModel(IEntityUoWBuilder uowBuilder, RoboatsViewModelFactory roboatsViewModelFactory, ICommonServices commonServices) : base(uowBuilder, commonServices)
		{
			_roboatsViewModelFactory = roboatsViewModelFactory ?? throw new ArgumentNullException(nameof(roboatsViewModelFactory));

			RoboatsEntityViewModel = _roboatsViewModelFactory.CreateViewModel(Entity);
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
