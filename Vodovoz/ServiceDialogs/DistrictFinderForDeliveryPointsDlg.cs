using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Repositories.Sale;

namespace Vodovoz.ServiceDialogs
{
	public partial class DistrictFinderForDeliveryPointsDlg : QS.Dialog.Gtk.TdiTabBase
	{
		private IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot();

		List<DeliveryPoint> DeliveryPointsList = new List<DeliveryPoint>();
		List<string> errors = new List<string>();

		public DistrictFinderForDeliveryPointsDlg()
		{
			if(!QSMain.User.Permissions["database_maintenance"]) {
				MessageDialogHelper.RunWarningDialog("Доступ запрещён!", "У вас недостаточно прав для доступа к этой вкладке. Обратитесь к своему руководителю.", Gtk.ButtonsType.Ok);
				FailInitialize = true;
				return;
			}

			//throw new ApplicationException("Сюда сейчас нельзя.");

			this.Build();

			TabName = "Сопоставление районов доставки";
		}

		#region Label свойства

		int totalDP;
		public int TotalDP {
			get { return totalDP; }
			set {
				totalDP = value;
				labelDPTotalValue.LabelProp = totalDP.ToString();
				QSMain.WaitRedraw();
			}
		}

		int skipedDP;
		public int SkipedDP {
			get { return skipedDP; }
			set {
				skipedDP = value;
				labelDPFailsValue.LabelProp = skipedDP.ToString();
				QSMain.WaitRedraw();
			}
		}

		int errorsDP;
		public int ErrorsDP {
			get { return errorsDP; }
			set {
				errorsDP = value;
				labelDPErrorsValue.LabelProp = errorsDP.ToString();
				QSMain.WaitRedraw();
			}
		}

		int successDP;
		public int SuccessDP {
			get { return successDP; }
			set {
				successDP = value;
				labelDPSuccessValue.LabelProp = successDP.ToString();
				QSMain.WaitRedraw();
			}
		}

		#endregion

		public static string GenerateHashName() => typeof(DistrictFinderForDeliveryPointsDlg).Name;

		void Load()
		{
			Clear();

			progressbar.Text = string.Format("Загрузка контрагентов и счетов");

			var deliveryPointsQuery = UoW.Session.QueryOver<DeliveryPoint>()
												 .Where(d => d.District == null)
												 .Future<DeliveryPoint>();

			var districtSource = ScheduleRestrictionRepository.AreaWithGeometry(UoW);

			DeliveryPointsList = deliveryPointsQuery.ToList();

			TotalDP = DeliveryPointsList.Count;
			var progressCounter = 0;
			progressbar.Adjustment.Upper = TotalDP;

			foreach(var dp in DeliveryPointsList) {
				if(!dp.CoordinatesExist) {
					errors.Add(string.Format("{0} - Нет координат\n------\n", dp.Id));
					SkipedDP++;
				} else if(dp.FindAndAssociateDistrict(UoW, districtSource)) {
					SuccessDP++;
				} else {
					errors.Add(string.Format("{0} - Нет района доставки\n------\n", dp.Id));
					SkipedDP++;
				}

				progressCounter++;

				progressbar.Text = string.Format("Элемент: {0} из {1}", progressCounter, TotalDP);
				progressbar.Adjustment.Value = progressCounter;
				QSMain.WaitRedraw();
			}

			progressbar.Text = string.Format("Обработано {0} точек доставки", progressCounter);
		}

		private void Clear()
		{
			TotalDP = 0;
			ErrorsDP = 0;
			SkipedDP = 0;
			SuccessDP = 0;
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			Save();
		}

		void Save()
		{
			int counter = 0;
			int batchCounter = 0;
			progressbar.Adjustment.Value = 0;
			UoW.Session.SetBatchSize(500);
			foreach(var item in DeliveryPointsList) {
				try {
					UoW.Save<DeliveryPoint>(item);
				} catch(Exception ex) {
					errors.Add(string.Format("{0}\n{1}\n------\n", item.Id, ex.Message));
					ErrorsDP++;
				}
				if(batchCounter == 500) {
					UoW.Commit();
					batchCounter = 0;
					progressbar.Text = string.Format("Сохранение: {0} из {1}", counter, DeliveryPointsList.Count);
					progressbar.Adjustment.Value = counter;
					QSMain.WaitRedraw();
				}
				counter++;
				batchCounter++;
			}
			UoW.Commit();

			string fileName = String.Format("ErrorsDeliveryPoints_{0}.txt", DateTime.Now.ToString("yyyyMMddhhmmss"));
			if(errors.Any())
				File.WriteAllLines(fileName, errors);

			progressbar.Adjustment.Value = counter;
			progressbar.Text = String.Format("Сохранено {0} точек доставки. Детали в {1}.", counter, fileName);
		}

		protected void OnButtonLoadClicked(object sender, EventArgs e)
		{
			Load();
		}
	}
}