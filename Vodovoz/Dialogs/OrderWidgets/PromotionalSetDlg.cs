using System.Linq;
using Gamma.ColumnConfig;
using Gtk;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalFilters;
using Vodovoz.ViewModel;

namespace Vodovoz.Dialogs.OrderWidgets
{
	public partial class PromotionalSetDlg : QS.Dialog.Gtk.EntityDialogBase<PromotionalSet>
	{
		PromotionalSetItem selectedItem;

		public PromotionalSetDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<PromotionalSet>();
			ConfigureDlg();
		}

		public PromotionalSetDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<PromotionalSet>(id);
			ConfigureDlg();
		}

		public PromotionalSetDlg(PromotionalSet sub) : this(sub.Id) { }

		void ConfigureDlg()
		{
			yCmbDiscountReason.SetRenderTextFunc<DiscountReason>(x => x.Name);
			yCmbDiscountReason.ItemsList = UoW.Session.QueryOver<DiscountReason>().List();
			yCmbDiscountReason.Binding.AddBinding(Entity, e => e.PromoSetName, w => w.SelectedItem).InitializeFromSource();

			yChkIsArchive.Binding.AddBinding(Entity, s => s.IsArchive, w => w.Active).InitializeFromSource();
			yTreePromoSetItems.Selection.Changed += (sender, e) => {
				selectedItem = yTreePromoSetItems.GetSelectedObject<PromotionalSetItem>();
				SetControlsAcessibility();
			};
			yTreePromoSetItems.ColumnsConfig = new FluentColumnsConfig<PromotionalSetItem>()
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(i => i.Nomenclature.Id.ToString())
				.AddColumn("Товар")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(i => i.Nomenclature.Name)
				.AddColumn("Кол-во")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(i => i.Count)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter((c, n) => c.Digits = n.Nomenclature.Unit == null ? 0 : (uint)n.Nomenclature.Unit.Digits)
					.WidthChars(10)
					.Editing()
				.AddTextRenderer(i => i.Nomenclature.Unit == null ? string.Empty : i.Nomenclature.Unit.Name, false)
				.AddColumn("Скидка")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(i => i.Discount).Editing(true)
					.Adjustment(new Adjustment(0, 0, 100, 1, 100, 1))
					.Digits(2)
					.WidthChars(10)
					.AddTextRenderer(n => "%", false)
				.AddColumn("")
				.Finish();
			yTreePromoSetItems.ItemsDataSource = Entity.ObservablePromotionalSetItems;
			if(Entity.Id > 0)
				lblCreationDate.Text = Entity.CreateDate.ToString("G");
		}

		public override bool Save()
		{
			var valid = new QSValidator<PromotionalSet>(Entity);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;
			UoWGeneric.Save();
			return true;
		}

		protected void OnBtnDeleteClicked(object sender, System.EventArgs e)
		{
			if(selectedItem != null)
				Entity.ObservablePromotionalSetItems.Remove(selectedItem);
		}

		protected void OnBtnAddNomenclatureClicked(object sender, System.EventArgs e)
		{
			var nomenclatureFilter = new NomenclatureRepFilter(UoWGeneric);
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder(),
				x => x.DefaultSelectedCategory = NomenclatureCategory.water,
				x => x.DefaultSelectedSubCategory = SubtypeOfEquipmentCategory.forSale
			);
			ReferenceRepresentation SelectDialog = new ReferenceRepresentation(new NomenclatureForSaleVM(nomenclatureFilter)) {
				Mode = OrmReferenceMode.MultiSelect,
				TabName = "Номенклатуры на продажу",
				ShowFilter = true
			};
			SelectDialog.ObjectSelected += (s, ea) => {
				var nomenclaturesForSaleVMNode = ea.Selected.Select(o => o.VMNode as NomenclatureForSaleVMNode);
				foreach(var nomenclatureForSaleVMNode in nomenclaturesForSaleVMNode) {
					Nomenclature n = UoWGeneric.GetById<Nomenclature>(nomenclatureForSaleVMNode.Id);
					Entity.ObservablePromotionalSetItems.Add(
						new PromotionalSetItem {
							Discount = 0,
							Count = 0,
							Nomenclature = n,
							PromoSet = Entity
						}
					);
				}
			};
			TabParent.AddSlaveTab(this, SelectDialog);
		}

		void SetControlsAcessibility()
		{
			btnDelete.Sensitive = selectedItem != null;
		}
	}
}