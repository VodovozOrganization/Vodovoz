using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Widgets;
using QSProjectsLib;
using Vodovoz.Domain.Payments;
using Vodovoz.Repositories.Payments;

namespace Vodovoz.ServiceDialogs
{
	//FIXME Переименовать диалог при переписывании на MVVM.
	public partial class ImportPaymentsFromTinkoffDlg : TdiTabBase
	{
		PaymentsFromTinkoffParser tinkoffParser;
		PaymentsFromYookassaParser yookassaParser;
		private MenuItem readTinkoff;
		private MenuItem readYookassa;
        private Widget readFileButton;

        GenericObservableList<PaymentByCardOnline> paymentsByCard;
		List<string> errorList = new List<string>();
		Dictionary<int, decimal> otherPaymentsFromDB;

		string colorWhite = "white";
		string colorLightRed = "light coral";
		string colorYellow = "yellow";

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
					.AddSetter<CellRenderer>((c, n) => c.CellBackground = n.Color)
				.Finish();

			ConfigureButtonReadFile();

			SetControlsAccessibility();
		}

		private void ConfigureButtonReadFile()
		{
			MenuButton menuButton = new MenuButton {Label = "Прочитать данные из файла"};
			Menu childActionButtons = new Menu();
			
			readTinkoff = new MenuItem("Прочитать выгрузку Тинькова");
			readTinkoff.Activated += (sender, args) => ReadTinkoffPayments();
			readTinkoff.Sensitive = !string.IsNullOrEmpty(fChooser.Filename);
			
			readYookassa = new MenuItem("Прочитать выгрузку Юкассы");
			readYookassa.Activated += (sender, args) => ReadYookassaPayments();
			readYookassa.Sensitive = !string.IsNullOrEmpty(fChooser.Filename);

			childActionButtons.Add(readTinkoff);
			childActionButtons.Add(readYookassa);

			childActionButtons.ShowAll();
			menuButton.Menu = childActionButtons;
			readFileButton = menuButton;
            readFileButton.ShowAll();

			hboxButtons.Add(readFileButton);
			Box.BoxChild readFileButtonBox = (Box.BoxChild)hboxButtons[readFileButton];
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
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
					otherPaymentsFromDB = PaymentsRepository.GetPaymentsByOneMonth(uow, DateTime.Now);
				}

			return otherPaymentsFromDB.Any() && otherPaymentsFromDB.ContainsKey(payment.PaymentNr);
		}

		void SetControlsAccessibility(bool enabled = false)
		{
			btnUpload.Sensitive = readFileButton.Sensitive = enabled;
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
			
			tinkoffParser = new PaymentsFromTinkoffParser(fChooser.Filename);
			tinkoffParser.Parse();
			paymentsByCard = new GenericObservableList<PaymentByCardOnline>(tinkoffParser.PaymentsFromTinkoff);
			ShowParsedPayments();
		}

		void ReadYookassaPayments()
		{
			var fileNameExtension = fChooser.Filename.Split(new[] {'.'}).Last();

			if (fileNameExtension == "csv")
			{
				MessageDialogHelper.RunErrorDialog($"Неверное расширение файла! Для выгрузки с Юкассы нужен {fChooser.Filters[1].Name}.");
				return;
			}

			yookassaParser = new PaymentsFromYookassaParser(fChooser.Filename);
			yookassaParser.Parse();
			paymentsByCard = new GenericObservableList<PaymentByCardOnline>(yookassaParser.PaymentsFromYookassa);
			ShowParsedPayments();
		}
		
		private void ShowParsedPayments()
		{
			InitializeListOfPayments();
			btnUpload.Sensitive = chkAll.Active = chkAll.Sensitive =
				treeDocuments.Sensitive = paymentsByCard.Any(p => p.Selectable);
			UpdateDescription();
			treeDocuments.ItemsDataSource = paymentsByCard;
		}
		
		void InitializeListOfPayments()
		{
			otherPaymentsFromDB?.Clear();
			foreach(PaymentByCardOnline payment in paymentsByCard) {
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
			if(paymentsByCard != null) {
				StringBuilder sb = new StringBuilder();
				if(paymentsByCard.Any())
					sb.Append(
						string.Format(
							"Отмечено для загрузки <b>{0}</b> платежей из <b>{1}</b>, ",
							paymentsByCard.Count(p => p.Selected),
							paymentsByCard.Count()
						)
					);
				if(paymentsByCard.Any(p => p.IsDuplicate))
					sb.Append($"<span background=\"{colorYellow}\">     </span> - был загружен ранее, ");
				if(paymentsByCard.Any(p => p.PaymentStatus != PaymentStatus.CONFIRMED))
					sb.Append($"<span background=\"{colorLightRed}\">     </span> - статус неприемлем, ");

				lblDescription.Markup = sb.ToString().Trim(new[] { ' ', ',' });
			}
		}

		protected void OnFilechooserSelectionChanged(object sender, EventArgs e)
		{
            readFileButton.Sensitive = !string.IsNullOrWhiteSpace(fChooser.Filename);
			readTinkoff.Sensitive = !string.IsNullOrEmpty(fChooser.Filename);
			readYookassa.Sensitive = !string.IsNullOrEmpty(fChooser.Filename);

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
			var paymentsToSave = paymentsByCard.Where(p => p.Selected);
			int qtyPaymentsToSave = paymentsToSave.Count();

			IUnitOfWork uow = null;
			foreach(var selectedPayment in paymentsToSave) {
				try {
					if(cnt == 0) {
						uow = UnitOfWorkFactory.CreateWithoutRoot();
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
				foreach(PaymentByCardOnline item in paymentsByCard)
					item.Selected = item.Selectable && chkAll.Active;
				treeDocuments.YTreeModel.EmitModelChanged();
				UpdateDescription();
			}
		}

		protected void OnBtnUploadClicked(object sender, EventArgs e)
		{
			btnUpload.Sensitive = treeDocuments.Sensitive = chkAll.Sensitive = chkAll.Active = false;
			Save(paymentsByCard.Count(p => p.Selected) < 200 ? 1 : 100);
			UpdateDescription();

			if(errorList.Any()) {
				string caption = "Некоторые платежи не были добавлены. Возможно, они уже есть в нашей базе\n\n";
				string message = caption + string.Join("\n", errorList);
				LargeMessageDialog messageDialog = new LargeMessageDialog("Ошибка", message);
				messageDialog.Show();
				errorList.Clear();
			}
			paymentsByCard.Clear();
            readFileButton.Sensitive = true;
		}

		protected void OnBtnCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}
	}
}
