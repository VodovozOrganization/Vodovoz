using Autofac;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Widgets;
using QSProjectsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Application.Payments.OnlinePayments;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Infrastructure;
using VodovozBusiness.Domain.Payments;

namespace Vodovoz.ServiceDialogs
{
	//FIXME Переименовать диалог при переписывании на MVVM.
	public partial class ImportPaymentsFromTinkoffDlg : TdiTabBase
	{
		private readonly IPaymentsRepository _paymentsRepository = ScopeProvider.Scope.Resolve<IPaymentsRepository>();
		private MenuItem _readTinkoff;
		private MenuItem _readYookassa;
		private MenuItem _readCloudPayments;
		private Widget _readFileButton;
		private readonly IInteractiveService _interactiveService = ServicesConfig.InteractiveService;

		IObservableList<PaymentByCardOnline> _paymentsByCard;
		List<string> errorList = new List<string>();
		IList<PaymentByCardOnlineNode> otherPaymentsFromDB;

		string colorWhite = "white";
		string colorLightRed = "light coral";
		string colorYellow = "yellow";

		private const string _ioErrorMessage = "Файл уже открыт в другой программе.Сначала закройте файл.";

		public ImportPaymentsFromTinkoffDlg()
		{
			this.Build();
			TabName = "Загрузка платежей по карте";
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			var csvFilter = new FileFilter();
			csvFilter.AddPattern("*.csv");
			csvFilter.Name = "Comma Separated Values File (*.csv)";
			
			var txtFilter = new FileFilter();
			txtFilter.AddPattern("*.txt");
			txtFilter.Name = "Текстовый файл (*.txt)";
			
			var allFilter = new FileFilter();
			allFilter.AddPattern("*");
			allFilter.Name = "Все файлы";
			
			fChooser.AddFilter(csvFilter);
			fChooser.AddFilter(txtFilter);
			fChooser.AddFilter(allFilter);

			treeDocuments.ColumnsConfig = ColumnsConfigFactory.Create<PaymentByCardOnline>()
				.AddColumn("Загрузить")
					.AddToggleRenderer(x => x.Selected).Editing()
					.AddSetter((c, n) => (c as CellRendererToggle).Activatable = n.Selectable)
				.AddColumn("Дата и\nвремя")
					.AddTextRenderer(x => $"{x.DateAndTime:M}\n{x.DateAndTime:t}")
				.AddColumn("Номер и\nсумма оплаты")
					.AddTextRenderer(
						x => $"{x.PaymentNr.ToString()}\n{CurrencyWorks.GetShortCurrencyString(x.PaymentRUR)}"
				)
				.AddColumn("Контакты")
					.AddTextRenderer(x => $"{x.Phone}\n{x.Email}")
				.AddColumn("Магазин")
					.AddTextRenderer(x => x.Shop)
				.AddColumn("Статус оплаты")
				.AddTextRenderer(x => x.PaymentStatus.GetEnumTitle())
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRenderer>((c, n) =>
					{
						var color = GdkColors.PrimaryBase;

						if(n.Color == colorLightRed)
						{
							color = GdkColors.LightCoral;
						}

						if(n.Color == colorYellow)
						{
							color = GdkColors.WarningBase;
						}

						c.CellBackgroundGdk = color;
					})
				.Finish();

			ConfigureButtonReadFile();

			SetControlsAccessibility();
		}

		private void ConfigureButtonReadFile()
		{
			var menuButton = new MenuButton {Label = "Прочитать данные из файла"};
			var childActionButtons = new Menu();
			
			_readTinkoff = new MenuItem("Прочитать выгрузку Тинькова");
			_readTinkoff.Activated += (sender, args) => ReadTinkoffPayments();
			_readTinkoff.Sensitive = !string.IsNullOrEmpty(fChooser.Filename);
			
			_readYookassa = new MenuItem("Прочитать выгрузку Юкассы");
			_readYookassa.Activated += (sender, args) => ReadYookassaPayments();
			_readYookassa.Sensitive = !string.IsNullOrEmpty(fChooser.Filename);

			_readCloudPayments = new MenuItem("Прочитать выгрузку CloudPayments");
			_readCloudPayments.Activated += (sender, args) => ReadCloudPayments();
			_readCloudPayments.Sensitive = !string.IsNullOrEmpty(fChooser.Filename);

			childActionButtons.Add(_readTinkoff);
			childActionButtons.Add(_readYookassa);
			childActionButtons.Add(_readCloudPayments);

			childActionButtons.ShowAll();
			menuButton.Menu = childActionButtons;
			_readFileButton = menuButton;
            _readFileButton.ShowAll();

			hboxButtons.Add(_readFileButton);
			Box.BoxChild readFileButtonBox = (Box.BoxChild)hboxButtons[_readFileButton];
			readFileButtonBox.Position = 0;
			readFileButtonBox.Expand = false;
			readFileButtonBox.Fill = false;
		}

		/// <summary>
		/// Проверка, что в БД уже нет такого платежа
		/// </summary>
		/// <returns><c>true</c>, если есть такой номер платёжа в БД,
		/// <c>false</c> если номер платежа не найден в БД</returns>
		/// <param name="payment">Платёж</param>
		bool IsPaymentUploadedAlready(PaymentByCardOnline payment)
		{
			if(otherPaymentsFromDB == null || !otherPaymentsFromDB.Any())
			{
				using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
				{
					otherPaymentsFromDB = _paymentsRepository.GetPaymentsByTwoMonths(uow, payment.DateAndTime);
				}
			}

			return otherPaymentsFromDB.Any(
				x =>
					x.Number == payment.PaymentNr &&
				    x.Sum == payment.PaymentRUR &&
				    x.Date == payment.DateAndTime);
		}

		void SetControlsAccessibility(bool enabled = false)
		{
			btnUpload.Sensitive = _readFileButton.Sensitive = enabled;
			chkAll.Sensitive = enabled;
		}

		void ReadTinkoffPayments()
		{
			var fileNameExtension = fChooser.Filename.Split(new[] {'.'}).Last();

			if (fileNameExtension == "txt")
			{
				MessageDialogHelper.RunErrorDialog($"Неверное расширение файла! Для выгрузки с Тинькова нужен {fChooser.Filters[0].Name}.");
				return;
			}

			ParsePayments(new PaymentsFromTinkoffParser(), fChooser.Filename);
		}

		void ReadYookassaPayments()
		{
			var fileNameExtension = fChooser.Filename.Split(new[] {'.'}).Last();

			if (fileNameExtension != "txt" && fileNameExtension != "csv")
			{
				MessageDialogHelper.RunErrorDialog($"Неверное расширение файла! Для выгрузки с Юкассы нужен {fChooser.Filters[1].Name} или {fChooser.Filters[0].Name}.");
				return;
			}

			ParsePayments(new PaymentsFromYookassaParser(), fChooser.Filename);
		}

		void ReadCloudPayments()
		{
			var fileNameExtension = fChooser.Filename.Split(new[] { '.' }).Last();

			if(fileNameExtension != "txt" && fileNameExtension != "csv")
			{
				MessageDialogHelper.RunErrorDialog($"Неверное расширение файла! Для выгрузки с CloudPayments нужен {fChooser.Filters[1].Name} или {fChooser.Filters[0].Name}.");
				return;
			}

			ParsePayments(new PaymentsFromCloudPaymentsParser(), fChooser.Filename);
		}
		
		private void ParsePayments(IPaymentByCardOnlineParser parser, string filename)
		{
			try
			{
				parser.Parse(filename);
			}
			catch(Exception e)
			{
				ShowError(e);
				return;
			}

			_paymentsByCard = new ObservableList<PaymentByCardOnline>(parser.ParsedPayments);
			ShowParsedPayments();
		}

		private void ShowError(Exception e)
		{
			var message = new StringBuilder();

			if(e is IOException)
			{
				message.AppendLine(_ioErrorMessage);
			}

			message.AppendLine(e.Message);

			_interactiveService.ShowMessage(ImportanceLevel.Error, message.ToString());
		}

		private void ShowParsedPayments()
		{
			InitializeListOfPayments();
			btnUpload.Sensitive = chkAll.Active = chkAll.Sensitive =
				treeDocuments.Sensitive = _paymentsByCard.Any(p => p.Selectable);
			UpdateDescription();
			treeDocuments.ItemsDataSource = _paymentsByCard;
		}
		
		void InitializeListOfPayments()
		{
			otherPaymentsFromDB?.Clear();
			foreach(PaymentByCardOnline payment in _paymentsByCard) {
				if(payment.PaymentStatus != PaymentStatus.CONFIRMED) {
					payment.Color = colorLightRed;
					payment.Selected = payment.Selectable = false;
				} else if(payment.PaymentStatus == PaymentStatus.CONFIRMED && IsPaymentUploadedAlready(payment)) {
					payment.Color = colorYellow;
					payment.Selected = payment.Selectable = false;
					payment.IsDuplicate = true;
				} else if(payment.PaymentStatus == PaymentStatus.CONFIRMED && !IsPaymentUploadedAlready(payment)) {
					payment.Color = colorWhite;
					payment.Selected = payment.Selectable = true;
					payment.IsDuplicate = false;
				}

				payment.PropertyChanged += (s, ea) => {
					if(ea.PropertyName == payment.GetPropertyName(p => p.Selected))
						UpdateDescription();
				};
			}
		}
		
		void UpdateDescription()
		{
			if(_paymentsByCard != null) {
				StringBuilder sb = new StringBuilder();
				if(_paymentsByCard.Any())
					sb.Append(
						string.Format(
							"Отмечено для загрузки <b>{0}</b> платежей из <b>{1}</b>, ",
							_paymentsByCard.Count(p => p.Selected),
							_paymentsByCard.Count()
						)
					);
				if(_paymentsByCard.Any(p => p.IsDuplicate))
					sb.Append($"<span background=\"{colorYellow}\">     </span> - был загружен ранее, ");
				if(_paymentsByCard.Any(p => p.PaymentStatus != PaymentStatus.CONFIRMED))
					sb.Append($"<span background=\"{colorLightRed}\">     </span> - статус неприемлем, ");

				lblDescription.Markup = sb.ToString().Trim(new[] { ' ', ',' });
			}
		}

		protected void OnFilechooserSelectionChanged(object sender, EventArgs e)
		{
            _readFileButton.Sensitive = !string.IsNullOrWhiteSpace(fChooser.Filename);
			_readTinkoff.Sensitive = !string.IsNullOrEmpty(fChooser.Filename);
			_readYookassa.Sensitive = !string.IsNullOrEmpty(fChooser.Filename);
			_readCloudPayments.Sensitive = !string.IsNullOrEmpty(fChooser.Filename);

			btnUpload.Sensitive = false;
			UpdateDescription();
		}

		/// <summary>
		/// Пакетное сохранение платежей в БД.
		/// Если из пакета один платёж не сохранится, то весь пакет не будет сохранён.
		/// </summary>
		/// <param name="batchSize">Размер пакета</param>
		void Save(int batchSize)
		{
			int cnt = 0;
			int totalSaved = 0;
			int totalProcessed = 0;
			var paymentsToSave = _paymentsByCard.Where(p => p.Selected);
			int qtyPaymentsToSave = paymentsToSave.Count();

			IUnitOfWork uow = null;
			foreach(var selectedPayment in paymentsToSave) {
				try {
					if(cnt == 0) {
						uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
						uow.Session.SetBatchSize(batchSize);
					}
					uow.Save(selectedPayment);
					cnt++;
					if(cnt >= batchSize) {
						uow.Commit();
						uow.Dispose();
						totalSaved += cnt;
						cnt = 0;
					}
				} catch(Exception exp) {
					uow.Dispose();
					cnt = 0;
					AddErrorMessage(
						string.Format(
							"Ошибка при сохранении платёжа №{0}\n{1}\n{2}",
							selectedPayment.PaymentNr,
							exp.Message,
							exp.InnerException?.Message
						)
					);
				}
				UpdateProgress(totalSaved, ++totalProcessed, qtyPaymentsToSave, "Сохранение выбранных платежей...");
			}
			if(uow != null) {
				uow.Commit();
				uow.Dispose();
				UpdateProgress(totalSaved + cnt, totalProcessed, qtyPaymentsToSave, "Сохранение выбранных платежей...");
			}
		}

		void AddErrorMessage(string msg)
		{
			errorList.Add($"{DateTime.Now:G}: {msg}\n");
		}

		void UpdateProgress(int saved, int processed, int total, string text)
		{
			UpdateProgress(processed * 1d / total, $"{text} (Сохранено {saved} из {total})");
		}

		void UpdateProgress(double progress, string text)
		{
			if(progress > 1)
				progress = 1;
			progressBar.Fraction = progress;
			progressBar.Text = $"{(int) (progress * 100)}% - {text} ";
			progressBar.Fraction = progress > 1 ? 1d : progress;
			QSMain.WaitRedraw();
		}

		protected void OnChkAllToggled(object sender, EventArgs e)
		{
			if(chkAll.HasFocus) {
				foreach(PaymentByCardOnline item in _paymentsByCard)
					item.Selected = item.Selectable && chkAll.Active;
				treeDocuments.YTreeModel.EmitModelChanged();
				UpdateDescription();
			}
		}

		protected void OnBtnUploadClicked(object sender, EventArgs e)
		{
			btnUpload.Sensitive = treeDocuments.Sensitive = chkAll.Sensitive = chkAll.Active = false;
			Save(_paymentsByCard.Count(p => p.Selected) < 200 ? 1 : 100);
			UpdateDescription();

			if(errorList.Any()) {
				string caption = "Некоторые платежи не были добавлены. Возможно, они уже есть в нашей базе\n\n";
				string message = caption + string.Join("\n", errorList);
				LargeMessageDialog messageDialog = new LargeMessageDialog("Ошибка", message);
				messageDialog.Show();
				errorList.Clear();
			}
			_paymentsByCard.Clear();
            _readFileButton.Sensitive = true;
		}

		protected void OnBtnCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}
	}
}
