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
		private DelegateCommand _setArchiveCommand;
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

		public DelegateCommand SetArchiveCommand =>
			_setArchiveCommand ?? (_setArchiveCommand = new DelegateCommand(() =>
				{
					if(!Entity.IsArchive)
					{
						var parent = Entity.Parent;
						if(parent != null && parent.IsArchive)
						{
							Entity.IsArchive = true;
							ShowWarningMessage(
								$"Родительская группа {parent.Name} архивирована.\n" +
								"Выполните одно из действий:\n" +
								"Либо уберите родительскую группу у данной группы\n" +
								"Либо разархивируйте родителя\n" +
								"Либо перенесите эту группу в действующую неархивную группу товаров");
						}
					}
					else
					{
						Entity.FetchChilds(UoW);
						Entity.SetIsArchiveRecursively(Entity.IsArchive);
					}
				}
			));

		public bool CanEditOnlineStore { get; } = true;
		public IEntityAutocompleteSelectorFactory ProductGroupSelectorFactory { get; }
	}
}
