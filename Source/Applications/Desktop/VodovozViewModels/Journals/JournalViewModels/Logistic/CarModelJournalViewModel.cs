using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Controllers;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.JournalViewModels
{
	public class CarModelJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<CarModel, CarModelViewModel, CarModelJournalNode, CarModelJournalFilterViewModel>
	{
		private readonly ICarManufacturerJournalFactory _carManufacturerJournalFactory;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;

		public CarModelJournalViewModel(
			CarModelJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ICarManufacturerJournalFactory carManufacturerJournalFactory,
			IRouteListProfitabilityController routeListProfitabilityController,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false)
			: base(filterViewModel, unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			_carManufacturerJournalFactory =
				carManufacturerJournalFactory ?? throw new ArgumentNullException(nameof(carManufacturerJournalFactory));
			_routeListProfitabilityController =
				routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			
			TabName = "Журнал моделей автомобилей";
			UpdateOnChanges(typeof(CarModel));
		}

		protected override Func<IUnitOfWork, IQueryOver<CarModel>> ItemsSourceQueryFunction => (uow) =>
		{
			CarModel carModelAlias = null;
			CarModelJournalNode resultNode = null;
			CarManufacturer carManufacturerAlias = null;

			var query = uow.Session.QueryOver(() => carModelAlias)
				.Left.JoinAlias(() => carModelAlias.CarManufacturer, () => carManufacturerAlias);

			if(FilterViewModel.Archive.HasValue)
			{
				query.Where(() => carModelAlias.IsArchive == FilterViewModel.Archive);
			}

			query.Where(
				GetSearchCriterion(
					() => carModelAlias.Id,
					() => carModelAlias.Name,
					() => carManufacturerAlias.Name
				)
			);

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
		};

		protected override Func<CarModelViewModel> CreateDialogFunction => () =>
			new CarModelViewModel(
				EntityUoWBuilder.ForCreate(),
				UnitOfWorkFactory,
				commonServices,
				_carManufacturerJournalFactory,
				_routeListProfitabilityController);

		protected override Func<CarModelJournalNode, CarModelViewModel> OpenDialogFunction => node =>
			new CarModelViewModel(
				EntityUoWBuilder.ForOpen(node.Id),
				UnitOfWorkFactory,
				commonServices,
				_carManufacturerJournalFactory,
				_routeListProfitabilityController);
	}
}
