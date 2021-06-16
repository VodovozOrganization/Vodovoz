using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class CarEventJournalViewModel : FilterableSingleEntityJournalViewModelBase<CarEvent, CarEventViewModel, CarEventJournalNode,
		CarEventFilterViewModel>
	{
		private readonly IEntityAutocompleteSelectorFactory _carSelectorFactory;
		private readonly IEntityAutocompleteSelectorFactory _carEventTypeSelectorFactory;

		public CarEventJournalViewModel(CarEventFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices, IEntityAutocompleteSelectorFactory carSelectorFactory, IEntityAutocompleteSelectorFactory carEventTypeSelectorFactory)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал событий ТС";

			_carSelectorFactory = carSelectorFactory ?? throw new ArgumentNullException(nameof(carSelectorFactory));
			_carEventTypeSelectorFactory = carEventTypeSelectorFactory ?? throw new ArgumentNullException(nameof(carEventTypeSelectorFactory));

			UpdateOnChanges(typeof(CarEvent));
		}

		protected override Func<IUnitOfWork, IQueryOver<CarEvent>> ItemsSourceQueryFunction => (uow) =>
		{
			CarEvent carEventAlias = null;
			CarEventType carEventTypeAlias = null;
			Subdivision subdivisionAlias = null;
			Employee authorAlias = null;
			Employee driverAlias = null;
			Car carAlias = null;
			CarEventJournalNode resultAlias = null;

			var authorProjection = CustomProjections.Concat_WS(
				" ",
				() => authorAlias.LastName,
				() => authorAlias.Name,
				() => authorAlias.Patronymic
			);

			var driverProjection = CustomProjections.Concat_WS(
				" ",
				() => driverAlias.LastName,
				() => driverAlias.Name,
				() => driverAlias.Patronymic
			);

			var itemsQuery = uow.Session.QueryOver(() => carEventAlias);
			itemsQuery.Left.JoinAlias(x => x.CarEventType, () => carEventTypeAlias);
			itemsQuery.Left.JoinAlias(x => x.Author, () => authorAlias);
			itemsQuery.Left.JoinAlias(x => x.Driver, () => driverAlias);
			itemsQuery.Left.JoinAlias(x => x.Car, () => carAlias);
			itemsQuery.Left.JoinAlias(() => driverAlias.Subdivision, () => subdivisionAlias);

			if(FilterViewModel.CarEventType != null)
			{
				itemsQuery.Where(x => x.CarEventType == FilterViewModel.CarEventType);
			}

			if(FilterViewModel.CreateEventDateFrom != null && FilterViewModel.CreateEventDateTo != null)
			{
				itemsQuery.Where(x => x.CreateDate >= FilterViewModel.CreateEventDateFrom &&
									  x.CreateDate <= FilterViewModel.CreateEventDateTo);
			}

			if(FilterViewModel.StartEventDateFrom != null && FilterViewModel.StartEventDateTo != null)
			{
				itemsQuery.Where(x => x.StartDate >= FilterViewModel.StartEventDateFrom &&
									  x.StartDate <= FilterViewModel.StartEventDateTo);
			}

			if(FilterViewModel.EndEventDateFrom != null && FilterViewModel.EndEventDateTo != null)
			{
				itemsQuery.Where(x => x.EndDate >= FilterViewModel.EndEventDateFrom &&
									  x.EndDate <= FilterViewModel.EndEventDateTo);
			}

			if(FilterViewModel.Author != null)
			{
				itemsQuery.Where(x => x.Author == FilterViewModel.Author);
			}

			if(FilterViewModel.Car != null)
			{
				itemsQuery.Where(x => x.Car == FilterViewModel.Car);
			}

			if(FilterViewModel.Driver != null)
			{
				itemsQuery.Where(x => x.Driver == FilterViewModel.Driver);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => carEventAlias.Id,
				() => carEventAlias.Comment,
				() => carEventTypeAlias.Name,
				() => carEventTypeAlias.ShortName,
				() => carAlias.Model,
				() => carAlias.RegistrationNumber,
				() => driverProjection)
			);

			itemsQuery
				.SelectList(list => list
					.Select(() => carEventAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => carEventAlias.CreateDate).WithAlias(() => resultAlias.CreateDate)
					.Select(() => carEventAlias.StartDate).WithAlias(() => resultAlias.StartDate)
					.Select(() => carEventAlias.EndDate).WithAlias(() => resultAlias.EndDate)
					.Select(() => carEventAlias.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => carEventTypeAlias.ShortName).WithAlias(() => resultAlias.CarEventTypeShortName)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarRegistrationNumber)
					.Select(() => carAlias.OrderNumber).WithAlias(() => resultAlias.CarOrderNumber)
					.Select(() => carAlias.TypeOfUse).WithAlias(() => resultAlias.CarTypeOfUse)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
					.Select(authorProjection).WithAlias(() => resultAlias.AuthorFullName)
					.Select(driverProjection).WithAlias(() => resultAlias.DriverFullName)
				)
				.TransformUsing(Transformers.AliasToBean<CarEventJournalNode>());

			return itemsQuery;
		};

		protected override Func<CarEventViewModel> CreateDialogFunction =>
			() => new CarEventViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices, _carSelectorFactory, _carEventTypeSelectorFactory);

		protected override Func<CarEventJournalNode, CarEventViewModel> OpenDialogFunction =>
			node => new CarEventViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices, _carSelectorFactory, _carEventTypeSelectorFactory);
	}
}
