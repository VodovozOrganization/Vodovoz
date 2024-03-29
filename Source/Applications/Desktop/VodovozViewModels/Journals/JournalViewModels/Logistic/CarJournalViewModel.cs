﻿using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public class CarJournalViewModel : EntityJournalViewModelBase<Car, CarViewModel, CarJournalNode>
	{
		private readonly CarJournalFilterViewModel _filterViewModel;

		public CarJournalViewModel(
			CarJournalFilterViewModel filterViewModel,
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

			filterViewModel.Journal = this;

			JournalFilter = filterViewModel;

			if(filterConfiguration != null)
			{
				filterViewModel.ConfigureWithoutFiltering(filterConfiguration);
			}

			TabName = "Журнал автомобилей";

			UpdateOnChanges(
				typeof(Car),
				typeof(CarModel),
				typeof(Employee));

			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override IQueryOver<Car> ItemsQuery(IUnitOfWork uow)
		{
			var currentDateTime = DateTime.Now;

			CarJournalNode carJournalNodeAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			CarVersion currentCarVersion = null;
			Employee driverAlias = null;
			CarManufacturer carManufacturerAlias = null;

			var query = uow.Session.QueryOver<Car>(() => carAlias)
				.Inner.JoinAlias(c => c.CarModel, () => carModelAlias)
				.Inner.JoinAlias(() => carModelAlias.CarManufacturer, () => carManufacturerAlias)
				.JoinEntityAlias(() => currentCarVersion,
					() => currentCarVersion.Car.Id == carAlias.Id
						&& currentCarVersion.StartDate <= currentDateTime
						&& (currentCarVersion.EndDate == null || currentCarVersion.EndDate >= currentDateTime))
				.Left.JoinAlias(c => c.Driver, () => driverAlias);

			if(_filterViewModel.Archive != null)
			{
				query.Where(c => c.IsArchive == _filterViewModel.Archive);
			}

			if(_filterViewModel.VisitingMasters != null)
			{
				if(_filterViewModel.VisitingMasters.Value)
				{
					query.Where(() => driverAlias.VisitingMaster);
				}
				else
				{
					query.Where(Restrictions.Disjunction()
						.Add(Restrictions.IsNull(Projections.Property(() => driverAlias.Id)))
						.Add(() => !driverAlias.VisitingMaster));
				}
			}

			if(_filterViewModel.RestrictedCarTypesOfUse != null)
			{
				query.WhereRestrictionOn(() => carModelAlias.CarTypeOfUse).IsIn(_filterViewModel.RestrictedCarTypesOfUse.ToArray());
			}

			if(_filterViewModel.ExcludedCarTypesOfUse != null && _filterViewModel.ExcludedCarTypesOfUse.Any())
			{
				query.WhereRestrictionOn(() => carModelAlias.CarTypeOfUse).Not.IsIn(_filterViewModel.ExcludedCarTypesOfUse.ToArray());
			}

			if(_filterViewModel.RestrictedCarOwnTypes != null)
			{
				query.WhereRestrictionOn(() => currentCarVersion.CarOwnType).IsIn(_filterViewModel.RestrictedCarOwnTypes.ToArray());
			}

			if(_filterViewModel.CarModel != null)
			{
				query.Where(() => carModelAlias.Id == _filterViewModel.CarModel.Id);
			}

			query.Where(GetSearchCriterion(
				() => carAlias.Id,
				() => carManufacturerAlias.Name,
				() => carModelAlias.Name,
				() => carAlias.RegistrationNumber,
				() => driverAlias.Name,
				() => driverAlias.LastName,
				() => driverAlias.Patronymic));

			var result = query
				.SelectList(list => list
					.Select(c => c.Id).WithAlias(() => carJournalNodeAlias.Id)
					.Select(() => carManufacturerAlias.Name).WithAlias(() => carJournalNodeAlias.ManufacturerName)
					.Select(() => carModelAlias.Name).WithAlias(() => carJournalNodeAlias.ModelName)
					.Select(c => c.RegistrationNumber).WithAlias(() => carJournalNodeAlias.RegistrationNumber)
					.Select(c => c.IsArchive).WithAlias(() => carJournalNodeAlias.IsArchive)
					.Select(CustomProjections.Concat_WS(" ",
						Projections.Property(() => driverAlias.LastName),
						Projections.Property(() => driverAlias.Name),
						Projections.Property(() => driverAlias.Patronymic)))
					.WithAlias(() => carJournalNodeAlias.DriverName))
				.OrderBy(() => carAlias.Id).Asc
				.TransformUsing(Transformers.AliasToBean<CarJournalNode>());

			return result;
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}
