using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using QSValidation;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Dialogs.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListControlDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();


		public GenericObservableList<RouteListControlNotLoadedNode> ObservableNotLoadedList { get; set; }
			= new GenericObservableList<RouteListControlNotLoadedNode>();

		public GenericObservableList<Nomenclature> ObservableNotAttachedList { get; set; }
			= new GenericObservableList<Nomenclature>();

		public RouteListControlDlg(RouteList sub) : this(sub.Id) { }

		public RouteListControlDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			ConfigureDlg();
		}

		public override bool Save()
		{
			var valid = new QSValidator<RouteList>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем маршрутный лист...");
			UoWGeneric.Save();
			logger.Info("Ok");
			return true;
		}

		private void ConfigureDlg()
		{
			btnSendEnRoute.Visible = Entity.Status == RouteListStatus.InLoading;
			ytreeviewNotLoaded.ColumnsConfig = ColumnsConfigFactory.Create<RouteListControlNotLoadedNode>()
				.AddColumn("Номенклатура")
					.AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Возможные склады")
					.AddTextRenderer(x => string.Join(", ", x.Nomenclature.Warehouses.Select(w => w.Name)))
				.AddColumn("Погружено")
					.AddTextRenderer(x => x.CountLoadedString, useMarkup: true)
				.AddColumn("Всего")
					.AddNumericRenderer(x => x.CountTotal)
				.AddColumn("Осталось погрузить")
					.AddNumericRenderer(x => x.CountNotLoaded)
				.RowCells()
				.Finish();

			ytreeviewNotAttached.ColumnsConfig = ColumnsConfigFactory.Create<Nomenclature>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Name)
				.RowCells()
				.Finish();

			ytreeviewNotLoaded.RowActivated += YtreeviewNotLoaded_RowActivated; ;
			ytreeviewNotAttached.RowActivated += YtreeviewNotAttached_RowActivated;

			UpdateLists();
		}

		private void UpdateLists()
		{
			var goods = Repository.Logistics.RouteListRepository.GetGoodsAndEquipsInRL(UoW, Entity);
			var notLoadedNomenclatures = Entity.NotLoadedNomenclatures();
			ObservableNotLoadedList = new GenericObservableList<RouteListControlNotLoadedNode>(notLoadedNomenclatures);

			var notAttachedNomenclatures = UoW.Session.QueryOver<Nomenclature>()
											  .WhereRestrictionOn(x => x.Id).IsIn(goods.Select(x => x.NomenclatureId).ToList())
											  .List()
											  .Where(n => !n.Warehouses.Any())
											  .ToList();
			ObservableNotAttachedList = new GenericObservableList<Nomenclature>(notAttachedNomenclatures);
			ytreeviewNotLoaded.ItemsDataSource = ObservableNotLoadedList;
			ytreeviewNotAttached.ItemsDataSource = ObservableNotAttachedList;
		}

		void YtreeviewNotLoaded_RowActivated(object o, Gtk.RowActivatedArgs args)
		{
			if(ytreeviewNotLoaded.GetSelectedObject() is RouteListControlNotLoadedNode notLoadedNomenclature) {
				throw new NotImplementedException("\n\nПоломали, т.к. складов в номенклатуре теперь несколько!\n");
				/*var dlg = new CarLoadDocumentDlg(Entity.Id, notLoadedNomenclature.Nomenclature.Warehouse?.Id);
				TabParent.AddTab(dlg, this);*/
			}
		}

		void YtreeviewNotAttached_RowActivated(object o, Gtk.RowActivatedArgs args)
		{
			if(ytreeviewNotAttached.GetSelectedObject() is Nomenclature notAttachedNomenclature)
				TabParent.AddTab(new NomenclatureDlg(notAttachedNomenclature), this);
		}

		protected void OnBtnSendEnRouteClicked(object sender, EventArgs e)
		{
			#region костыль
			//FIXME пока не можем найти причину бага с несменой статуса на в пути при полной отгрузке, позволяем логистам отправлять МЛ в путь из этого диалога
			bool fullyLoaded = false;
			if(Entity.ShipIfCan(UoW)) {
				fullyLoaded = true;
				MessageDialogHelper.RunInfoDialog("Маршрутный лист отгружен полностью.");
			}
			#endregion

			if(
				!fullyLoaded &&
				UserPermissionRepository.CurrentUserPresetPermissions["can_send_not_loaded_route_lists_en_route"] &&
				MessageDialogHelper.RunQuestionWithTitleDialog(
					"Оптправить в путь?",
					string.Format(
						"{0} погружен <span foreground=\"Red\">НЕ ПОЛНОСТЬЮ</span> и будет переведён в статус \"{1}\". После сохранения изменений откат этого действия будет невозможен.\nВы уверены что хотите отправить МЛ в путь?",
						Entity.Title,
						RouteListStatus.EnRoute.GetEnumTitle()
					)
				)
			) {
				Entity.ChangeStatus(RouteListStatus.EnRoute);
				Entity.NotFullyLoaded = true;
			} else if(!fullyLoaded && !UserPermissionRepository.CurrentUserPresetPermissions["can_send_not_loaded_route_lists_en_route"]) {
				MessageDialogHelper.RunWarningDialog(
					"Недостаточно прав",
					string.Format("У вас нет прав для перевода не полностью погруженных МЛ в статус \"{0}\"", RouteListStatus.EnRoute.GetEnumTitle()),
					Gtk.ButtonsType.Ok
				);
			}
		}
	}
}