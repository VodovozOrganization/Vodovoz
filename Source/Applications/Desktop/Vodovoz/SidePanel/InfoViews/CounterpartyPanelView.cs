﻿using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using Pango;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.Utilities;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewWidgets.Mango;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyPanelView : Bin, IPanelView
	{
		private readonly IOrderRepository _orderRepository = new OrderRepository();
		private Counterparty _counterparty;
		private IPermissionResult _counterpartyPermissionResult;
		private bool _textviewcommentBufferChanged = false;
		private string _commentText = "";

		public CounterpartyPanelView(ICommonServices commonServices)
		{
			if(commonServices == null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}
			Build();
			_counterpartyPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Counterparty));
			Configure();
		}

		private void Configure()
		{
			labelName.LineWrapMode = Pango.WrapMode.WordChar;
			labelLatestOrderDate.LineWrapMode = Pango.WrapMode.WordChar;
			textviewComment.Editable = _counterpartyPermissionResult.CanUpdate;
			_commentText = textviewComment.Buffer.Text;
			ytreeCurrentOrders.ColumnsConfig = ColumnsConfigFactory.Create<Order>()
				.AddColumn("Номер")
				.AddNumericRenderer(node => node.Id)
				.AddColumn("Дата")
				.AddTextRenderer(node => node.DeliveryDate.HasValue ? node.DeliveryDate.Value.ToShortDateString() : string.Empty)
				.AddColumn("Статус")
				.AddTextRenderer(node => node.OrderStatus.GetEnumTitle())
				.Finish();
		}
		
		private void Refresh(object changedObj)
		{
			if(InfoProvider == null)
			{
				return;
			}
			
			_counterparty = changedObj as Counterparty;
			RefreshData();
		}

		#region IPanelView implementation

		public IInfoProvider InfoProvider { get; set; }

		public void Refresh()
		{
			_counterparty = (InfoProvider as ICounterpartyInfoProvider)?.Counterparty;
			RefreshData();
		}

		private void RefreshData()
		{
			if(_counterparty == null)
			{
				buttonSaveComment.Sensitive = false;
				return;
			}

			labelName.Text = _counterparty.FullName;
			SetupPersonalManagers();
			textviewComment.Buffer.Text = _counterparty.Comment;

			textviewComment.Buffer.Changed += OnTextviewCommentBufferChanged;
			textviewComment.FocusOutEvent += OnTextviewCommentFocusOut;

			var latestOrder = _orderRepository.GetLatestCompleteOrderForCounterparty(InfoProvider.UoW, _counterparty);
			if(latestOrder != null)
			{
				var daysFromLastOrder = (DateTime.Today - latestOrder.DeliveryDate.Value).Days;
				labelLatestOrderDate.Text = string.Format(
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

			var currentOrders = _orderRepository.GetCurrentOrders(InfoProvider.UoW, _counterparty);
			ytreeCurrentOrders.SetItemsSource<Order>(currentOrders);
			vboxCurrentOrders.Visible = currentOrders.Count > 0;

			foreach(var child in PhonesTable.Children)
			{
				PhonesTable.Remove(child);
				child.Destroy();
			}
			List<Phone> phones = _counterparty.Phones.Where(p => !p.IsArchive).ToList();
			uint rowsCount = Convert.ToUInt32(phones.Count) + 1;
			PhonesTable.Resize(rowsCount, 2);
			for(uint row = 0; row < rowsCount - 1; row++)
			{
				Label label = new Label();
				label.Selectable = true;
				label.Markup = $"{phones[Convert.ToInt32(row)].LongText}";

				HandsetView handsetView = new HandsetView(phones[Convert.ToInt32(row)].DigitsNumber);

				PhonesTable.Attach(label, 0, 1, row, row + 1);
				PhonesTable.Attach(handsetView, 1, 2, row, row + 1);
			}

			Label labelAddPhone = new Label() { LabelProp = "Щёлкните чтобы\n добавить телефон-->" };
			PhonesTable.Attach(labelAddPhone, 0, 1, rowsCount - 1, rowsCount);

			Image addIcon = new Image();
			addIcon.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-add", IconSize.Menu);
			Button btn = new Button();
			btn.Image = addIcon;
			btn.Clicked += OnBtnAddPhoneClicked;
			PhonesTable.Attach(btn, 1, 2, rowsCount - 1, rowsCount);
			PhonesTable.ShowAll();
			btn.Sensitive = buttonSaveComment.Sensitive = _counterpartyPermissionResult.CanUpdate && _counterparty.Id != 0;
		}

		private void SetupPersonalManagers()
		{
			if(_counterparty?.SalesManager != null)
			{
				labelSalesManager.Visible = true;
				prefixSalesManager.Visible = true;
				labelSalesManager.Markup = _counterparty?.SalesManager.GetPersonNameWithInitials();
				prefixSalesManager.ModifyFont(FontDescription.FromString("9"));
				prefixSalesManager.QueueResize();
			}
			else
			{
				labelSalesManager.Visible = false;
				prefixSalesManager.Visible = false;
			}

			if(_counterparty?.Accountant != null)
			{
				labelAccountant.Visible = true;
				prefixAccountant.Visible = true;
				labelAccountant.Markup = _counterparty?.Accountant.GetPersonNameWithInitials();
			}
			else
			{
				labelAccountant.Visible = false;
				prefixAccountant.Visible = false;
			}

			if(_counterparty?.BottlesManager != null)
			{
				labelBottlesManager.Visible = true;
				prefixBottlesManager.Visible = true;
				labelBottlesManager.Markup = _counterparty?.BottlesManager.GetPersonNameWithInitials();
				prefixBottlesManager.ModifyFont(FontDescription.FromString("9"));
				prefixBottlesManager.QueueResize();
			}
			else
			{
				labelBottlesManager.Visible = false;
				prefixBottlesManager.Visible = false;
			}
		}

		public bool VisibleOnPanel => _counterparty != null;

		public void OnCurrentObjectChanged(object changedObject)
		{
			if(changedObject is Counterparty)
			{
				Refresh();
			}
		}

		private void SaveComment()
		{
			using(var uow =
				UnitOfWorkFactory.CreateForRoot<Counterparty>(_counterparty.Id, "Кнопка «Cохранить комментарий» на панели контрагента"))
			{
				uow.Root.Comment = textviewComment.Buffer.Text;
				uow.Save();
			}
			_textviewcommentBufferChanged = false;
		}

		protected void OnButtonSaveCommentClicked(object sender, EventArgs e)
		{
			SaveComment();
		}

		private void OnTextviewCommentBufferChanged(object sender, EventArgs e)
		{
			_textviewcommentBufferChanged = true;
		}

		private void OnTextviewCommentFocusOut(object sender, EventArgs e)
		{
			if(_commentText != textviewComment.Buffer.Text && !buttonSaveComment.HasFocus)
			{
				bool isRequiredToSaveComment = MessageDialogHelper.RunQuestionDialog("В поле комментария внесены изменения.\nСохранить изменения?");
				if(isRequiredToSaveComment)
				{
					SaveComment();
				}
			}
			textviewComment.Buffer.Text = _commentText;
		}

		protected void OnBtnAddPhoneClicked(object sender, EventArgs e)
		{
			TDIMain.MainNotebook.OpenTab(
				DialogHelper.GenerateDialogHashName<Counterparty>(_counterparty.Id),
				() =>
				{
					var dlg = new CounterpartyDlg(EntityUoWBuilder.ForOpen(_counterparty.Id), UnitOfWorkFactory.GetDefaultFactory);
					dlg.ActivateContactsTab();
					dlg.EntitySaved += (o, args) => Refresh(args.Entity);
					return dlg;
				}
			);
		}

		#endregion
	}
}
