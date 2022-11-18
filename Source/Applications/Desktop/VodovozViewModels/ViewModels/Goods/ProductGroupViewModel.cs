using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class ProductGroupViewModel : EntityTabViewModelBase<ProductGroup>
	{
		private DelegateCommand<bool> _setArchiveCommand;
		public ProductGroupViewModel(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
			IProductGroupJournalFactory productGroupJournalFactory) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			ProductGroupSelectorFactory = (productGroupJournalFactory ?? throw new ArgumentNullException(nameof(productGroupJournalFactory)))
				.CreateProductGroupAutocompleteSelectorFactory();

			if(Entity.Id != 0 && !commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_online_store"))
			{
				CanEditOnlineStore = false;
			}
		}

		public DelegateCommand<bool> SetArchiveCommand =>
			_setArchiveCommand ?? (_setArchiveCommand = new DelegateCommand<bool>((isActive) =>
				{
					Entity.FetchChilds(UoW);
					Entity.SetIsArchiveRecursively(isActive);
				},
				(isActive) => true
			));

		public bool CanEditOnlineStore { get; } = true;
		public IEntityAutocompleteSelectorFactory ProductGroupSelectorFactory { get; }
	}
}
