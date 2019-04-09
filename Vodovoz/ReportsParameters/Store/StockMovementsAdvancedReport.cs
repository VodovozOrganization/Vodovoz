using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz.ReportsParameters.Store
{
	public partial class StockMovementsAdvancedReport : Gtk.Bin, ISingleUoWDialog, IParametersWidget
	{
		class StockMovementsAdvancedReportNode : PropertyChangedBase
		{
			public int Id { get; set; }

			public string Name { get; set; }

			private bool selected;

			public bool Selected {
				get => selected;
				set => SetField(ref selected, value, () => Selected);
			}
		}

		/// <summary>
		/// Конструкция фильтра где фильтры могут быть связаны друг с другом,
		/// позволяет снимать выделение документов и фильтровать наборы данных других фильтров
		/// 
		/// Принцип работы: Выбираемые в одном фильтре Id передается связанным фильтрам по FilteringRelation
		/// в связанных фильтрах наборы данных фильтруются по переданным им Id, условия фильтрации определяются
		/// в функции переданной при создании фильтра.
		/// </summary>
		class Criterion
		{
			//Набор данных
			List<StockMovementsAdvancedReportNode> list = new List<StockMovementsAdvancedReportNode>();
			//Observable служит для подсчета количества выделенных элементов при их выделении
			public GenericObservableList<StockMovementsAdvancedReportNode> ObservableList;

			/// <summary>
			/// Функция по получению набора данных, принимающая массив id для фильтрации
			/// </summary>
			private Func<int[], List<StockMovementsAdvancedReportNode>> sourceFunction;

			public List<Criterion> UnselectRelation = new List<Criterion>();
			public List<Criterion> FilteringRelation = new List<Criterion>();
			/// <summary>
			/// Хранит массив id  
			/// </summary>
			public int[] FilteredId { get; set; }

			public event Action<string> Changed;

			public void SubcribeWithClearOld(Action<string> action)
			{
				Changed = delegate { };
				Changed += action;
			}

			public bool AnySelected => list.Any(x => x.Selected);

			public void Unselect()
			{
				if(AnySelected) {
					ObservableList.ElementChanged -= ObservableList_ElementChanged_Unselect;
					list.Where(x => x.Selected).ToList().ForEach(x => x.Selected = false);
					ObservableList.ElementChanged += ObservableList_ElementChanged_Unselect;
				}
			}

			public void RefreshItems()
			{
				list = sourceFunction.Invoke(FilteredId);
				ObservableList = new GenericObservableList<StockMovementsAdvancedReportNode>(list);

				ObservableList.ElementChanged -= ObservableList_ElementChanged_Unselect;
				ObservableList.ElementChanged += ObservableList_ElementChanged_Unselect;

				ObservableList.ElementChanged -= ObservableList_ElementChanged_Filtering;
				ObservableList.ElementChanged += ObservableList_ElementChanged_Filtering;
			}

			void ObservableList_ElementChanged_Unselect(object aList, int[] aIdx)
			{
				if(UnselectRelation.Any())
					UnselectRelation.ForEach(x => x.Unselect());

				Changed?.Invoke(SumSelected());
			}

			void ObservableList_ElementChanged_Filtering(object aList, int[] aIdx)
			{
				FilteringRelation.ForEach(x => x.Unselect());
				FilteringRelation.ForEach(x => x.FilteredId = ObservableList.Where(o => o.Selected).Select(o => o.Id).ToArray());
				FilteringRelation.ForEach(x => x.RefreshItems());
				Changed?.Invoke(SumSelected());
			}

			public Criterion(Func<int[], List<StockMovementsAdvancedReportNode>> sourceFunc)
			{
				sourceFunction = sourceFunc;
				RefreshItems();
			}

			string SumSelected() => ObservableList.Count(x => x.Selected).ToString();
		}

		enum FilterTypes
		{
			WarehouseInclude,
			WarehouseExclude,
			NomenclatureInclude,
			NomenclatureExclude,
			NomenclatureCategoryInclude,
			NomenclatureCategoryExclude,
			DocumentTypeInclude,
			DocumentTypeExclude
		}

		Dictionary<FilterTypes, Criterion> criterions = new Dictionary<FilterTypes, Criterion>();

		public StockMovementsAdvancedReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Today;
			ConfigureFilters();
			ytreeviewSelectedList.ColumnsConfig = columnsConfig;
		}

		private void ConfigureFilters()
		{
			//Склады
			Criterion warehouseIncludeCrit = CreateWarehouseCriterion();
			Criterion warehouseExcludeCrit = CreateWarehouseCriterion();
			//Номенклатуры
			Criterion nomenclatureIncludeCrit = CreateNomenclatureCriterion();
			Criterion nomenclatureExcludeCrit = CreateNomenclatureCriterion();
			//Категории номенклатур
			Criterion nomenclatureCategoryIncludeCrit = CreateNomenclatureCategoryCriterion();
			Criterion nomenclatureCategoryExcludeCrit = CreateNomenclatureCategoryCriterion();
			//Типы документов
			Criterion docTypeIncludeCrit = CreateDocTypeCriterion();
			Criterion docTypeExcludeCrit = CreateDocTypeCriterion();

			//Задание связей по фильтрации и снятию выделения между критериями
			//Склады
			warehouseIncludeCrit.UnselectRelation.Add(warehouseExcludeCrit);
			warehouseExcludeCrit.UnselectRelation.Add(warehouseIncludeCrit);
			//Номенклатура
			nomenclatureIncludeCrit.UnselectRelation.Add(nomenclatureExcludeCrit);
			nomenclatureExcludeCrit.UnselectRelation.Add(nomenclatureIncludeCrit);
			//Типы номенклатур
			nomenclatureCategoryIncludeCrit.FilteringRelation.Add(nomenclatureIncludeCrit);
			nomenclatureCategoryIncludeCrit.FilteringRelation.Add(nomenclatureExcludeCrit);
			nomenclatureCategoryIncludeCrit.UnselectRelation.Add(nomenclatureIncludeCrit);
			nomenclatureCategoryIncludeCrit.UnselectRelation.Add(nomenclatureExcludeCrit);
			nomenclatureCategoryIncludeCrit.UnselectRelation.Add(nomenclatureCategoryExcludeCrit);
			nomenclatureCategoryExcludeCrit.FilteringRelation.Add(nomenclatureIncludeCrit);
			nomenclatureCategoryExcludeCrit.FilteringRelation.Add(nomenclatureExcludeCrit);
			nomenclatureCategoryExcludeCrit.UnselectRelation.Add(nomenclatureIncludeCrit);
			nomenclatureCategoryExcludeCrit.UnselectRelation.Add(nomenclatureExcludeCrit);
			nomenclatureCategoryExcludeCrit.UnselectRelation.Add(nomenclatureCategoryIncludeCrit);
			//Типы документов
			docTypeIncludeCrit.UnselectRelation.Add(docTypeExcludeCrit);
			docTypeExcludeCrit.UnselectRelation.Add(docTypeIncludeCrit);
			//Сохранение фильтров для использования
			criterions.Add(FilterTypes.WarehouseInclude, warehouseIncludeCrit);
			criterions.Add(FilterTypes.WarehouseExclude, warehouseExcludeCrit);
			criterions.Add(FilterTypes.NomenclatureInclude, nomenclatureIncludeCrit);
			criterions.Add(FilterTypes.NomenclatureExclude, nomenclatureExcludeCrit);
			criterions.Add(FilterTypes.NomenclatureCategoryInclude, nomenclatureCategoryIncludeCrit);
			criterions.Add(FilterTypes.NomenclatureCategoryExclude, nomenclatureCategoryExcludeCrit);
			criterions.Add(FilterTypes.DocumentTypeInclude, docTypeIncludeCrit);
			criterions.Add(FilterTypes.DocumentTypeExclude, docTypeExcludeCrit);
		}

		#region Создание фильтров

		Criterion CreateWarehouseCriterion()
		{
			return new Criterion((arg) => {
				StockMovementsAdvancedReportNode resultAlias = null;
				Warehouse warehouseAlias = null;
				var query = UoW.Session.QueryOver(() => warehouseAlias)
									   .SelectList(list => list
													.Select(x => x.Id).WithAlias(() => resultAlias.Id)
													.Select(x => x.Name).WithAlias(() => resultAlias.Name)
												   )
									   .OrderBy(o => o.IsArchive)
									   .Asc
									   .TransformUsing(Transformers.AliasToBean<StockMovementsAdvancedReportNode>())
									   .List<StockMovementsAdvancedReportNode>();
				return query.ToList();
			});
		}

		Criterion CreateNomenclatureCategoryCriterion()
		{
			return new Criterion((arg) => {
				List<StockMovementsAdvancedReportNode> result = new List<StockMovementsAdvancedReportNode>();
				var categories = Enum.GetValues(typeof(NomenclatureCategory)).Cast<NomenclatureCategory>();
				foreach(var item in categories) {
					result.Add(new StockMovementsAdvancedReportNode() {
						Id = (int)item,
						Name = item.GetAttribute<DisplayAttribute>().Name
					});
				}
				return result;
			});
		}

		Criterion CreateNomenclatureCriterion()
		{
			return new Criterion((arg) => {
				StockMovementsAdvancedReportNode alias = null;
				var query = UoW.Session.QueryOver<Nomenclature>()
							   .Where(n => n.IsArchive == false);
				if(arg != null && arg.Any()) {
					NomenclatureCategory[] categories = new NomenclatureCategory[arg.Count()];
					for(int i = 0; i < arg.Count(); i++) {
						categories[i] = (NomenclatureCategory)arg[i];
					}
					query.WhereRestrictionOn(x => x.Category).IsIn(categories);
				}
				var queryResult = query.SelectList(list => list
						 .Select(x => x.Id).WithAlias(() => alias.Id)
						 .Select(x => x.Name).WithAlias(() => alias.Name)
						)
				.TransformUsing(Transformers.AliasToBean<StockMovementsAdvancedReportNode>())
				.List<StockMovementsAdvancedReportNode>();
				return queryResult.ToList();
			});
		}

		Criterion CreateDocTypeCriterion()
		{
			return new Criterion((arg) => {
				List<StockMovementsAdvancedReportNode> result = new List<StockMovementsAdvancedReportNode>();
				var workingDocuments = new[] { DocumentType.CarLoadDocument, DocumentType.CarUnloadDocument };
				foreach(var item in Enum.GetValues(typeof(DocumentType)).Cast<DocumentType>()) {
					if(workingDocuments.Contains(item))
						result.Add(new StockMovementsAdvancedReportNode {
							Id = (int)item,
							Name = item.GetAttribute<DisplayAttribute>().Name
						});
				}
				return result;
			});
		}

		#endregion

		IColumnsConfig columnsConfig = ColumnsConfigFactory.Create<StockMovementsAdvancedReportNode>()
														   .AddColumn("Выбрать").AddToggleRenderer(node => node.Selected)
															   .Editing()
														   .AddColumn("Название").AddTextRenderer(node => node.Name)
														   .Finish();

		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Развернутые движения ТМЦ";

		#endregion

		string[] GetDocumetTypes(int[] enumIds)
		{
			if(!enumIds.Any())
				return new[] { "0" };
			string[] result = new string[enumIds.Count()];
			for(int i = 0; i < enumIds.Count(); i++)
				result[i] = ((DocumentType)enumIds[i]).ToString();
			return result;
		}

		string[] GetCategories(int[] enumIds)
		{
			if(!enumIds.Any())
				return new[] { "0" };
			string[] result = new string[enumIds.Count()];
			for(int i = 0; i < enumIds.Count(); i++)
				result[i] = ((NomenclatureCategory)enumIds[i]).ToString();
			return result;
		}

		int[] GetResultIds(IEnumerable<int> ids) => ids.Any() ? ids.ToArray() : new[] { 0 };

		ReportInfo GetReportInfo()
		{
			string[] includeCategories = GetCategories(criterions[FilterTypes.NomenclatureCategoryInclude].ObservableList.Where(x => x.Selected).Select(d => d.Id).ToArray());
			string[] excludeCategories = GetCategories(criterions[FilterTypes.NomenclatureCategoryExclude].ObservableList.Where(x => x.Selected).Select(d => d.Id).ToArray());

			string[] includeDocTypes = GetDocumetTypes(criterions[FilterTypes.DocumentTypeInclude].ObservableList.Where(x => x.Selected).Select(d => d.Id).ToArray());
			string[] excludeDocTypes = GetDocumetTypes(criterions[FilterTypes.DocumentTypeExclude].ObservableList.Where(x => x.Selected).Select(d => d.Id).ToArray());

			return new ReportInfo {
				Identifier = "Store.StockMovementsAdvancedReport",
				Parameters = new Dictionary<string, object>
				{
					{ "start_date", dateperiodpicker.StartDateOrNull },
					{ "end_date", dateperiodpicker.EndDateOrNull },
					//склады
					{ "wh_include", GetResultIds(criterions[FilterTypes.WarehouseInclude].ObservableList.Where(x => x.Selected).Select(d => d.Id)) },
					{ "wh_exclude", GetResultIds(criterions[FilterTypes.WarehouseExclude].ObservableList.Where(x => x.Selected).Select(d => d.Id)) },
					//категории номенклатур
					{ "nomcat_include", includeCategories },
					{ "nomcat_exclude", excludeCategories },
					//номенклатуры
					{ "nomen_include", GetResultIds(criterions[FilterTypes.NomenclatureInclude].ObservableList.Where(x => x.Selected).Select(d => d.Id))},
					{ "nomen_exclude", GetResultIds(criterions[FilterTypes.NomenclatureExclude].ObservableList.Where(x => x.Selected).Select(d => d.Id))},
					//типы документов
					{ "doctype_include", includeDocTypes},
					{ "doctype_exclude", excludeDocTypes}
				}
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		GenericObservableList<StockMovementsAdvancedReportNode> treeNodes;

		protected void OnBtnWarehousesSelectClicked(object sender, EventArgs e)
		{
			treeNodes = criterions[FilterTypes.WarehouseInclude].ObservableList;
			ytreeviewSelectedList.ItemsDataSource = treeNodes;
			criterions[FilterTypes.WarehouseInclude].SubcribeWithClearOld(
				obj => yLblWarehouses.Text = string.Format("Вкл.: {0} елем.", obj)
			);
			labelTableTitle.Text = "Включаемые склады";
		}

		protected void OnBtnWarehousesDeselectClicked(object sender, EventArgs e)
		{
			treeNodes = criterions[FilterTypes.WarehouseExclude].ObservableList;
			ytreeviewSelectedList.ItemsDataSource = treeNodes;
			criterions[FilterTypes.WarehouseExclude].SubcribeWithClearOld(
				obj => yLblWarehouses.Text = string.Format("Искл.: {0} елем.", obj)
			);
			labelTableTitle.Text = "Исключаемые склады";
		}

		protected void OnButtonNomTypeSelectClicked(object sender, EventArgs e)
		{
			treeNodes = criterions[FilterTypes.NomenclatureCategoryInclude].ObservableList;
			ytreeviewSelectedList.ItemsDataSource = treeNodes;
			criterions[FilterTypes.NomenclatureCategoryInclude].SubcribeWithClearOld((string obj) => {
				ylabelNomType.Text = string.Format("Вкл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Включаемые категории номенклатуры";
		}

		protected void OnButtonNomTypeUnselectClicked(object sender, EventArgs e)
		{
			treeNodes = criterions[FilterTypes.NomenclatureCategoryExclude].ObservableList;
			ytreeviewSelectedList.ItemsDataSource = treeNodes;
			criterions[FilterTypes.NomenclatureCategoryExclude].SubcribeWithClearOld((string obj) => {
				ylabelNomType.Text = string.Format("Искл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Исключаемые категории номенклатуры";
		}

		protected void OnButtonNomenSelectClicked(object sender, EventArgs e)
		{
			treeNodes = criterions[FilterTypes.NomenclatureInclude].ObservableList;
			ytreeviewSelectedList.ItemsDataSource = treeNodes;
			criterions[FilterTypes.NomenclatureInclude].SubcribeWithClearOld((string obj) => {
				ylabelNomen.Text = string.Format("Вкл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Включаемые номенклатуры";
		}

		protected void OnButtonNomenUnselectClicked(object sender, EventArgs e)
		{
			treeNodes = criterions[FilterTypes.NomenclatureExclude].ObservableList;
			ytreeviewSelectedList.ItemsDataSource = treeNodes;
			criterions[FilterTypes.NomenclatureExclude].SubcribeWithClearOld((string obj) => {
				ylabelNomen.Text = string.Format("Искл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Исключаемые номенклатуры";
		}

		protected void OnBtnDocTypesSelectClicked(object sender, EventArgs e)
		{
			treeNodes = criterions[FilterTypes.DocumentTypeInclude].ObservableList;
			ytreeviewSelectedList.ItemsDataSource = treeNodes;
			criterions[FilterTypes.DocumentTypeInclude].SubcribeWithClearOld(
				obj => yLblDocTypes.Text = string.Format("Вкл.: {0} елем.", obj)
			);
			labelTableTitle.Text = "Включаемые типы документов";
		}

		protected void OnBtnDocTypesDeselectClicked(object sender, EventArgs e)
		{
			treeNodes = criterions[FilterTypes.DocumentTypeExclude].ObservableList;
			ytreeviewSelectedList.ItemsDataSource = treeNodes;
			criterions[FilterTypes.DocumentTypeExclude].SubcribeWithClearOld(
				obj => yLblDocTypes.Text = string.Format("Искл.: {0} елем.", obj)
			);
			labelTableTitle.Text = "Исключаемые типы документов";
		}

		protected void OnButtonSelectAllClicked(object sender, EventArgs e)
		{
			object source = ytreeviewSelectedList.ItemsDataSource;
			if(source is GenericObservableList<StockMovementsAdvancedReportNode>) {
				foreach(StockMovementsAdvancedReportNode item in (source as GenericObservableList<StockMovementsAdvancedReportNode>)) {
					item.Selected = true;
				}
			}
		}

		protected void OnButtonUnselectAllClicked(object sender, EventArgs e)
		{
			object source = ytreeviewSelectedList.ItemsDataSource;
			if(source is GenericObservableList<StockMovementsAdvancedReportNode>) {
				foreach(StockMovementsAdvancedReportNode item in (source as GenericObservableList<StockMovementsAdvancedReportNode>)) {
					item.Selected = false;
				}
			}
		}

		protected void OnSearchEntityInSelectedListTextChanged(object sender, EventArgs e)
		{
			if(treeNodes != null) {
				if(searchEntityInSelectedList.Text.Length > 0)
					ytreeviewSelectedList.ItemsDataSource = new GenericObservableList<StockMovementsAdvancedReportNode>(
						treeNodes
						.Where(
							n => n.Name
							.ToLower()
							.Contains(
								searchEntityInSelectedList
								.Text
								.ToLower()
							)
						)
						.ToList()
					);
				else
					ytreeviewSelectedList.ItemsDataSource = treeNodes;
			} else {
				searchEntityInSelectedList.Text = string.Empty;
			}
		}
	}
}