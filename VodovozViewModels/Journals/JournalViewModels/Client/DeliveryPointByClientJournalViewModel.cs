using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Client
{
	/// <summary>
	/// Данный журнал главным образом необходим для выбора точки доставки конкретного клиента в различных entityVMentry без колонок с лишней информацией
	/// </summary>
	public class DeliveryPointByClientJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<DeliveryPoint, DeliveryPointViewModel, DeliveryPointByClientJournalNode, DeliveryPointJournalFilterViewModel>
	{
		private readonly IDeliveryPointViewModelFactory _deliveryPointViewModelFactory;

		public DeliveryPointByClientJournalViewModel(
			IDeliveryPointViewModelFactory deliveryPointViewModelFactory,
			DeliveryPointJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			bool hideJournalForOpen,
			bool hideJournalForCreate)
			: base(filterViewModel, unitOfWorkFactory, commonServices, hideJournalForOpen, hideJournalForCreate)
		{
			TabName = "Журнал точек доставки клиента";

			_deliveryPointViewModelFactory =
				deliveryPointViewModelFactory ?? throw new ArgumentNullException(nameof(deliveryPointViewModelFactory));

			UpdateOnChanges(
				typeof(Counterparty),
				typeof(DeliveryPoint)
			);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
		}

		protected override Func<IUnitOfWork, IQueryOver<DeliveryPoint>> ItemsSourceQueryFunction => (uow) =>
		{
			DeliveryPoint deliveryPointAlias = null;
			DeliveryPointByClientJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => deliveryPointAlias);

			if(FilterViewModel?.RestrictOnlyActive == true)
			{
				query.Where(() => deliveryPointAlias.IsActive);
			}

			if(FilterViewModel?.Counterparty != null)
			{
				query.Where(() => deliveryPointAlias.Counterparty.Id == FilterViewModel.Counterparty.Id);
			}

			if(FilterViewModel?.RestrictOnlyNotFoundOsm == true)
			{
				query.Where(() => deliveryPointAlias.FoundOnOsm == false);
			}

			if(FilterViewModel?.RestrictOnlyWithoutStreet == true)
			{
				query.Where(() => deliveryPointAlias.Street == null || deliveryPointAlias.Street == " ");
			}

			query.Where(GetSearchCriterion(
				() => deliveryPointAlias.Id,
				() => deliveryPointAlias.CompiledAddress
			));

			var resultQuery = query
				.SelectList(list => list
					.Select(() => deliveryPointAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.CompiledAddress)
					.Select(() => deliveryPointAlias.IsActive).WithAlias(() => resultAlias.IsActive)
				)
				.TransformUsing(Transformers.AliasToBean<DeliveryPointByClientJournalNode>());

			return resultQuery;
		};

		protected override Func<DeliveryPointViewModel> CreateDialogFunction => () =>
			_deliveryPointViewModelFactory.GetForCreationDeliveryPointViewModel(FilterViewModel.Counterparty);

		protected override Func<DeliveryPointByClientJournalNode, DeliveryPointViewModel> OpenDialogFunction => (node) =>
			_deliveryPointViewModelFactory.GetForOpenDeliveryPointViewModel(node.Id);
	}
}
