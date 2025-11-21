using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Journals.Nodes.Rent;
using Vodovoz.ViewModels.ViewModels.Rent;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Rent
{
	public class FreeRentPackagesJournalViewModel
		: EntityJournalViewModelBase<FreeRentPackage, FreeRentPackageViewModel, FreeRentPackagesJournalNode>
	{
		private readonly FreeRentPackagesFilterViewModel _filterViewModel;

		public FreeRentPackagesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			FreeRentPackagesFilterViewModel freeRentPackagesFilterViewModel,
			ICurrentPermissionService currentPermissionService = null,
			IDeleteEntityService deleteEntityService = null,
			Action<FreeRentPackagesFilterViewModel> filterConfig = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			_filterViewModel = freeRentPackagesFilterViewModel
				?? throw new ArgumentNullException(nameof(freeRentPackagesFilterViewModel));

			JournalFilter = _filterViewModel;

			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;

			if(filterConfig != null)
			{
				_filterViewModel.SetAndRefilterAtOnce(filterConfig);
			}
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override IQueryOver<FreeRentPackage> ItemsQuery(IUnitOfWork unitOfWork)
		{
			FreeRentPackagesJournalNode resultAlias = null;
			EquipmentKind equipmentKindAlias = null;

			var query = unitOfWork.Session.QueryOver<FreeRentPackage>()
				.Left.JoinAlias(x => x.EquipmentKind, () => equipmentKindAlias)
				.Where(GetSearchCriterion<FreeRentPackage>(x => x.Name));

			if(!_filterViewModel.ShowArchieved)
			{
				query.Where(x => !x.IsArchive);
			}

			return query.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(() => equipmentKindAlias.Name).WithAlias(() => resultAlias.EquipmentKindName)
					.Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive))
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<FreeRentPackagesJournalNode>());
		}
	}
}
