using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Logistic.MileagesWriteOff
{
	public class MileageWriteOffJournalViewModel : EntityJournalViewModelBase<MileageWriteOff, MileageWriteOffViewModel, MileageWriteOffJournalNode>
	{
		private readonly MileageWriteOffJournalFilterViewModel _filterViewModel;

		public MileageWriteOffJournalViewModel(
			MileageWriteOffJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			Action<MileageWriteOffJournalFilterViewModel> filterConfig = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));

			Title = "Списание километража без МЛ";

			UpdateOnChanges(typeof(MileageWriteOff));

			if(filterConfig != null)
			{
				_filterViewModel.ConfigureWithoutFiltering(filterConfig);
			}

			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override IQueryOver<MileageWriteOff> ItemsQuery(IUnitOfWork uow)
		{
			MileageWriteOff mileageWriteOffAlias = null;
			Car carAlias = null;
			Employee driverAlias = null;
			Employee authorAlias = null;
			MileageWriteOffJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => mileageWriteOffAlias)
				.Left.JoinAlias(() => mileageWriteOffAlias.Car, () => carAlias)
				.Left.JoinAlias(() => mileageWriteOffAlias.Driver, () => driverAlias)
				.Left.JoinAlias(() => mileageWriteOffAlias.Author, () => authorAlias);

			if(_filterViewModel.WriteOffDateFrom.HasValue)
			{
				query.Where(() => mileageWriteOffAlias.WriteOffDate >= _filterViewModel.WriteOffDateFrom.Value);
			}

			if(_filterViewModel.WriteOffDateTo.HasValue)
			{
				query.Where(() => mileageWriteOffAlias.WriteOffDate <= _filterViewModel.WriteOffDateTo.Value);
			}

			if(_filterViewModel.Car != null)
			{
				query.Where(() => carAlias == _filterViewModel.Car);
			}

			if(_filterViewModel.Driver != null)
			{
				query.Where(() => driverAlias == _filterViewModel.Driver);
			}

			if(_filterViewModel.Author != null)
			{
				query.Where(() => authorAlias == _filterViewModel.Author);
			}

			query.Where(GetSearchCriterion(
				() => mileageWriteOffAlias.Id,
				() => carAlias.RegistrationNumber,
				() => driverAlias.FullName,
				() => authorAlias.FullName
			));

			query = query.SelectList(list => list
				.Select(() => mileageWriteOffAlias.Id).WithAlias(() => resultAlias.Id)
				.Select(() => mileageWriteOffAlias.DistanceKm).WithAlias(() => resultAlias.DistanceKm)
				.Select(() => mileageWriteOffAlias.WriteOffDate).WithAlias(() => resultAlias.WriteOffDate)
				.Select(() => mileageWriteOffAlias.CreationDate).WithAlias(() => resultAlias.CreateDate)
				.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.CarRegNumber)
				.Select(() => driverAlias.FullName).WithAlias(() => resultAlias.DriverName)
				.Select(() => authorAlias.FullName).WithAlias(() => resultAlias.AuthorName))
			.TransformUsing(Transformers.AliasToBean<MileageWriteOffJournalNode>());

			return query;
		}
	}
}
