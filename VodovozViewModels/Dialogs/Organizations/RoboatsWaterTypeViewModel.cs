using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Organizations;
using Vodovoz.Factories;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Dialogs.Organizations
{
	public class RoboatsWaterTypeViewModel : EntityTabViewModelBase<RoboatsWaterType>
	{
		private RoboatsEntityViewModel _roboatsEntityViewModel;
		private readonly INomenclatureSelectorFactory _nomenclatureJournalFactory;
		private IEntityAutocompleteSelectorFactory _nomenclatureSelectorFactory;

		public RoboatsWaterTypeViewModel(IEntityUoWBuilder uowBuilder, INomenclatureSelectorFactory nomenclatureJournalFactory, RoboatsViewModelFactory roboatsViewModelFactory, ICommonServices commonServices) : base(uowBuilder, commonServices)
		{
			if(roboatsViewModelFactory is null)
			{
				throw new ArgumentNullException(nameof(roboatsViewModelFactory));
			}

			_nomenclatureJournalFactory = nomenclatureJournalFactory ?? throw new ArgumentNullException(nameof(nomenclatureJournalFactory));

			RoboatsEntityViewModel = roboatsViewModelFactory.CreateViewModel(Entity);

			NomenclatureSelectorFactory = _nomenclatureJournalFactory.GetRoboatsWaterJournalFactory();
		}

		public RoboatsEntityViewModel RoboatsEntityViewModel
		{
			get => _roboatsEntityViewModel;
			set => SetField(ref _roboatsEntityViewModel, value);
		}

		public virtual  IEntityAutocompleteSelectorFactory  NomenclatureSelectorFactory
		{
			get => _nomenclatureSelectorFactory;
			set => SetField(ref _nomenclatureSelectorFactory, value);
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
