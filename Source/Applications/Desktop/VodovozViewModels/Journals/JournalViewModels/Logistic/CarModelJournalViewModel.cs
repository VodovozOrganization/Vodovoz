using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.JournalViewModels
{
	public class CarModelJournalViewModel : EntityJournalViewModelBase<CarModel, CarModelViewModel, CarModelJournalNode>
	{
		private readonly CarModelJournalFilterViewModel _filterViewModel;

		public CarModelJournalViewModel(
			CarModelJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			Action<CarJournalFilterViewModel> filterConfiguration = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			_filterViewModel = filterViewModel
				?? throw new ArgumentNullException(nameof(filterViewModel));

			JournalFilter = _filterViewModel;

			if(filterConfiguration != null)
			{
				_filterViewModel.SetAndRefilterAtOnce(filterConfiguration);
			}

			TabName = "Журнал моделей автомобилей";

			UseSlider = true;

			UpdateOnChanges(typeof(CarModel));
		}

		protected override IQueryOver<CarModel> ItemsQuery(IUnitOfWork uow)
		{
			CarModel carModelAlias = null;
			CarModelJournalNode resultNode = null;
			CarManufacturer carManufacturerAlias = null;

			var query = uow.Session.QueryOver(() => carModelAlias)
				.Left.JoinAlias(() => carModelAlias.CarManufacturer, () => carManufacturerAlias);

			if(_filterViewModel.Archive.HasValue)
			{
				query.Where(() => carModelAlias.IsArchive == _filterViewModel.Archive);
			}

			query.Where(
				GetSearchCriterion(
					() => carModelAlias.Id,
					() => carModelAlias.Name,
					() => carManufacturerAlias.Name));

			var result = query
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultNode.Id)
					.Select(x => x.IsArchive).WithAlias(() => resultNode.IsArchive)
					.Select(x => x.Name).WithAlias(() => resultNode.Name)
					.Select(() => carManufacturerAlias.Name).WithAlias(() => resultNode.ManufactererName)
					.Select(x => x.CarTypeOfUse).WithAlias(() => resultNode.TypeOfUse))
				.OrderBy(() => carManufacturerAlias.Name).Asc
				.TransformUsing(Transformers.AliasToBean<CarModelJournalNode>());

			return result;
		}
	}
}
