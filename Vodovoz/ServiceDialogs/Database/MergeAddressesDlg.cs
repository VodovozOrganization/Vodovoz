using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using DiffPlex;
using DiffPlex.DiffBuilder;
using Gamma.ColumnConfig;
using NHibernate.Criterion;
using QSHistoryLog;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain.Client;

namespace Vodovoz.ServiceDialogs.Database
{
	public partial class MergeAddressesDlg : TdiTabBase
	{
		IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		List<DublicateNode> Duplicates;
		GenericObservableList<DublicateNode> ObservableDuplicates;

		public MergeAddressesDlg()
		{
			this.Build();

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

			var list = uow.Session.QueryOver<DeliveryPoint>(() => mainPointAlias)
						  .WithSubquery.WhereExists(dublicateSubquery)
			              .OrderBy(x => x.Counterparty).Asc
			              .ThenBy(x => x.Latitude).Asc
			              .ThenBy(x => x.Longitude).Asc
			              .List();

			progressOp.Adjustment.Upper = list.Count + 2;
			progressOp.Text = "Обрабатываем адреса...";
			progressOp.Adjustment.Value++;
			QSMain.WaitRedraw();

			Duplicates = new List<DublicateNode>();
			DublicateNode lastDuplicate = null;
			foreach(var dp in list) {
				if(lastDuplicate == null || !lastDuplicate.Compare(dp)) {
					lastDuplicate = new DublicateNode();
					Duplicates.Add(lastDuplicate);
				}
				lastDuplicate.Addresses.Add(new AddressNode(lastDuplicate, dp));
				progressOp.Adjustment.Value++;
				QSMain.WaitRedraw();
			}

			Duplicates.ForEach(x => x.FineMain());
			progressOp.Adjustment.Value++;
			QSMain.WaitRedraw();

			ObservableDuplicates = new GenericObservableList<DublicateNode>(Duplicates);

			ytreeviewDuplicates.SetItemsSource(ObservableDuplicates);
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

	}
}

