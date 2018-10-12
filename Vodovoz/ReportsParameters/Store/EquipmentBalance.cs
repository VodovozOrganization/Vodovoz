using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Binding;
using Gamma.ColumnConfig;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSReport;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ReportsParameters.Store
{
	public partial class EquipmentBalance : Gtk.Bin, IParametersWidget
	{
		IUnitOfWork uow;

		GenericObservableList<SelectableNomenclatureTypeNode> observableItems { get; set; }

		public EquipmentBalance()
		{
			this.Build();
			uow = UnitOfWorkFactory.CreateWithoutRoot();

			var categoryList =  Enum.GetValues(typeof(NomenclatureCategory)).Cast<NomenclatureCategory>().ToList();

			List<SelectableNomenclatureTypeNode> items = new List<SelectableNomenclatureTypeNode>();

			foreach(var cat in categoryList) {
				var node = new SelectableNomenclatureTypeNode();
				node.Category = cat;
				var attr = cat.GetAttribute<DisplayAttribute>();
				if(attr == null) {
					node.Title = cat.ToString();
				} else {
					node.Title = attr.Name;
				}
				if(cat == NomenclatureCategory.equipment) {
					node.Children = GenerateEnumCategoryNodes();
				}
				//РАСКОМЕНТИРОВАТЬ ПРИ ПЕРЕХОДЕ НА ОБЯЗАТЕЛЬНОЕ УКАЗАНИЕ ГРУППЫ У ТОВАРОВ
				/*if(cat == NomenclatureCategory.additional) {
					var groups = uow.Session.QueryOver<ProductGroup>().List().ToList();
					node.Children = GenerateGoodsGroupNodes(groups, null);
				}*/
				node.Children.ForEach(x => x.Parent = node);
				items.Add(node);
			}

			observableItems = new GenericObservableList<SelectableNomenclatureTypeNode>(items);
			observableItems.ListContentChanged += ObservableItemsField_ListContentChanged;

			ytreeviewCategories.ColumnsConfig = FluentColumnsConfig<SelectableNomenclatureTypeNode>
				.Create()
				.AddColumn("Выбрать").AddToggleRenderer(node => node.Selected).Editing()
				.AddColumn("Название").AddTextRenderer(node => node.Title)
				.Finish();
			
			ytreeviewCategories.YTreeModel = new RecursiveTreeModel<SelectableNomenclatureTypeNode>(observableItems, x => x.Parent, x => x.Children);
		}
		
		public List<SelectableNomenclatureTypeNode> GenerateEnumCategoryNodes()
		{
			var result = new List<SelectableNomenclatureTypeNode>();
			var categoryList = Enum.GetValues(typeof(SubtypeOfEquipmentCategory)).Cast<SubtypeOfEquipmentCategory>().ToList();
			foreach(var cat in categoryList) {
				var node = new SelectableNomenclatureTypeNode();
				node.Category = NomenclatureCategory.equipment;
				node.Subject = cat;
				var attr = cat.GetAttribute<DisplayAttribute>();
				if(attr == null) {
					node.Title = cat.ToString();
				}else {
					node.Title = attr.Name;
				}
				result.Add(node);
			}
			return result;
		}

		public List<SelectableNomenclatureTypeNode> GenerateGoodsGroupNodes(List<ProductGroup> groups, ProductGroup parent)
		{
			var result = new List<SelectableNomenclatureTypeNode>();

			foreach(var item in groups.Where(x => x.Parent == parent)) {
				var subNode = new SelectableNomenclatureTypeNode();
				subNode.Category = NomenclatureCategory.additional;
				subNode.Subject = item;
				subNode.Title = item.Name;
				subNode.Children = GenerateGoodsGroupNodes(groups, item);
				subNode.Children.ForEach(x => x.Parent = subNode);
				result.Add(subNode);
			}
			return result;
		}
		#region IParametersWidget implementation

		public string Title => "ТМЦ на остатках";

		public event EventHandler<LoadReportEventArgs> LoadReport;

		#endregion

		void ObservableItemsField_ListContentChanged(object sender, EventArgs e)
		{
			ytreeviewCategories.QueueDraw();
		}

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		private ReportInfo GetReportInfo()
		{
			var leafs = observableItems.SelectMany(x => x.Children).SelectMany(x => x.GetAllLeafs());

			string[] categories = observableItems.Where(x => x.Parent == null)
			                                     .Where(x => x.Selected)
			                                     .Select(x => x.CategoryName)
			                                     .ToArray();
			if(!categories.Any()) {
				categories = new string[] { "0" };
			}
			
			string[] equipments = leafs.Where(x => x.Category == NomenclatureCategory.equipment)
												 .Where(x => x.Selected)
			                                     .Select(x => x.SubCategory)
												 .ToArray();
			if(!equipments.Any()) {
				equipments = new string[] { "0" };
			}

			string[] additional = leafs.Where(x => x.Category == NomenclatureCategory.additional)
												 .Where(x => x.Selected)
												 .Select(x => x.SubCategory)
												 .ToArray();
			if(!additional.Any()) {
				additional = new string[] { "0" };
			}

			return new ReportInfo {
				Identifier = "Store.EquipmentBalance",
				Parameters = new Dictionary<string, object>
				{
					{ "categories", categories }, //все выбранные категории номенклатур без подтипов
					{ "equipments", equipments }, //все выбранные подтипы категории оборудования
					{ "additional", additional } //все выбранные подтипы категории товаров
				}
			};
		}
	}

	public class SelectableNomenclatureTypeNode : PropertyChangedBase
	{
		private bool selected;
		public bool Selected {
			get { return selected; }
			set { 
				if(SetField(ref selected, value, () => Selected)){
					if(Parent != null) {
						if(value) {
							Parent.SelectOnlyThisNode(value);
						} else {
							Parent.TryUnselect();
						}
					}
					Children.ForEach(x => x.Selected = value);
				}
			}
		}

		public void SelectOnlyThisNode(bool value)
		{
			selected = value;
			OnPropertyChanged(() => Selected);
		}

		public void TryUnselect()
		{
			if(!Children.Any(x => x.Selected)){
				selected = false;
			}
			OnPropertyChanged(() => Selected);
		}

		public NomenclatureCategory Category { get; set; }

		public virtual object Subject { get; set; }

		public string SubCategory {
			get{
				if(Subject is ProductGroup) {
					return (Subject as ProductGroup).Id.ToString();
				}

				if(Subject is SubtypeOfEquipmentCategory) {
					return ((SubtypeOfEquipmentCategory)Subject).ToString();
				}

				return "";
			}
		}

		public string CategoryName => Category.ToString();

		public string Title { get; set; }

		public List<SelectableNomenclatureTypeNode> GetAllLeafs()
		{
			var result = new List<SelectableNomenclatureTypeNode>();
			if(Children.Any()) {
				foreach(var child in Children.Where(x => x.Selected)){
					result.AddRange(child.GetAllLeafs());
				}
			}else {
				result.Add(this);
			}
			return result;
		}

		public SelectableNomenclatureTypeNode Parent { get; set; }

		public List<SelectableNomenclatureTypeNode> Children { get; set; } = new List<SelectableNomenclatureTypeNode>();

	}
}
