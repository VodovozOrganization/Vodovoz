using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using System;
using System.Linq;
using Vodovoz.Dialogs.Sale;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.Dialogs.Mango.Talks
{
	public class UnknowTalkViewModel : TalkViewModelBase, IDisposable
	{
		private readonly ITdiCompatibilityNavigation _tdiNavigation;
		private readonly IInteractiveQuestion _interactive;
		private readonly IUnitOfWork _uow;
		private IPage<CounterpartyJournalViewModel> _counterpartyJournalPage;
		
		public UnknowTalkViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			ITdiCompatibilityNavigation navigation, 
			IInteractiveQuestion interactive,
			MangoManager manager) : base(navigation, manager)
		{
			_tdiNavigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			_interactive = interactive ?? throw new ArgumentNullException(nameof(interactive));
			_uow = (unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory)))
				.CreateWithoutRoot();
		}

		#region Действия View

		public void SelectNewConterparty()
		{
			var page = _tdiNavigation.OpenTdiTab<CounterpartyDlg, Phone>(this, ActiveCall.Phone);
			var tab = page.TdiTab as CounterpartyDlg;
			page.PageClosed += NewCounerpatry_PageClosed;
		}

		public void SelectExistConterparty()
		{
			_counterpartyJournalPage = NavigationManager.OpenViewModel<CounterpartyJournalViewModel>(null);
			_counterpartyJournalPage.ViewModel.SelectionMode = QS.Project.Journal.JournalSelectionMode.Single;
			_counterpartyJournalPage.ViewModel.OnEntitySelectedResult -= OnExistingCounterpartyPageClosed;
			_counterpartyJournalPage.ViewModel.OnEntitySelectedResult += OnExistingCounterpartyPageClosed;
		}

		void NewCounerpatry_PageClosed(object sender, PageClosedEventArgs e)
		{ 
			if(e.CloseSource == CloseSource.Save) {
				Counterparty client = ((sender as TdiTabPage).TdiTab as CounterpartyDlg).Counterparty;
				if(client != null) {
					this.Close(true, CloseSource.External);
					MangoManager.AddCounterpartyToCall(client.Id);
				} else
					throw new Exception("При сохранении контрагента произошла ошибка, попробуйте снова." + "\n Сообщение для поддержки : UnknowTalkViewModel.NewCounterparty_PageClose()");
			}
		}

		void OnExistingCounterpartyPageClosed(object sender, QS.Project.Journal.JournalSelectedNodesEventArgs e)
		{
			var counterpartyNode = e.SelectedNodes.First() as CounterpartyJournalNode;
			Counterparty client = _uow.GetById<Counterparty>(counterpartyNode.Id);
			if(_interactive.Question($"Добавить телефон к контрагенту {client.Name} ?", "Телефон контрагента")) {
				if(!client.Phones.Any(phone => phone.DigitsNumber == ActiveCall.Phone.DigitsNumber)) 
				{
					var phone = ActiveCall.Phone;
					phone.Counterparty = client;
					_uow.Save(phone);
					_uow.Commit();
				}
			}
			this.Close(true, CloseSource.External);
			MangoManager.AddCounterpartyToCall(client.Id);
		}

		public void CreateComplaintCommand()
		{
			_tdiNavigation.OpenViewModel<CreateComplaintViewModel, IEntityUoWBuilder, string>(
				null, EntityUoWBuilder.ForCreate(), "+7" + ActiveCall.Phone.Number);
		}

		public void StockBalanceCommand()
		{
			NavigationManager.OpenViewModel<NomenclatureStockBalanceJournalViewModel>(null);
		}

		public void CostAndDeliveryIntervalCommand()
		{
			_tdiNavigation.OpenTdiTab<DeliveryPriceDlg>(null);
		}

		public void Dispose()
		{
			if(_counterpartyJournalPage?.ViewModel != null)
			{
				_counterpartyJournalPage.ViewModel.OnEntitySelectedResult -= OnExistingCounterpartyPageClosed;
			}

			_uow?.Dispose();
		}

		#endregion
	}
}
