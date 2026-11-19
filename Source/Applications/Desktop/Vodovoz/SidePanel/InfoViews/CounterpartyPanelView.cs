using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using Microsoft.Extensions.Logging;
using Pango;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using QS.Utilities;
using Stetic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewWidgets.Mango;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.SidePanel.InfoViews
{
	[ToolboxItem(true)]
	public partial class CounterpartyPanelView : Bin, IPanelView
	{
		private const int _phoneTextWidthChars = 20;
		private const int _phoneButtonWidthRequest = 32;
		
		private readonly IOrderRepository _orderRepository;
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository;
		private readonly ILogger<CounterpartyPanelView> _logger;
		private readonly ICommonServices _commonServices;
		private readonly Dictionary<CounterpartyFrom, Label> _ipzRegisteredLabels = new Dictionary<CounterpartyFrom, Label>();
		private Counterparty _counterparty;
		private IPermissionResult _counterpartyPermissionResult;
		private bool _textviewcommentBufferChanged = false;

		public CounterpartyPanelView(
			ILogger<CounterpartyPanelView> logger,
			ICommonServices commonServices,
			IOrderRepository orderRepository,
			IExternalCounterpartyRepository externalCounterpartyRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			Build();
			_counterpartyPermissionResult = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Counterparty));
			Configure();
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_externalCounterpartyRepository = externalCounterpartyRepository ?? throw new ArgumentNullException(nameof(externalCounterpartyRepository));
		}

		private void Configure()
		{
			labelName.LineWrapMode = WrapMode.WordChar;
			labelLatestOrderDate.LineWrapMode = WrapMode.WordChar;
			textviewComment.Editable = _counterpartyPermissionResult.CanUpdate;
			ytreeCurrentOrders.ColumnsConfig = ColumnsConfigFactory.Create<Order>()
				.AddColumn("Номер")
				.AddNumericRenderer(node => node.Id)
				.AddColumn("Дата")
				.AddTextRenderer(node => node.DeliveryDate.HasValue ? node.DeliveryDate.Value.ToShortDateString() : string.Empty)
				.AddColumn("Статус")
				.AddTextRenderer(node => node.OrderStatus.GetEnumTitle())
				.Finish();

			textviewComment.Buffer.Changed += OnTextviewCommentBufferChanged;
			textviewComment.FocusOutEvent += OnTextviewCommentFocusOut;

			logisticsRequirementsView.ViewModel = new LogisticsRequirementsViewModel(GetLogisticsRequirements(), _commonServices);

			ConfigureRegisteredIpzTable();
		}

		private void ConfigureRegisteredIpzTable()
		{
			var ipzValues = Enum.GetValues(typeof(CounterpartyFrom))
				.Cast<CounterpartyFrom>()
				.ToArray();

			tblRegisteredIpz.Resize((uint)(ipzValues.Length + 1), 2);

			var headerIpz = new Label("<b>ИПЗ</b>")
			{
				UseMarkup = true,
				Xalign = 0f
			};

			var headerRegistered = new Label("<b>Да/Нет</b>")
			{
				UseMarkup = true,
				Xalign = 0f
			};

			tblRegisteredIpz.Attach(headerIpz, 0, 1, 0, 1, AttachOptions.Fill, AttachOptions.Fill, 0, 0);
			tblRegisteredIpz.Attach(headerRegistered, 1, 2, 0, 1, AttachOptions.Fill, AttachOptions.Fill, 0, 0);

			for(var i = 0; i < ipzValues.Length; i++)
			{
				var ipz = ipzValues[i];

				var ipzLabel = new Label(ipz.GetEnumTitle())
				{
					Xalign = 0f
				};

				var registeredLabel = new Label("Нет")
				{
					Xalign = 0f
				};

				tblRegisteredIpz.Attach(ipzLabel, 0, 1, (uint)(i + 1), (uint)(i + 2), AttachOptions.Fill, AttachOptions.Fill, 0, 0);
				tblRegisteredIpz.Attach(registeredLabel, 1, 2, (uint)(i + 1), (uint)(i + 2), AttachOptions.Fill, AttachOptions.Fill, 0, 0);

				_ipzRegisteredLabels[ipz] = registeredLabel;
			}

			tblRegisteredIpz.ShowAll();
		}

		private void UpdateRegisteredIpzTable()
		{
			HashSet<CounterpartyFrom> registeredIpz = null;

			if(_counterparty != null && _counterparty.Id != 0)
			{
				registeredIpz = new HashSet<CounterpartyFrom>(
					_externalCounterpartyRepository
						.GetPersonalCounterpartyExternalUsersInfo(InfoProvider.UoW, _counterparty.Id)
						.Select(x => x.CounterpartyFrom));
			}

			foreach(var ipzRegisteredLabel in _ipzRegisteredLabels)
			{
				ipzRegisteredLabel.Value.Text = registeredIpz != null && registeredIpz.Contains(ipzRegisteredLabel.Key)
					? "Да"
					: "Нет";
			}
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

		#region LogisticsRequirements
		private LogisticsRequirements GetLogisticsRequirements()
		{
			return _counterparty?.LogisticsRequirements ?? new LogisticsRequirements();
		}
		private void SetLogisticsRequirementsCheckboxes()
		{
			var requirements = GetLogisticsRequirements();

			logisticsRequirementsView.ViewModel.Entity.CopyRequirementPropertiesValues(requirements);
		}
		private void SaveLogisticsRequirements()
		{
			using(var uow =
					ServicesConfig.UnitOfWorkFactory.CreateForRoot<Counterparty>(_counterparty.Id, "Кнопка «Cохранить требования к логистике на панели контрагента"))
			{
				if(uow.Root.LogisticsRequirements == null)
				{
					uow.Root.LogisticsRequirements = new LogisticsRequirements();
				}

				uow.Root.LogisticsRequirements.ForwarderRequired = logisticsRequirementsView.ViewModel.Entity.ForwarderRequired;
				uow.Root.LogisticsRequirements.DocumentsRequired = logisticsRequirementsView.ViewModel.Entity.DocumentsRequired;
				uow.Root.LogisticsRequirements.RussianDriverRequired = logisticsRequirementsView.ViewModel.Entity.RussianDriverRequired;
				uow.Root.LogisticsRequirements.PassRequired = logisticsRequirementsView.ViewModel.Entity.PassRequired;
				uow.Root.LogisticsRequirements.LargusRequired = logisticsRequirementsView.ViewModel.Entity.LargusRequired;

				uow.Save();
			}
		}
		#endregion

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
				UpdateRegisteredIpzTable();
				return;
			}

			labelName.Text = _counterparty.FullName;
			SetupPersonalManagers();
			textviewComment.Buffer.Text = _counterparty.Comment;
			_textviewcommentBufferChanged = false;
			UpdateRegisteredIpzTable();

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
			PhonesTable.Resize(rowsCount, 1);
			for(uint row = 0; row < rowsCount - 1; row++)
			{
				Label label = new Label();
				label.Selectable = true;
				label.LineWrap = true;
				label.LineWrapMode = WrapMode.WordChar;
				label.WidthChars = _phoneTextWidthChars;
				label.MaxWidthChars = _phoneTextWidthChars;
				label.Markup = $"{phones[Convert.ToInt32(row)].LongText}";

				HandsetView handsetView = new HandsetView(phones[Convert.ToInt32(row)].DigitsNumber);
				handsetView.WidthRequest = _phoneButtonWidthRequest;

				var phoneRow = new HBox();
				phoneRow.PackStart(label, true, true, 0);
				phoneRow.PackEnd(handsetView, false, false, 0);

				PhonesTable.Attach(phoneRow, 0, 1, row, row + 1);
			}

			Label labelAddPhone = new Label() { LabelProp = "Щёлкните чтобы\n добавить телефон-->" };

			Image addIcon = new Image();
			addIcon.Pixbuf = IconLoader.LoadIcon(this, "gtk-add", IconSize.Menu);
			Button btn = new Button();
			btn.Image = addIcon;
			btn.WidthRequest = _phoneButtonWidthRequest;
			btn.Clicked += OnBtnAddPhoneClicked;

			var addPhoneRow = new HBox();
			addPhoneRow.PackStart(labelAddPhone, true, true, 0);
			addPhoneRow.PackEnd(btn, false, false, 0);
			PhonesTable.Attach(addPhoneRow, 0, 1, rowsCount - 1, rowsCount);

			PhonesTable.ShowAll();
			btn.Sensitive = buttonSaveComment.Sensitive = _counterpartyPermissionResult.CanUpdate && _counterparty.Id != 0;

			var isLogistcsRequirementsEditable = _counterpartyPermissionResult.CanUpdate && _counterparty.Id != 0;
			logisticsRequirementsView.Sensitive = isLogistcsRequirementsEditable;
			buttonSaveLogisticsRequirements.Sensitive = isLogistcsRequirementsEditable;

			if(InfoProvider is OrderDlg)
			{
				yvboxLogisticsRequirements.Visible = true;
				SetLogisticsRequirementsCheckboxes();
			}
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
				ServicesConfig.UnitOfWorkFactory.CreateForRoot<Counterparty>(_counterparty.Id, "Кнопка «Cохранить комментарий» на панели контрагента"))
			{
				uow.Root.Comment = textviewComment.Buffer.Text;
				uow.Save();
			}
			_textviewcommentBufferChanged = false;
		}

		protected void OnButtonSaveCommentClicked(object sender, EventArgs e)
		{
			_logger.LogInformation("Нажата кнопка сохранить комментарий в {ButtonClickedDateTime}",
				DateTime.Now.ToString("g"));

			SaveComment();
		}

		protected void OnButtonSaveLogisticsRequirementsClicked(object sender, EventArgs e)
		{
			SaveLogisticsRequirements();
		}

		private void OnTextviewCommentBufferChanged(object sender, EventArgs e)
		{
			_textviewcommentBufferChanged = true;
		}

		private void OnTextviewCommentFocusOut(object sender, EventArgs e)
		{
			if(_textviewcommentBufferChanged && buttonSaveComment.State != StateType.Prelight)
			{
				Gtk.Application.Invoke((s, ea) =>
				{
					if(_counterparty.Id == 0)
					{
						_counterparty.Comment = textviewComment.Buffer.Text;
						_textviewcommentBufferChanged = false;
						return;
					}
					
					var isRequiredToSaveComment = MessageDialogHelper.RunQuestionDialog("Сохранить изменения в комментарии?");
					if(isRequiredToSaveComment)
					{
						SaveComment();
					}
					else
					{
						textviewComment.Buffer.Text = _counterparty.Comment ?? string.Empty;
						_textviewcommentBufferChanged = false;
					}
				});
			}
		}

		protected void OnBtnAddPhoneClicked(object sender, EventArgs e)
		{
			TDIMain.MainNotebook.OpenTab(
				DialogHelper.GenerateDialogHashName<Counterparty>(_counterparty.Id),
				() =>
				{
					var dlg = new CounterpartyDlg(EntityUoWBuilder.ForOpen(_counterparty.Id), ServicesConfig.UnitOfWorkFactory);
					dlg.ActivateContactsTab();
					dlg.EntitySaved += (o, args) => Refresh(args.Entity);
					return dlg;
				}
			);
		}

		#endregion

		#region overrided Dispose() method

		private bool _disposed = false;

		public override void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if(_disposed)
			{
				return;
			}

			if(disposing)
			{
				textviewComment.Buffer.Changed -= OnTextviewCommentBufferChanged;
				textviewComment.FocusOutEvent -= OnTextviewCommentFocusOut;
				base.Dispose();
			}

			_disposed = true;
		}
		#endregion
	}
}
