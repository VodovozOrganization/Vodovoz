using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using QS.Project.Repositories;
using QS.Project.Services;
using Autofac;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Settings.Common;
using QS.Osrm;

namespace Vodovoz.ServiceDialogs.Database
{
	public partial class CalculateDistanceToPointsDlg : QS.Dialog.Gtk.TdiTabBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		int SaveBy = 300;

		IUnitOfWorkFactory _uowFactory = ScopeProvider.Scope.Resolve<IUnitOfWorkFactory>();
		IDeliveryRepository _deliveryRepository = ScopeProvider.Scope.Resolve<IDeliveryRepository>();
		IOsrmSettings _globalSettings = ScopeProvider.Scope.Resolve<IOsrmSettings>();
		IOsrmClient _osrmClient = ScopeProvider.Scope.Resolve<IOsrmClient>();

		IUnitOfWork _uow;
		IList<DeliveryPoint> points;

		public CalculateDistanceToPointsDlg()
		{
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("database_maintenance")) {
				MessageDialogHelper.RunWarningDialog(
					"Доступ запрещён!",
					"У вас недостаточно прав для доступа к этой вкладке. Обратитесь к своему руководителю.",
					Gtk.ButtonsType.Ok
				);
				FailInitialize = true;
				return;
			}

			this.Build();
			TabName = "Расчет расстояний до точек";
			_uow = _uowFactory.CreateWithoutRoot();
			_uow.Session.SetBatchSize(SaveBy);
		}

		protected void OnButtonLoadClicked(object sender, EventArgs e)
		{
			logger.Info("Загружаем все точки доставки...");
			points = _uow.Session.QueryOver<DeliveryPoint>()
			            //.Where(x => x.DistanceFromBaseMeters == null && x.Latitude != null && x.Longitude != null)
			            .Fetch(x => x.DeliverySchedule).Lazy
						.List();

			labelTotal.LabelProp = points.Count.ToString();

			if(points.Any())
				buttonCalculate.Sensitive = true;

			logger.Info("Ок");
		}

		protected void OnButtonCalculateClicked(object sender, EventArgs e)
		{

			progressbar1.Adjustment.Value = 0;
			progressbar1.Adjustment.Upper = points.Count;
			logger.Info("Рассчитываем расстояния от склада...");
			int notSaved = 0;
			int calculated = 0;
			int saved = 0;
			foreach(var point in points)
			{
				point.SetСoordinates(point.Latitude, point.Longitude, _deliveryRepository, _globalSettings, _osrmClient);
				notSaved++;
				calculated++;
				labelCalculated.LabelProp = calculated.ToString();
				progressbar1.Adjustment.Value++;
				_uow.Save(point);
				if(notSaved >= SaveBy || calculated == points.Count)
				{
					logger.Info("Сохраняем {0} точек в базу...", notSaved);
					_uow.Commit();
					saved += notSaved;
					labelSaved.LabelProp = saved.ToString();
					notSaved = 0;
					logger.Info("Рассчитываем расстояния от центра...");
				}
				QSMain.WaitRedraw();
			}
			logger.Info("Ок");
		}
	}
}
