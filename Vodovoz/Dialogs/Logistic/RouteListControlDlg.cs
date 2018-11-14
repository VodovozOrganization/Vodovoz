using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using NLog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
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
			ytreeviewNotLoaded.ColumnsConfig = ColumnsConfigFactory.Create<RouteListControlNotLoadedNode>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.Nomenclature.OfficialName)
				.AddColumn("Склад").AddTextRenderer(x => x.Nomenclature.Warehouse == null ? "Не привязан к складу" : x.Nomenclature.Warehouse.Name)
				.AddColumn("Количество").AddNumericRenderer(x => x.Count)
				.RowCells()
				.Finish();

			ytreeviewNotAttached.ColumnsConfig = ColumnsConfigFactory.Create<Nomenclature>()
				.AddColumn("Номенклатура").AddTextRenderer(x => x.OfficialName)
				.RowCells()
				.Finish();
			
			ytreeviewNotLoaded.RowActivated += YtreeviewNotLoaded_RowActivated;;
			ytreeviewNotAttached.RowActivated += YtreeviewNotAttached_RowActivated;

			UpdateLists();
		}

		private void UpdateLists()
		{
			var inLoaded = Repository.Logistics.RouteListRepository.AllGoodsLoaded(UoW, Entity);
			var goods = Repository.Logistics.RouteListRepository.GetGoodsAndEquipsInRL(UoW, Entity);
			List<RouteListControlNotLoadedNode> notLoadedNomenclatures = new List<RouteListControlNotLoadedNode>();
			foreach(var good in goods) {
				var loaded = inLoaded.FirstOrDefault(x => x.NomenclatureId == good.NomenclatureId);
				decimal loadedAmount = 0;
				if(loaded != null) {
					loadedAmount = loaded.Amount;
				}
				if(loadedAmount < good.Amount) {
					notLoadedNomenclatures.Add(new RouteListControlNotLoadedNode() {
						NomenclatureId = good.NomenclatureId,
						Count = (int)(good.Amount - loadedAmount)
					});
				}
			}
			DomainHelper.FillPropertyByEntity<RouteListControlNotLoadedNode, Nomenclature>
					(UoW,
					 notLoadedNomenclatures,
					 x => x.NomenclatureId,
					 (node, obj) => { node.Nomenclature = obj; });
			ObservableNotLoadedList = new GenericObservableList<RouteListControlNotLoadedNode>(notLoadedNomenclatures);


			var notAttachedNomenclatures = UoW.Session.QueryOver<Nomenclature>()
			   .WhereRestrictionOn(x => x.Warehouse).IsNull
			   .WhereRestrictionOn(x => x.Id).IsIn(goods.Select(x => x.NomenclatureId).ToList())
			   .List();
			ObservableNotAttachedList = new GenericObservableList<Nomenclature>(notAttachedNomenclatures);
			ytreeviewNotLoaded.ItemsDataSource = ObservableNotLoadedList;
			ytreeviewNotAttached.ItemsDataSource = ObservableNotAttachedList;
		}

		void YtreeviewNotLoaded_RowActivated(object o, Gtk.RowActivatedArgs args)
		{
			var notLoadedNomenclature = ytreeviewNotLoaded.GetSelectedObject() as RouteListControlNotLoadedNode;
			if(notLoadedNomenclature == null) {
				return;
			}

			var dlg = new CarLoadDocumentDlg(Entity.Id, notLoadedNomenclature.Nomenclature.Warehouse?.Id);
			TabParent.AddTab(dlg, this);
		}

		void YtreeviewNotAttached_RowActivated(object o, Gtk.RowActivatedArgs args)
		{
			var notAttachedNomenclature = ytreeviewNotAttached.GetSelectedObject() as Nomenclature;
			if(notAttachedNomenclature == null) {
				return;
			}

			var dlg = new NomenclatureDlg(notAttachedNomenclature);
			TabParent.AddTab(dlg, this);
		}
	}

	public class RouteListControlNotLoadedNode
	{
		public int NomenclatureId { get; set; }
		public Nomenclature Nomenclature { get; set; }
		public int Count { get; set; }
	}
}
