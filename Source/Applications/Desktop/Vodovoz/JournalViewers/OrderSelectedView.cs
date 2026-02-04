using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Autofac;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Infrastructure;
using Vodovoz.JournalViewModels;

namespace Vodovoz.JournalViewers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderSelectedView : Gtk.Bin, INotifyPropertyChanged
	{
		private Counterparty _client;

		public OrderSelectedView()
		{
			this.Build();
		}

		public event EventHandler<int> OrderActivated;
		public event PropertyChangedEventHandler PropertyChanged;

		public Counterparty Client
		{
			get => _client;
			set
			{
				if(_client != value)
				{
					_client = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Client)));
				}
			}
		}

		public INavigationManager NavigationManager { get; private set; }

		private ILifetimeScope _lifetimeScope;

		public IUnitOfWork UoW { get; private set; }

		public List<SelectedOrdersDocumentVMNode> Documents { get; set; } = new List<SelectedOrdersDocumentVMNode>();

		public void Config(IUnitOfWork uow, Counterparty client, ITdiTab master, INavigationManager navigationManager, ILifetimeScope lifetimeScope)
		{
			UoW = uow;
			Client = client;
			NavigationManager = navigationManager;
			_lifetimeScope = lifetimeScope;
			var colorGreen = GdkColors.SuccessText;
			var basePrimary = GdkColors.PrimaryBase;

			datatreeviewOrderDocuments.ColumnsConfig = FluentColumnsConfig<SelectedOrdersDocumentVMNode>
				.Create()
				.AddColumn("Выбрать").AddToggleRenderer(node => node.Selected).Editing()
				.AddColumn("Заказ").AddNumericRenderer(node => node.OrderId).Editing(false)
				.AddColumn("Дата").AddTextRenderer(node => node.OrderDate.ToString("d"))
				.AddColumn("Клиент").AddTextRenderer(node => node.ClientName)
				.AddColumn("Документ").AddTextRenderer(node => node.DocumentType.GetAttribute<DisplayAttribute>().Name)
				.AddColumn("Адрес").AddTextRenderer(node => node.AddressString)
				.RowCells()
				.AddSetter<CellRenderer>((c, n) => {
					if(n.Selected) {
						c.CellBackgroundGdk = colorGreen;
					} else {
						c.CellBackgroundGdk = basePrimary;
					}
				})
				.Finish();

			entryCounterparty.ViewModel = new LegacyEEVMBuilderFactory<OrderSelectedView>(master, this, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Client)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();

			entryCounterparty.ViewModel.ChangedByUser += (sender, e) => { UpdateNodes(); };
			yvalidatedentry1.ValidationMode = QSWidgetLib.ValidationType.numeric;
			UpdateNodes();
		}

		private void UpdateNodes()
		{
			SelectedOrdersDocumentVMNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Counterparty counterpartyAlias = null;
			OrderDocument orderDocumentAlias = null;
			DeliveryPoint deliveryPointAlias = null;

			var query = UoW.Session.QueryOver<OrderDocument>(() => orderDocumentAlias);

			if(Client != null) {
				query.Where(() => counterpartyAlias.Id == Client.Id);
			}

			int orderId = default(int);
			if(int.TryParse(yvalidatedentry1.Text, out orderId)){
				//query.Where(() => orderAlias.Id == orderId);
				query.WhereRestrictionOn(() => orderAlias.Id).IsLike(orderId);
			}

			Documents = query
				.JoinAlias(() => orderDocumentAlias.Order, () => orderAlias)
				.JoinAlias(() => orderAlias.Client, () => counterpartyAlias)
				.JoinAlias(() => orderAlias.DeliveryPoint, () => deliveryPointAlias)
				.Where(() => 
				          orderDocumentAlias.GetType() == typeof(BillDocument)
					   || orderDocumentAlias.GetType() == typeof(DoneWorkDocument)
					   || orderDocumentAlias.GetType() == typeof(EquipmentTransferDocument)
					   || orderDocumentAlias.GetType() == typeof(LetterOfDebtDocument)
					   || orderDocumentAlias.GetType() == typeof(InvoiceBarterDocument)
					   || orderDocumentAlias.GetType() == typeof(InvoiceDocument)
				       || orderDocumentAlias.GetType() == typeof(InvoiceContractDoc)
					   || orderDocumentAlias.GetType() == typeof(UPDDocument)
					   || orderDocumentAlias.GetType() == typeof(DriverTicketDocument)
					   || orderDocumentAlias.GetType() == typeof(Torg12Document)
					   || orderDocumentAlias.GetType() == typeof(ShetFacturaDocument)
					   || orderDocumentAlias.GetType() == typeof(SpecialBillDocument)
					   || orderDocumentAlias.GetType() == typeof(SpecialUPDDocument)
				      )
				.SelectList(list => list
				   .Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
				   .Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.OrderDate)
				   .Select(() => counterpartyAlias.Id).WithAlias(() => resultAlias.ClientId)
				   .Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.ClientName)
				   .Select(() => orderDocumentAlias.Id).WithAlias(() => resultAlias.DocumentId)
				   .Select(() => orderDocumentAlias.GetType()).WithAlias(() => resultAlias.DocumentTypeString)
				   .Select(() => deliveryPointAlias.CompiledAddress).WithAlias(() => resultAlias.AddressString)
				).OrderBy(() => orderAlias.DeliveryDate).Desc
				.TransformUsing(Transformers.AliasToBean<SelectedOrdersDocumentVMNode>())
				.List<SelectedOrdersDocumentVMNode>().ToList();
			
			datatreeviewOrderDocuments.SetItemsSource(Documents);
		}

		protected void OnYvalidatedentry1Changed(object sender, EventArgs e)
		{
			UpdateNodes();
		}

		public List<SelectedOrdersDocumentVMNode> GetSelectedDocuments()
		{
			return Documents.Where(x => x.Selected).ToList();
		}

		protected void OnDatatreeviewOrderDocumentsRowActivated(object o, RowActivatedArgs args)
		{
			var selectedItem = datatreeviewOrderDocuments.GetSelectedObject() as SelectedOrdersDocumentVMNode;
			if(selectedItem == null) {
				return;
			}

			OrderActivated?.Invoke(this, selectedItem.DocumentId);
		}
	}

	public class SelectedOrdersDocumentVMNode
	{
		public bool Selected { get; set; } = false;
		public int OrderId { get; set; }
		public DateTime OrderDate { get; set; }
		public int ClientId { get; set; }
		public string ClientName { get; set; }
		public int DocumentId { get; set; }
		public OrderDocumentType DocumentType {
			get{
				OrderDocumentType result;
				Enum.TryParse<OrderDocumentType>(DocumentTypeString, out result);
				return result;
			}
		}
		public string DocumentTypeString { get; set; }
		public DateTime DocumentDate { get; set; }
		public string AddressString { get; set; }

	}
}
