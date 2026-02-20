using Autofac;
using Gtk;
using NHibernate.Transform;
using NHibernate.Util;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewWidgets.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DefectiveItemsReceptionView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private GenericObservableList<DefectiveItemNode> _defectiveList = new GenericObservableList<DefectiveItemNode>();
		private bool? _userHasOnlyAccessToWarehouseAndComplaints;

		public IList<DefectiveItemNode> Items => _defectiveList;

		public INavigationManager NavigationManager { get; } = Startup.MainWin.NavigationManager;

		public void AddItem(DefectiveItemNode item) => _defectiveList.Add(item);

		public DefectiveItemsReceptionView()
		{
			Build();

			List<CullingCategory> types;
			using(IUnitOfWork uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot()) {
				types = uow.GetAll<CullingCategory>().OrderBy(c => c.Name).ToList();
			}

			ytreeReturns.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<DefectiveItemNode>()
				.AddColumn("Номенклатура").AddTextRenderer(node => node.Name)
				.AddColumn("Кол-во").AddNumericRenderer(node => node.Amount, new RoundedDecimalToStringConverter())
				.Adjustment(new Adjustment(0, 0, 9999, 1, 100, 0))
				.Editing(true)
				.AddColumn("Тип брака")
					.AddComboRenderer(x => x.TypeOfDefect)
					.SetDisplayFunc(x => x.Name)
					.FillItems(types)
					.AddSetter(
						(c, n) =>
						{
							c.Editable = true;
							c.BackgroundGdk = n.TypeOfDefect == null
								? GdkColors.DangerBase
								: GdkColors.PrimaryBase;
						}
					)
				.AddColumn("Источник\nбрака")
					.AddEnumRenderer(x => x.Source, true, new Enum[] { DefectSource.None })
					.AddSetter(
						(c, n) =>
						{
							c.Editable = true;
							c.BackgroundGdk = n.Source == DefectSource.None
								? GdkColors.DangerBase
								: GdkColors.PrimaryBase;
						}
					)
				.AddColumn("")
				.Finish();

			ytreeReturns.ItemsDataSource = _defectiveList;
		}

		private IUnitOfWork _uow;

		public IUnitOfWork UoW
		{
			get
			{
				return _uow;
			}
			set
			{
				if(_uow == value)
				{
					return;
				}

				_uow = value;
			}
		}

		private Warehouse _warehouse;
		public Warehouse Warehouse
		{
			get
			{
				return _warehouse;
			}
			set
			{
				_warehouse = value;
				FillDefectiveListFromRoute();
			}
		}

		private RouteList _routeList;
		public RouteList RouteList
		{
			get
			{
				return _routeList;
			}
			set
			{
				if(_routeList == value)
				{
					return;
				}

				_routeList = value;
				if(_routeList != null)
				{
					FillDefectiveListFromRoute();
				}
				else
				{
					_defectiveList.Clear();
				}

			}
		}

		public new bool Sensitive
		{
			set => ytreeReturns.Sensitive = buttonAddNomenclature.Sensitive = value;
		}

		private void FillDefectiveListFromRoute()
		{
			if(Warehouse == null || RouteList == null)
			{
				return;
			}

			DefectiveItemNode resultAlias = null;
			GoodsAccountingOperation operationAlias = null;
			CarUnloadDocument carUnloadDocumentAlias = null;
			CarUnloadDocumentItem carUnloadDocumentItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			var defectiveItems = UoW.Session.QueryOver(() => carUnloadDocumentItemAlias)
				.Left.JoinAlias(() => carUnloadDocumentItemAlias.Document, () => carUnloadDocumentAlias)
				.Where(() => carUnloadDocumentAlias.RouteList.Id == RouteList.Id)
				.Left.JoinAlias(() => carUnloadDocumentItemAlias.GoodsAccountingOperation, () => operationAlias)
				.Left.JoinAlias(() => operationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.IsDefectiveBottle)
				.SelectList(
					list => list
					.Select(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
					.Select(() => operationAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => carUnloadDocumentItemAlias.GoodsAccountingOperation).WithAlias(() => resultAlias.MovementOperation)
					.Select(() => carUnloadDocumentItemAlias.DefectSource).WithAlias(() => resultAlias.Source)
					.Select(() => carUnloadDocumentItemAlias.TypeOfDefect).WithAlias(() => resultAlias.TypeOfDefect)
				   )
				.TransformUsing(Transformers.AliasToBean<DefectiveItemNode>())
				.List<DefectiveItemNode>();

			defectiveItems.ForEach(i => _defectiveList.Add(i));
		}

		protected void OnButtonAddNomenclatureClicked(object sender, EventArgs e)
		{
			if(_userHasOnlyAccessToWarehouseAndComplaints == null)
			{
				_userHasOnlyAccessToWarehouseAndComplaints =
					ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(
						Vodovoz.Core.Domain.Permissions.UserPermissions.UserHaveAccessOnlyToWarehouseAndComplaints)
					&& !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin;
			}

			(NavigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
				MyTab,
				filter =>
				{
					filter.IsDefectiveBottle = true;
				},
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.OnSelectResult += Journal_OnEntitySelectedResult;
					viewModel.SelectionMode = JournalSelectionMode.Multiple;
					viewModel.HideButtons();
				});
		}

		private void Journal_OnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var selectedNodes = e.SelectedObjects.Cast<NomenclatureJournalNode>();

			if(!selectedNodes.Any())
			{
				return;
			}

			var nomenclatures = UoW.GetById<Nomenclature>(selectedNodes.Select(x => x.Id));
			foreach(var nomenclature in nomenclatures)
			{
				_defectiveList.Add(new DefectiveItemNode(nomenclature, 0));
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;
		}
	}
}
