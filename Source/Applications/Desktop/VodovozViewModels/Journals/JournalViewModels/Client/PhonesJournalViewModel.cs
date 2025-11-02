using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Contacts;
using Vodovoz.ViewModels.Journals.FilterViewModels.Counterparties;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.ViewModels.Counterparty;
using VodovozBusiness.Domain.Contacts;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Client
{
	public class PhonesJournalViewModel : FilterableSingleEntityJournalViewModelBase<Phone, PhoneViewModel, PhonesJournalNode, PhonesJournalFilterViewModel>
	{
		public PhonesJournalViewModel(
			PhonesJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			Action<PhonesJournalFilterViewModel> filterConfig = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал телефонов";

			filterViewModel.Journal = this;

			if(filterConfig != null)
			{
				filterConfig.Invoke(filterViewModel);
			}

			UpdateOnChanges(typeof(Phone));
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
		}

		protected override Func<IUnitOfWork, IQueryOver<Phone>> ItemsSourceQueryFunction => (uow) =>
		{
			Phone phoneAlias = null;
			PhoneType phoneTypeAlias = null;
			PhonesJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => phoneAlias)
			.Left.JoinAlias(x => x.PhoneType, () => phoneTypeAlias);


			if(FilterViewModel.Counterparty != null)
			{
				if(FilterViewModel.DeliveryPoint == null)
				{
					itemsQuery.Where(x => x.Counterparty.Id == FilterViewModel.Counterparty.Id);
				}
				else
				{
					itemsQuery.Where(x => x.Counterparty.Id == FilterViewModel.Counterparty.Id
						|| x.DeliveryPoint.Id == FilterViewModel.DeliveryPoint.Id);
				}
			}

			if(FilterViewModel.Employee != null)
			{
				itemsQuery.Where(x => x.Employee.Id == FilterViewModel.Employee.Id);
			}

			if(!FilterViewModel.ShowArchive)
			{
				itemsQuery.Where(x => !x.IsArchive);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => phoneAlias.Id,
				() => phoneAlias.DigitsNumber,
				() => phoneAlias.Number)
			);

			itemsQuery.OrderBy(() => phoneAlias.DeliveryPoint.Id).Desc();

			itemsQuery
				.SelectList(list => list
					.SelectGroup(() => phoneAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => phoneAlias.Number).WithAlias(() => resultAlias.Phone)
					.Select(() => phoneAlias.Counterparty.Id).WithAlias(() => resultAlias.CounterpartyId)
					.Select(() => phoneAlias.DeliveryPoint.Id).WithAlias(() => resultAlias.DeliveryPointId)
					.Select(() => phoneTypeAlias.Name).WithAlias(() => resultAlias.PhoneType)
				)
				.TransformUsing(Transformers.AliasToBean<PhonesJournalNode>());

			return itemsQuery;
		};

		protected override Func<PhoneViewModel> CreateDialogFunction => () =>
			throw new NotSupportedException("Не поддерживается создание телефона из журнала");

		protected override Func<PhonesJournalNode, PhoneViewModel> OpenDialogFunction => node =>
			throw new NotSupportedException("Не поддерживается открытие телефона из журнала");
	}
}
