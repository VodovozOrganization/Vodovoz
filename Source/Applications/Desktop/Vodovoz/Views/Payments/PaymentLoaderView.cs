using Gtk;
using Vodovoz.ViewModels;
using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.Domain.Payments;
using QS.Utilities;
using System.Linq;
using QS.Navigation;
using System.Threading.Tasks;
using System;
using NLog;
using Vodovoz.ViewModels.ViewModels.Payments;
using Vodovoz.Infrastructure;

namespace Vodovoz.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentLoaderView : TabViewBase<PaymentLoaderViewModel>
	{
		private readonly Logger _logger = LogManager.GetCurrentClassLogger();
		static Gdk.Color colorPink = GdkColors.Pink;
		static Gdk.Color colorWhite = GdkColors.PrimaryBase;
		static Gdk.Color colorLightGreen = GdkColors.SuccessBase;

		public PaymentLoaderView(PaymentLoaderViewModel paymentLoaderViewModel) : base(paymentLoaderViewModel)
		{
			Build();
			Configure();
		}
		

		private void Configure()
		{
			var txtFilter = new FileFilter();
			txtFilter.AddPattern("*.txt");
			txtFilter.Name = "Текстовые файлы (*.txt)";
			var allFilter = new FileFilter();
			allFilter.AddPattern("*");
			allFilter.Name = "Все файлы";
			fileChooserBtn.AddFilter(txtFilter);
			fileChooserBtn.AddFilter(allFilter);

			btnUpload.Clicked += (sender, e) => SaveAsync();
			btnUpload.Binding.AddBinding(ViewModel, v => v.CanSave, w => w.Sensitive).InitializeFromSource();
			btnCancel.Clicked += (sender, e) => ViewModel.Close(false, CloseSource.Cancel);
			btnCancel.Binding.AddBinding(ViewModel, v => v.CanCancel, w => w.Sensitive).InitializeFromSource();

			ViewModel.UpdateProgress += UpdateProgress;

			btnReadFile.Clicked += (sender, e) => ViewModel.ParseCommand.Execute(fileChooserBtn.Filename);
			btnReadFile.Binding.AddBinding(ViewModel, vm => vm.CanReadFile, v => v.Sensitive).InitializeFromSource();

			ConfigureTree();
		}

		private void ConfigureTree()
		{
			treeDocuments.ColumnsConfig = FluentColumnsConfig<Payment>.Create()
				.AddColumn("№")
					.AddTextRenderer(x => x.PaymentNum.ToString())
				.AddColumn("Дата")
					.AddTextRenderer(x => x.Date.ToShortDateString())
				.AddColumn("Cумма")
					.AddTextRenderer(x => x.Total.ToString())
				.AddColumn("Заказы")
					.AddTextRenderer(x => x.NumOrders)
				.AddColumn("Плательщик")
					.AddTextRenderer(x => x.CounterpartyName)
					.WrapWidth(450)
					.WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Получатель")
					.AddTextRenderer(x => x.Organization.FullName)
				.AddColumn("Назначение платежа")
					.AddTextRenderer(x => x.PaymentPurpose)
					.WrapWidth(600)
					.WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Категория дохода/расхода")
					.AddComboRenderer(x => x.ProfitCategory)
					.SetDisplayFunc(x => x.Name)
					.FillItems(ViewModel.ProfitCategories)
					.Editing()
				.AddColumn("")
				.RowCells().AddSetter<CellRenderer>(
					(c, n) =>
					{
						var color = colorLightGreen;
						
						if(n.Status != PaymentState.distributed)
						{
							color = colorPink;
						}

						c.CellBackgroundGdk = color;
					})
				.Finish();

			treeDocuments.Binding.AddBinding(ViewModel, vm => vm.ObservablePayments, w => w.ItemsDataSource).InitializeFromSource();
		}

		private void UpdateProgress(string msg, double progress)
		{
			if(progress == 0)
			{
				progressbar1.Fraction = 0;
			}
			else if(progress == 1)
			{
				progressbar1.Fraction = 1;
			}
			else
			{
				progressbar1.Fraction += progress;
			}

			progressbar1.Text = msg;
			GtkHelper.WaitRedraw();
		}

		private async void SaveAsync()
		{
			try
			{
				if(!ViewModel.ObservablePayments.Any())
				{
					return;
				}

				ViewModel.IsSavingState = true;
				UpdateProgress("Начинаем сохранение...", 0);

				var count = 0;
				var totalPayments = ViewModel.ObservablePayments.Count();
				var progress = 1d / totalPayments;

				foreach(Payment payment in ViewModel.ObservablePayments)
				{
					if(payment.Status == PaymentState.distributed)
					{
						foreach(var paymentItem in payment.PaymentItems)
						{
							var result = await Task.Run(() => ViewModel.SaveAllocatedOrderAsync(paymentItem));

							if(!result)
							{
								Gtk.Application.Invoke((s, e) => ShowMessageAndClose(
									QS.Dialog.ImportanceLevel.Warning,
									CloseSource.Self,
									"Ошибка при сохранении заказа.",
									0,
									$"При сохранении заказа { paymentItem.Order.Id} произошла ошибка. Вкладка будет закрыта.\nДля продолжения работы снова откройте вкладку"));
								return;
							}
						}
						ViewModel.CreateOperations(payment);
					}

					ViewModel.UoW.Save(payment);
					ViewModel.UoW.Commit();
					count++;
					UpdateProgress($"Сохранен {count} платеж из {totalPayments}", progress);
				}

				Gtk.Application.Invoke((s, e) => ShowMessageAndClose(
					QS.Dialog.ImportanceLevel.Info,
					CloseSource.Save,
					"Сохранение закончено...",
					1,
					"Сохранение прошло успешно")
				);
			}
			catch(Exception ex)
			{
				_logger.Error(ex);
				Gtk.Application.Invoke((s, e) => ShowMessageAndClose(
					QS.Dialog.ImportanceLevel.Warning,
					CloseSource.Self,
					"Ошибка при сохранении.",
					0,
					$"При сохранении произошла ошибка. Вкладка будет закрыта.\nДля продолжения работы снова откройте вкладку"));
			}
		}

		private void ShowMessageAndClose(QS.Dialog.ImportanceLevel importanceLevel, CloseSource closeSource, string progressMessage, double progress, string message)
		{
			UpdateProgress(progressMessage, progress);
			ViewModel.InteractiveService.ShowMessage(importanceLevel, message);
			ViewModel.Close(false, closeSource);
		}

		public override void Destroy()
		{
			ViewModel.UpdateProgress -= UpdateProgress;
			base.Destroy();
		}
	}
}
