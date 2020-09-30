using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.Utilities;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories.Orders;
using Vodovoz.Repository.Operations;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModelBased;
using Vodovoz.ViewWidgets.Mango;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyPanelView : Gtk.Bin, IPanelView
	{		
		Counterparty Counterparty{get;set;}

		public CounterpartyPanelView()
		{
			this.Build();
			Configure();
		}

		void Configure()
		{
			labelName.LineWrapMode = Pango.WrapMode.WordChar;
			labelLatestOrderDate.LineWrapMode = Pango.WrapMode.WordChar;
			ytreeCurrentOrders.ColumnsConfig = ColumnsConfigFactory.Create<Order>()
				.AddColumn("Номер")
					.AddNumericRenderer(node => node.Id)
				.AddColumn("Дата")
				.AddTextRenderer(node => node.DeliveryDate.HasValue ? node.DeliveryDate.Value.ToShortDateString() : String.Empty)
				.AddColumn("Статус")
					.AddTextRenderer(node => node.OrderStatus.GetEnumTitle())
				.Finish();			
		}

		#region IPanelView implementation
		public IInfoProvider InfoProvider{ get; set; }

		public void Refresh()
		{
			Counterparty = (InfoProvider as ICounterpartyInfoProvider)?.Counterparty;
			if(Counterparty == null) {
				buttonSaveComment.Sensitive = false;
				return;
			}
			buttonSaveComment.Sensitive = true;
			labelName.Text = Counterparty.FullName;
			textviewComment.Buffer.Text = Counterparty.Comment;

			var debt = MoneyRepository.GetCounterpartyDebt(InfoProvider.UoW, Counterparty);
			string labelDebtFormat 		 = "<span {0}>{1}</span>";
			string backgroundDebtColor 	 = "";
			if (debt > 0)
			{
				backgroundDebtColor 	 = "background=\"red\"";
				ylabelDebtInfo.LabelProp = "Долг:";
			}
			if (debt < 0)
			{
				backgroundDebtColor 	 = "background=\"lightgreen\"";
				ylabelDebtInfo.LabelProp = "Баланс:";
				debt 	= -debt;
			}
			labelDebt.Markup = string.Format(labelDebtFormat, backgroundDebtColor, CurrencyWorks.GetShortCurrencyString(debt));

			var latestOrder = OrderRepository.GetLatestCompleteOrderForCounterparty(InfoProvider.UoW, Counterparty);
			if (latestOrder != null)
			{
				var daysFromLastOrder = (DateTime.Today - latestOrder.DeliveryDate.Value).Days;
				labelLatestOrderDate.Text = String.Format(
					"{0} ({1} {2} назад)",
					latestOrder.DeliveryDate.Value.ToShortDateString(),
					daysFromLastOrder,
					NumberToTextRus.Case(daysFromLastOrder, "день", "дня", "дней")
				);
			}
			else
			{
				labelLatestOrderDate.Text = "(Выполненных заказов нет)";
			}
			var currentOrders = OrderRepository.GetCurrentOrders(InfoProvider.UoW, Counterparty);
			ytreeCurrentOrders.SetItemsSource<Order>(currentOrders);
			vboxCurrentOrders.Visible = currentOrders.Count > 0;
			if(Counterparty.Phones.Count > 0) {
				uint rowsCount = Convert.ToUInt32(Counterparty.Phones.Count)+1;
				PhonesTable.Resize(rowsCount, 2);
				for(uint row = 0; row < rowsCount - 1; row++) {
					Label label = new Label();
					label.Markup = $"<b>+{Counterparty.Phones[Convert.ToInt32(row)].Number}</b>";

					HandsetView handsetView = new HandsetView(Counterparty.Phones[Convert.ToInt32(row)].DigitsNumber);

					PhonesTable.Attach(label, 0, 1, row, row + 1);
					PhonesTable.Attach(handsetView, 1, 2, row, row + 1);
				}

				Label labelAddPhone = new Label() { LabelProp = "Щёлкните чтоб\n добавить телефон-->" };
				PhonesTable.Attach(labelAddPhone, 0, 1, rowsCount - 1, rowsCount);

				Image addIcon = new Image();
				addIcon.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-add", IconSize.Menu);
				Button btn = new Button();
				btn.Image = addIcon;
				btn.ClientEvent += OnBtnAddPhoneClicked;
				PhonesTable.Attach(btn, 1, 2, rowsCount - 1, rowsCount);
			}
			PhonesTable.ShowAll();
		}

		public bool VisibleOnPanel
		{
			get
			{
				return Counterparty != null;
			}
		}
			
		public void OnCurrentObjectChanged(object changedObject)
		{			
			if (changedObject is Counterparty)
			{
				Refresh();
			}
		}

		protected void OnButtonSaveCommentClicked(object sender, EventArgs e)
		{
			using(var uow = UnitOfWorkFactory.CreateForRoot<Counterparty>(Counterparty.Id, "Кнопка «Cохранить комментарий» на панели контрагента"))
			{
				uow.Root.Comment = textviewComment.Buffer.Text;
				uow.Save();
			}
		}

		protected void OnBtnAddPhoneClicked(object sender, EventArgs e)
		{
			TDIMain.MainNotebook.OpenTab(
				DialogHelper.GenerateDialogHashName<Counterparty>(Counterparty.Id),
				() => {
					var dlg = new CounterpartyDlg(EntityUoWBuilder.ForOpenInChildUoW(Counterparty.Id, InfoProvider.UoW), UnitOfWorkFactory.GetDefaultFactory);
					dlg.ActivateContactsTab();
					dlg.TabClosed += (senderObject, eventArgs) => { this.Refresh(); };
					return dlg;
				}
			);
		}
		#endregion
	}
}