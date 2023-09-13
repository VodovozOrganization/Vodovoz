using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class NomenclatureOnlineCategoryViewModel : EntityTabViewModelBase<NomenclatureOnlineCategory>
	{
		private readonly ILifetimeScope _scope;

		public NomenclatureOnlineCategoryViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ILifetimeScope scope) : base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			ConfigureEntryViewModels();
		}

		public bool CanShowId => !UoW.IsNew;
		public string IdString => Entity.Id.ToString();
		
		public IEntityEntryViewModel NomenclatureOnlineGroupsViewModel { get; private set; }
		
		private void ConfigureEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<NomenclatureOnlineCategory>(this, Entity, UoW, NavigationManager, _scope);

			NomenclatureOnlineGroupsViewModel = builder.ForProperty(x => x.NomenclatureOnlineGroup)
				.UseViewModelJournalAndAutocompleter<NomenclatureOnlineGroupsJournalViewModel>()
				.UseViewModelDialog<NomenclatureOnlineGroupViewModel>()
				.Finish();
		}
	}
}
