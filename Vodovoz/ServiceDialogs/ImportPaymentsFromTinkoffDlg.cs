using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using NHibernate.Util;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Payments;
using Vodovoz.Repositories.Payments;

namespace Vodovoz.ServiceDialogs
{
	public partial class ImportPaymentsFromTinkoffDlg : TdiTabBase
	{
		PaymentsFromTinkoffParser parser;
		GenericObservableList<PaymentFromTinkoff> paymentsFromTinkoff;
		List<String> errorList = new List<string>();
		Dictionary<int, decimal> otherPaymentsFromDB;

		string colorWhite = "white";
		string colorLightRed = "light coral";
		string colorYellow = "yellow";

		public ImportPaymentsFromTinkoffDlg()
		{
			this.Build();
			TabName = "Загрузка оплат из ЛК Тинькофф";
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			var csvFilter = new FileFilter();
			csvFilter.AddPattern("*.csv");
			csvFilter.Name = "Comma Separated Values File (*.csv)";
			var allFilter = new FileFilter();
			allFilter.AddPattern("*");
			allFilter.Name = "Все файлы";
			fChooser.AddFilter(csvFilter);
			fChooser.AddFilter(allFilter);

			treeDocuments.ColumnsConfig = ColumnsConfigFactory.Create<PaymentFromTinkoff>()
				.AddColumn("Загрузить")
					.AddToggleRenderer(x => x.Selected).Editing()
					.AddSetter((c, n) => (c as CellRendererToggle).Activatable = n.Selectable)
				.AddColumn("Дата и\nвремя")
					.AddTextRenderer(
						x => String.Format(
							"{0}\n{1}",
							x.DateAndTime.ToString("M"),
							x.DateAndTime.ToString("t")
						)
					)
				.AddColumn("Номер и\nсумма оплаты")
					.AddTextRenderer(
						x => String.Format(
							"{0}\n{1}",
							x.PaymentNr.ToString(),
							CurrencyWorks.GetShortCurrencyString(x.PaymentRUR)
						)
					)
				.AddColumn("Контакты")
					.AddTextRenderer(
						x => String.Format(
							"{0}\n{1}",
							x.Phone,
							x.Email
						)
					)
				.AddColumn("Магазин")
					.AddTextRenderer(x => x.Shop)
				.AddColumn("Статус оплаты")
				.AddTextRenderer(x => x.PaymentStatus.GetEnumTitle())
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRenderer>(
						(c, n) => c.CellBackground = n.Color
					)
				.Finish();

			SetControlsAccessibility();
		}

		void InitializeListOfPayments()
		{
			otherPaymentsFromDB?.Clear();
			foreach(PaymentFromTinkoff payment in paymentsFromTinkoff) {
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

		/// <summary>
		/// Проверка, что в БД уже нет такого платежа
		/// </summary>
		/// <returns><c>true</c>, если есть такой номер платёжа в БД,
		/// <c>false</c> если номер платежа не найден в БД</returns>
		/// <param name="payment">Платёж</param>
		bool IsPaymentUploadedAlready(PaymentFromTinkoff payment)
		{
			if(otherPaymentsFromDB == null || !otherPaymentsFromDB.Any())
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
					otherPaymentsFromDB = PaymentsRepository.GetAllPaymentsFromTinkoff(uow);
				}

			return otherPaymentsFromDB.Any() && otherPaymentsFromDB.ContainsKey(payment.PaymentNr);
		}

		void SetControlsAccessibility(bool enabled = false)
		{
			btnUpload.Sensitive = btnReadFile.Sensitive = enabled;
			chkAll.Sensitive = enabled;
		}

		protected void OnButtonReadFileClicked(object sender, EventArgs e)
		{
			parser = new PaymentsFromTinkoffParser(fChooser.Filename);
			parser.Parse();
			paymentsFromTinkoff = new GenericObservableList<PaymentFromTinkoff>(parser.PaymentsFromTinkoff);
			InitializeListOfPayments();
			btnUpload.Sensitive = chkAll.Active = chkAll.Sensitive =
				treeDocuments.Sensitive = paymentsFromTinkoff.Any(p => p.Selectable);
			UpdateDescription();
			treeDocuments.ItemsDataSource = paymentsFromTinkoff;
		}

		void UpdateDescription()
		{
			if(paymentsFromTinkoff != null) {
				StringBuilder sb = new StringBuilder();
				if(paymentsFromTinkoff.Any())
					sb.Append(
						String.Format(
							"Отмечено для загрузки <b>{0}</b> платежей из <b>{1}</b>, ",
							paymentsFromTinkoff.Count(p => p.Selected),
							paymentsFromTinkoff.Count()
						)
					);
				if(paymentsFromTinkoff.Any(p => p.IsDuplicate))
					sb.Append(String.Format("<span background=\"{0}\">     </span> - был загружен ранее, ", colorYellow));
				if(paymentsFromTinkoff.Any(p => p.PaymentStatus != PaymentStatus.CONFIRMED))
					sb.Append(String.Format("<span background=\"{0}\">     </span> - статус неприемлем, ", colorLightRed));

				lblDescription.Markup = sb.ToString().Trim(new[] { ' ', ',' });
			}
		}

		protected void OnFilechooserSelectionChanged(object sender, EventArgs e)
		{
			btnReadFile.Sensitive = !String.IsNullOrWhiteSpace(fChooser.Filename);
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
			var paymentsToSave = paymentsFromTinkoff.Where(p => p.Selected);
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
						String.Format(
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
			errorList.Add(
				String.Format(
					"{0}: {1}\n",
					DateTime.Now.ToString("G"),
					msg
				)
			);
		}

		void UpdateProgress(int saved, int processed, int total, string text)
		{
			UpdateProgress(
				processed * 1d / total,
				String.Format(
					"{0} (Сохранено {1} из {2})",
					text,
					saved,
					total
				)
			);
		}

		void UpdateProgress(double progress, string text)
		{
			if(progress > 1)
				progress = 1;
			progressBar.Fraction = progress;
			progressBar.Text = String.Format("{1}% - {0} ", text, (int)(progress * 100));
			progressBar.Fraction = progress > 1 ? 1d : progress;
			QSMain.WaitRedraw();
		}

		protected void OnChkAllToggled(object sender, EventArgs e)
		{
			if(chkAll.HasFocus) {
				foreach(PaymentFromTinkoff item in paymentsFromTinkoff)
					item.Selected = item.Selectable && chkAll.Active;
				treeDocuments.YTreeModel.EmitModelChanged();
				UpdateDescription();
			}
		}

		protected void OnBtnUploadClicked(object sender, EventArgs e)
		{
			btnUpload.Sensitive = treeDocuments.Sensitive = chkAll.Sensitive = chkAll.Active = false;
			Save(paymentsFromTinkoff.Count(p => p.Selected) < 200 ? 1 : 100);
			UpdateDescription();

			if(errorList.Any()) {
				string fileName = String.Format("PaymentFromTinkoffErrors_{0}.log", DateTime.Now.ToString("yyyyMMddHHmmss"));
				File.WriteAllLines(
					fileName,
					errorList
				);
				MessageDialogHelper.RunWarningDialog(
					String.Format(
						"Некоторые платежи не были добавлены. Возможно, они уже есть в нашей базе. Дополнительная информация в файле {0}",
						fileName
					)
				);
				errorList.Clear();
			}
			paymentsFromTinkoff.Clear();
			btnReadFile.Sensitive = true;
		}

		protected void OnBtnCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}
	}
}
