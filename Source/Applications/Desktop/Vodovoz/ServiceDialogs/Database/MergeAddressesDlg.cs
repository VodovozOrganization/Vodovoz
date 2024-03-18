using DiffPlex;
using DiffPlex.DiffBuilder;
using Gamma.ColumnConfig;
using NHibernate.Criterion;
using QS.Deletion;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Services;
using QSProjectsLib;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Fias.Client;
using Vodovoz.Domain.Client;
using Vodovoz.Factories;
using Autofac;

namespace Vodovoz.ServiceDialogs.Database
{
	public partial class MergeAddressesDlg : QS.Dialog.Gtk.TdiTabBase
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private readonly IUnitOfWork _uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot();
		private List<DublicateNode> _duplicates;
		private GenericObservableList<DublicateNode> _observableDuplicates;
		private readonly ReplaceEntity _replaceEntity;
		private readonly IDeliveryPointViewModelFactory _deliveryPointViewModelFactory;

		public MergeAddressesDlg(IFiasApiClient fiasApiClient)
		{
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("database_maintenance")) {
				MessageDialogHelper.RunWarningDialog("Доступ запрещён!", "У вас недостаточно прав для доступа к этой вкладке. Обратитесь к своему руководителю.", Gtk.ButtonsType.Ok);
				FailInitialize = true;
				return;
			}

			Build();

			TabName = "Дубликаты адресов";

			ytreeviewDuplicates.ColumnsConfig = FluentColumnsConfig<DublicateNode>.Create()
				.AddColumn("Слить").AddToggleRenderer(x => x.Selected).Editing()
				.AddColumn("Контрагент").AddTextRenderer(x => x.CounterParty)
				.AddColumn("Адрес 1с").AddTextRenderer(x => x.FirstAddress1c)
				.Finish();
			ytreeviewDuplicates.Selection.Changed += DuplicateSelection_Changed;

			ytreeviewAddresses.ColumnsConfig = FluentColumnsConfig<AddressNode>.Create()
				.AddColumn("Главный").AddToggleRenderer(x => x.IsMain).Editing().Radio()
				.AddColumn("Не трогать").AddToggleRenderer(x => x.Ignore).Editing()
				.AddColumn("Код 1С").AddTextRenderer(x => x.Address.Code1c)
				.AddColumn("Адрес 1с").AddTextRenderer(x => x.PangoText, useMarkup: true)
				.Finish();

			_replaceEntity = new ReplaceEntity(DeleteConfig.Main);
			_deliveryPointViewModelFactory = new DeliveryPointViewModelFactory(_lifetimeScope);
		}

		void DuplicateSelection_Changed(object sender, EventArgs e)
		{
			var selected = ytreeviewDuplicates.GetSelectedObject<DublicateNode>();
			if(selected != null)
				ytreeviewAddresses.SetItemsSource(new GenericObservableList<AddressNode>(selected.Addresses));
			else
				ytreeviewAddresses.ItemsDataSource = null;
		}

		protected void OnButtonFineDuplicatesClicked(object sender, EventArgs e)
		{
			progressOp.Visible = true;
			progressOp.Adjustment.Value = 0;
			progressOp.Text = "Получаем дубликаты адресов из базы...";
			QSMain.WaitRedraw();

			DeliveryPoint mainPointAlias = null;

			var dublicateSubquery = QueryOver.Of<DeliveryPoint>()
			                                 .Where(x => x.Counterparty.Id == mainPointAlias.Counterparty.Id
			                                        && x.Latitude == mainPointAlias.Latitude
			                                        && x.Longitude == mainPointAlias.Longitude
			                                        && x.Id != mainPointAlias.Id
			                                        && (x.Code1c == null || mainPointAlias.Code1c == null || x.Code1c == mainPointAlias.Code1c))
			                                 .Select(x => x.Id);

			var list = _uow.Session.QueryOver<DeliveryPoint>(() => mainPointAlias)
						  .WithSubquery.WhereExists(dublicateSubquery)
			              .Fetch(x => x.Counterparty).Eager
			              .OrderBy(x => x.Counterparty).Asc
			              .ThenBy(x => x.Latitude).Asc
			              .ThenBy(x => x.Longitude).Asc
			              .List();

			progressOp.Adjustment.Upper = list.Count + 3;
			progressOp.Text = "Обрабатываем адреса...";
			progressOp.Adjustment.Value++;
			QSMain.WaitRedraw();

			_duplicates = new List<DublicateNode>();
			DublicateNode lastDuplicate = null;
			foreach(var dp in list) {
				if(lastDuplicate == null || !lastDuplicate.Compare(dp)) {
					lastDuplicate = new DublicateNode();
					_duplicates.Add(lastDuplicate);
				}
				lastDuplicate.Addresses.Add(new AddressNode(lastDuplicate, dp));
				progressOp.Adjustment.Value++;
				QSMain.WaitRedraw();
			}

			_duplicates.ForEach(x => x.FineMain());
			progressOp.Adjustment.Value++;
			QSMain.WaitRedraw();

			_duplicates = _duplicates.OrderBy(x => x.CounterParty).ThenBy(x => x.FirstAddress1c).ToList();
			progressOp.Adjustment.Value++;
			QSMain.WaitRedraw();

			_observableDuplicates = new GenericObservableList<DublicateNode>(_duplicates);

			ytreeviewDuplicates.SetItemsSource(_observableDuplicates);
			progressOp.Visible = false;
		}

		protected void OnYtreeviewDuplicatesKeyReleaseEvent(object o, Gtk.KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Gdk.Key.space)
			{
				var selected = ytreeviewDuplicates.GetSelectedObject<DublicateNode>();
				if(selected != null)
					selected.Selected = !selected.Selected;
			}
		}

		protected void OnButtonApplyClicked(object sender, EventArgs e)
		{
			var mergeList = _duplicates.Where(x => x.Selected).ToList();
			progressOp.Visible = true;
			progressOp.Adjustment.Value = 0;
			progressOp.Adjustment.Upper = mergeList.Count;
			progressOp.Text = "Ищем ссылки...";
			QSMain.WaitRedraw();
			var totalLinks = 0;

			foreach(var dup in mergeList) {
				var main = dup.Addresses.First(x => x.IsMain);
				foreach(var deleted in dup.Addresses.Where(x => !x.IsMain && !x.Ignore))
				{
					totalLinks += _replaceEntity.ReplaceEverywhere(_uow, deleted.Address, main.Address);
					_uow.Delete(deleted.Address);
					_uow.Commit();

					progressOp.Text = $"Ищем ссылки... Заменено {totalLinks} ссылок.";
					QSMain.WaitRedraw();
				}

				_observableDuplicates.Remove(dup);
				progressOp.Adjustment.Value++;
				QSMain.WaitRedraw();
			}
			progressOp.Text = $"Готово. Заменено {totalLinks} ссылок.";
		}

		class DublicateNode : PropertyChangedBase{
			bool selected;

			public bool Selected {
				get {
					return selected;
				}

				set {
					SetField(ref selected, value, () => Selected);
				}
			}

			public List<AddressNode> Addresses = new List<AddressNode>();

			public string CounterParty{
				get{
					return Addresses.FirstOrDefault()?.Address.Counterparty.Name;
				}
			}

			public string FirstAddress1c {
				get {
					return Addresses.FirstOrDefault(x => x.IsMain)?.Address.Address1c;
				}
			}

			public bool Compare(DeliveryPoint dp)
			{
				var first = Addresses.First().Address;
				return first.Counterparty.Id == dp.Counterparty.Id
					        && first.Latitude == dp.Latitude
					        && first.Longitude == dp.Longitude;
			}

			public void FineMain()
			{
				var withCode = Addresses.Where(x => x.Address.Code1c != null).ToList();
				if(withCode.Count == 1)
					withCode[0].IsMain = true;
				else {
					var maxId = Addresses.Max(x => x.Address.Id);
					Addresses.Where(x => x.Address.Id == maxId).First().IsMain = true;
				}
			}


		}

		class AddressNode : PropertyChangedBase{
			bool ignore;

			public bool Ignore {
				get {
					return ignore;
				}

				set {
					SetField(ref ignore, value, () => Ignore);
				}
			}
			bool isMain;

			public bool IsMain {
				get {
					return isMain;
				}

				set {
					if(value)
						myDuplicateNode.Addresses.ForEach(x => x.IsMain = false);
					if(SetField(ref isMain, value, () => IsMain))
					{
						if(isMain && pangoText != null)
						{
							myDuplicateNode.Addresses.ForEach(x => x.MakeDiffPangoMarkup());
						}
					}

				}
			}

			public DeliveryPoint Address;
			private DublicateNode myDuplicateNode;

			public AddressNode (DublicateNode dupNode, DeliveryPoint dp)
			{
				Address = dp;
				myDuplicateNode = dupNode;
			}

			string pangoText = null;
			public virtual string PangoText {
				get {
					if(pangoText == null)
						MakeDiffPangoMarkup();
					return pangoText;
				}
				private set {
					SetField(ref pangoText, value, () => PangoText);
				}
			}

			public void MakeDiffPangoMarkup()
			{
				var d = new Differ();
				var differ = new SideBySideFullDiffBuilder(d);

				AddressNode pair;

				if(myDuplicateNode.Addresses.Count == 2)
				{
					pair = myDuplicateNode.Addresses.First(x => x.IsMain != IsMain);
					var diffRes = differ.BuildDiffModel(pair.Address.Address1c, Address.Address1c);
					PangoText = PangoRender.RenderDiffLines(diffRes.NewText);
				}
				else
				{
					if(IsMain)
						PangoText = Address.Address1c;
					else
					{
						pair = myDuplicateNode.Addresses.First(x => x.IsMain);
						var diffRes = differ.BuildDiffModel(pair.Address.Address1c, Address.Address1c);
						PangoText = PangoRender.RenderDiffLines(diffRes.NewText);
					}
				}
			}

		}

		protected void OnYtreeviewAddressesRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			var id = ytreeviewAddresses.GetSelectedObject<AddressNode>().Address.Id;
			var dpViewModel = _deliveryPointViewModelFactory.GetForOpenDeliveryPointViewModel(id);
			TabParent.AddSlaveTab(this, dpViewModel);
		}

		public override void Destroy()
		{
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
			base.Destroy();
		}
	}
}
