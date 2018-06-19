﻿﻿using System;
using QSOrmProject;
using QSReport;
using Vodovoz.Domain.Goods;
using System.Collections.Generic;
using System.Linq;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate.Transform;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Reports
{
	public partial class SalesReport : Gtk.Bin, IOrmDialog, IParametersWidget
	{
		class SalesReportNode : PropertyChangedBase
		{
			public int Id { get; set; }

			public string Name { get; set; }

			private bool selected;

			public bool Selected { 
				get{ return selected; }
				set{ SetField(ref selected, value, () => Selected); }
			}
		}

		//TODO Если эта конструкция окажется жизнеспособной возможно стоит ее 
		//доработать и вынести в отдельное место для других отчетов.

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
			private List<SalesReportNode> list = new List<SalesReportNode>();
			//Observable служит для подсчета количества выделенных элементов при их выделении
			public GenericObservableList<SalesReportNode> ObservableList;

			/// <summary>
			/// Функция по получению набора данных, принимающая массив id для фильтрации
			/// </summary>
			private Func<int[], List<SalesReportNode>> sourceFunction;

			public List<Criterion> UnselectRelation = new List<Criterion>();
			public List<Criterion> FilteringRelation = new List<Criterion>();
			/// <summary>
			/// Хранит массив id  
			/// </summary>
			/// <value>The filtered identifier.</value>
			public int[] FilteredId { get; set; }

			public event Action<string> Changed;

			public void SubcribeWithClearOld(Action<string> action)
			{
				Changed = delegate {};
				Changed += action;
			}

			public bool HaveSelected{
				get{ return list.Any(x => x.Selected); }
			}

			public void Unselect()
			{
				if(HaveSelected) {
					ObservableList.ElementChanged -= ObservableList_ElementChanged_Unselect;
					list.Where(x => x.Selected).ToList().ForEach((obj) => { obj.Selected = false; });
					ObservableList.ElementChanged += ObservableList_ElementChanged_Unselect;
				}
			}

			public void RefreshItems()
			{
				list = sourceFunction.Invoke(FilteredId);
				ObservableList = new GenericObservableList<SalesReportNode>(list);

				ObservableList.ElementChanged -= ObservableList_ElementChanged_Unselect;
				ObservableList.ElementChanged += ObservableList_ElementChanged_Unselect;

				ObservableList.ElementChanged -= ObservableList_ElementChanged_Filtering;
				ObservableList.ElementChanged += ObservableList_ElementChanged_Filtering;
			}

			void ObservableList_ElementChanged_Unselect(object aList, int[] aIdx)
			{
				if(UnselectRelation.Any()) {
					UnselectRelation.ForEach(x => x.Unselect());
				}
				Changed?.Invoke(SumSelected());
			}

			void ObservableList_ElementChanged_Filtering(object aList, int[] aIdx)
			{
				FilteringRelation.ForEach(x => x.Unselect());
				FilteringRelation.ForEach(x => x.FilteredId = ObservableList.Where(o => o.Selected).Select(o => o.Id).ToArray());
				FilteringRelation.ForEach(x => x.RefreshItems());
				Changed?.Invoke(SumSelected());
			}

			public Criterion(Func<int[], List<SalesReportNode>> sourceFunc)
			{
				sourceFunction = sourceFunc;
				RefreshItems();
			}

			string SumSelected()
			{
				return ObservableList.Where(x => x.Selected).Count().ToString();
			}

		}

		enum FilterTypes
		{
			NomenclatureInclude,
			NomenclatureExclude,
			NomenclatureTypeInclude,
			NomenclatureTypeExclude,
			ClientInclude,
			ClientExclude,
			OrganizationInclude,
			OrganizationExclude,
			DiscountReasonInclude,
			DiscountReasonExclude
		}

		Dictionary<FilterTypes, Criterion> criterions = new Dictionary<FilterTypes, Criterion>();

		public SalesReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Today;
			ConfigureFilters();
			ytreeviewSelectedList.ColumnsConfig = columnsConfig;
		}

		private void ConfigureFilters()
		{
			//Номенклатура
			Criterion nomenclatureIncludeCrit = new Criterion((arg) => {
				SalesReportNode alias = null;
				var query = UoW.Session.QueryOver<Nomenclature>();
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
				.TransformUsing(Transformers.AliasToBean<SalesReportNode>())
				.List<SalesReportNode>();
				return queryResult.ToList();
			});
			Criterion nomenclatureExcludeCrit = new Criterion((arg) => {
				SalesReportNode alias = null;
				var query = UoW.Session.QueryOver<Nomenclature>();
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
				.TransformUsing(Transformers.AliasToBean<SalesReportNode>())
				.List<SalesReportNode>();
				return queryResult.ToList();
			});

			//Типы номенклатуры
			Criterion nomenclatureTypeIncludeCrit = new Criterion((arg) => {
				List<SalesReportNode> result = new List<SalesReportNode>();
				var categories = Enum.GetValues(typeof(NomenclatureCategory)).Cast<NomenclatureCategory>();
				foreach(var item in categories) {
					result.Add(new SalesReportNode() {
						Id = (int)item, 
						Name = item.GetAttribute<DisplayAttribute>().Name
					});
				}
				return result;
			});
			Criterion nomenclatureTypeExcludeCrit = new Criterion((arg) => {
				List<SalesReportNode> result = new List<SalesReportNode>();
				var categories = Enum.GetValues(typeof(NomenclatureCategory)).Cast<NomenclatureCategory>();
				foreach(var item in categories) {
					result.Add(new SalesReportNode() {
						Id = (int)item,
						Name = item.GetAttribute<DisplayAttribute>().Name
					});
				}
				return result;
			});

			// Клиенты
			Criterion clientIncludeCrit = new Criterion((arg) => {
				SalesReportNode alias = null;
				var query = UoW.Session.QueryOver<Counterparty>();
				var queryResult = query.SelectList(list => list
						 .Select(x => x.Id).WithAlias(() => alias.Id)
						 .Select(x => x.Name).WithAlias(() => alias.Name)
						)
				.TransformUsing(Transformers.AliasToBean<SalesReportNode>())
				.List<SalesReportNode>();
				return queryResult.ToList();
			});
			Criterion clientExcludeCrit = new Criterion((arg) => {
				SalesReportNode alias = null;
				var query = UoW.Session.QueryOver<Counterparty>();
				var queryResult = query.SelectList(list => list
						 .Select(x => x.Id).WithAlias(() => alias.Id)
						 .Select(x => x.Name).WithAlias(() => alias.Name)
						)
				.TransformUsing(Transformers.AliasToBean<SalesReportNode>())
				.List<SalesReportNode>();
				return queryResult.ToList();
			});

			// Поставщики (организации)
			Criterion organizationIncludeCrit = new Criterion((arg) => {
				SalesReportNode alias = null;
				var query = UoW.Session.QueryOver<Organization>();
				var queryResult = query.SelectList(list => list
						 .Select(x => x.Id).WithAlias(() => alias.Id)
						 .Select(x => x.Name).WithAlias(() => alias.Name)
						)
				.TransformUsing(Transformers.AliasToBean<SalesReportNode>())
				.List<SalesReportNode>();
				return queryResult.ToList();
			});
			Criterion organizationExcludeCrit = new Criterion((arg) => {
				SalesReportNode alias = null;
				var query = UoW.Session.QueryOver<Organization>();
				var queryResult = query.SelectList(list => list
						 .Select(x => x.Id).WithAlias(() => alias.Id)
						 .Select(x => x.Name).WithAlias(() => alias.Name)
						)
				.TransformUsing(Transformers.AliasToBean<SalesReportNode>())
				.List<SalesReportNode>();
				return queryResult.ToList();
			});

			// Основания скидок
			Criterion discountReasonIncludeCrit = new Criterion((arg) => {
				SalesReportNode alias = null;
				var query = UoW.Session.QueryOver<DiscountReason>();
				var queryResult = query.SelectList(list => list
						 .Select(x => x.Id).WithAlias(() => alias.Id)
						 .Select(x => x.Name).WithAlias(() => alias.Name)
						)
				.TransformUsing(Transformers.AliasToBean<SalesReportNode>())
				.List<SalesReportNode>();
				return queryResult.ToList();
			});
			Criterion discountReasonExcludeCrit = new Criterion((arg) => {
				SalesReportNode alias = null;
				var query = UoW.Session.QueryOver<DiscountReason>();
				var queryResult = query.SelectList(list => list
						 .Select(x => x.Id).WithAlias(() => alias.Id)
						 .Select(x => x.Name).WithAlias(() => alias.Name)
						)
				.TransformUsing(Transformers.AliasToBean<SalesReportNode>())
				.List<SalesReportNode>();
				return queryResult.ToList();
			});

			//Задание связей по фильтрации и снятию выделения между критериями
			//Номенклатура
			nomenclatureIncludeCrit.UnselectRelation.Add(nomenclatureExcludeCrit);
			nomenclatureExcludeCrit.UnselectRelation.Add(nomenclatureIncludeCrit);
			//Типы номенклатур
			nomenclatureTypeIncludeCrit.FilteringRelation.Add(nomenclatureIncludeCrit);
			nomenclatureTypeIncludeCrit.FilteringRelation.Add(nomenclatureExcludeCrit);
			nomenclatureTypeIncludeCrit.UnselectRelation.Add(nomenclatureIncludeCrit);
			nomenclatureTypeIncludeCrit.UnselectRelation.Add(nomenclatureExcludeCrit);
			nomenclatureTypeIncludeCrit.UnselectRelation.Add(nomenclatureTypeExcludeCrit);
			nomenclatureTypeExcludeCrit.FilteringRelation.Add(nomenclatureIncludeCrit);
			nomenclatureTypeExcludeCrit.FilteringRelation.Add(nomenclatureExcludeCrit);
			nomenclatureTypeExcludeCrit.UnselectRelation.Add(nomenclatureIncludeCrit);
			nomenclatureTypeExcludeCrit.UnselectRelation.Add(nomenclatureExcludeCrit);
			nomenclatureTypeExcludeCrit.UnselectRelation.Add(nomenclatureTypeIncludeCrit);
			//Клиенты
			clientIncludeCrit.UnselectRelation.Add(clientExcludeCrit);
			clientExcludeCrit.UnselectRelation.Add(clientIncludeCrit);
			//Организации
			organizationIncludeCrit.UnselectRelation.Add(organizationExcludeCrit);
			organizationExcludeCrit.UnselectRelation.Add(organizationIncludeCrit);
			//Основания для скидок
			discountReasonIncludeCrit.UnselectRelation.Add(discountReasonExcludeCrit);
			discountReasonExcludeCrit.UnselectRelation.Add(discountReasonIncludeCrit);

			//Сохранение фильтров для использования
			criterions.Add(FilterTypes.NomenclatureInclude, nomenclatureIncludeCrit);
			criterions.Add(FilterTypes.NomenclatureExclude, nomenclatureExcludeCrit);
			criterions.Add(FilterTypes.NomenclatureTypeInclude, nomenclatureTypeIncludeCrit);
			criterions.Add(FilterTypes.NomenclatureTypeExclude, nomenclatureTypeExcludeCrit);
			criterions.Add(FilterTypes.ClientInclude, clientIncludeCrit);
			criterions.Add(FilterTypes.ClientExclude, clientExcludeCrit);
			criterions.Add(FilterTypes.OrganizationInclude, organizationIncludeCrit);
			criterions.Add(FilterTypes.OrganizationExclude, organizationExcludeCrit);
			criterions.Add(FilterTypes.DiscountReasonInclude, discountReasonIncludeCrit);
			criterions.Add(FilterTypes.DiscountReasonExclude, discountReasonExcludeCrit);
		}

		private IColumnsConfig columnsConfig = ColumnsConfigFactory
			.Create<SalesReportNode>()
			.AddColumn("Выбрать").AddToggleRenderer(node => node.Selected).Editing()
			.AddColumn("Название").AddTextRenderer(node => node.Name)
			.Finish();


		#region IOrmDialog implementation

		public IUnitOfWork UoW { get; private set; }

		public object EntityObject {
			get {
				return null;
			}
		}

		#endregion

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title {
			get {
				return "Отчет по продажам";
			}
		}

		#endregion

		private string[] GetCategories(int[] enumIds)
		{
			if(!enumIds.Any()) {
				return new string[] { "0" };
			}
			string[] result = new string[enumIds.Count()];
			for(int i = 0; i < enumIds.Count(); i++) {
				result[i] = ((NomenclatureCategory)enumIds[i]).ToString();
			}
			return result;
		}

		private int[] GetResultIds(IEnumerable<int> ids)
		{
			if(ids.Any()) {
				return ids.ToArray();
			}else {
				return new int[] { 0 };
			}
		}

		private ReportInfo GetReportInfo()
		{
			string[] includeCategories = GetCategories(criterions[FilterTypes.NomenclatureTypeInclude].ObservableList.Where(x => x.Selected).Select(d => d.Id).ToArray());
			string[] excludeCategories = GetCategories(criterions[FilterTypes.NomenclatureTypeExclude].ObservableList.Where(x => x.Selected).Select(d => d.Id).ToArray());

			string identifier;
			if(ycheckbuttonDetail.Active) {
				identifier = "Sales.SalesReportDetail";
			}else {
				identifier = "Sales.SalesReport";
			}

			return new ReportInfo {
				Identifier = identifier,
				Parameters = new Dictionary<string, object>
					{
						{ "start_date", dateperiodpicker.StartDateOrNull },
						{ "end_date", dateperiodpicker.EndDateOrNull },
						//тип номенклатур
						{ "nomtype_include", includeCategories },
						{ "nomtype_exclude", excludeCategories },
						//номенклатуры
						{ "nomen_include", GetResultIds(criterions[FilterTypes.NomenclatureInclude].ObservableList.Where(x => x.Selected).Select(d => d.Id))},
						{ "nomen_exclude", GetResultIds(criterions[FilterTypes.NomenclatureExclude].ObservableList.Where(x => x.Selected).Select(d => d.Id))},
						//клиенты
						{ "client_include", GetResultIds(criterions[FilterTypes.ClientInclude].ObservableList.Where(x => x.Selected).Select(d => d.Id)) },
						{ "client_exclude", GetResultIds(criterions[FilterTypes.ClientExclude].ObservableList.Where(x => x.Selected).Select(d => d.Id)) },
						//поставщики (наши организации)
						{ "org_include", GetResultIds(criterions[FilterTypes.OrganizationInclude].ObservableList.Where(x => x.Selected).Select(d => d.Id)) },
						{ "org_exclude", GetResultIds(criterions[FilterTypes.OrganizationExclude].ObservableList.Where(x => x.Selected).Select(d => d.Id)) },
						//основания для скидок
						{ "discountreason_include", GetResultIds(criterions[FilterTypes.DiscountReasonInclude].ObservableList.Where(x => x.Selected).Select(d => d.Id)) },
						{ "discountreason_exclude", GetResultIds(criterions[FilterTypes.DiscountReasonExclude].ObservableList.Where(x => x.Selected).Select(d => d.Id)) }
					}
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		void OnUpdate(bool hide = false)
		{
			if(LoadReport != null) {
				LoadReport(this, new LoadReportEventArgs(GetReportInfo(), hide));
			}
		}

		protected void OnButtonNomTypeSelectClicked(object sender, EventArgs e)
		{
			ytreeviewSelectedList.ItemsDataSource = criterions[FilterTypes.NomenclatureTypeInclude].ObservableList;
			criterions[FilterTypes.NomenclatureTypeInclude].SubcribeWithClearOld((string obj) => {
				ylabelNomType.Text = String.Format("Вкл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Включаемые категории номенклатуры";
		}

		protected void OnButtonNomenSelectClicked(object sender, EventArgs e)
		{
			ytreeviewSelectedList.ItemsDataSource = criterions[FilterTypes.NomenclatureInclude].ObservableList;
			criterions[FilterTypes.NomenclatureInclude].SubcribeWithClearOld((string obj) => {
				ylabelNomen.Text = String.Format("Вкл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Включаемые номенклатуры";
		}

		protected void OnButtonNomTypeUnselectClicked(object sender, EventArgs e)
		{
			ytreeviewSelectedList.ItemsDataSource = criterions[FilterTypes.NomenclatureTypeExclude].ObservableList;
			criterions[FilterTypes.NomenclatureTypeExclude].SubcribeWithClearOld((string obj) => {
				ylabelNomType.Text = String.Format("Искл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Исключаемые категории номенклатуры";
		}

		protected void OnButtonNomenUnselectClicked(object sender, EventArgs e)
		{
			ytreeviewSelectedList.ItemsDataSource = criterions[FilterTypes.NomenclatureExclude].ObservableList;
			criterions[FilterTypes.NomenclatureExclude].SubcribeWithClearOld((string obj) => {
				ylabelNomen.Text = String.Format("Искл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Исключаемые номенклатуры";
		}

		protected void OnButtonClientSelectClicked(object sender, EventArgs e)
		{
			ytreeviewSelectedList.ItemsDataSource = criterions[FilterTypes.ClientInclude].ObservableList;
			criterions[FilterTypes.ClientInclude].SubcribeWithClearOld((string obj) => {
				ylabelClient.Text = String.Format("Вкл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Включаемые номенклатуры";
		}

		protected void OnButtonClientUnselectClicked(object sender, EventArgs e)
		{
			ytreeviewSelectedList.ItemsDataSource = criterions[FilterTypes.ClientExclude].ObservableList;
			criterions[FilterTypes.ClientExclude].SubcribeWithClearOld((string obj) => {
				ylabelClient.Text = String.Format("Искл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Исключаемые контрагенты";
		}

		protected void OnButtonOrgSelectClicked(object sender, EventArgs e)
		{
			ytreeviewSelectedList.ItemsDataSource = criterions[FilterTypes.OrganizationInclude].ObservableList;
			criterions[FilterTypes.OrganizationInclude].SubcribeWithClearOld((string obj) => {
				ylabelOrg.Text = String.Format("Вкл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Включаемые номенклатуры";
		}

		protected void OnButtonOrgUnselectClicked(object sender, EventArgs e)
		{
			ytreeviewSelectedList.ItemsDataSource = criterions[FilterTypes.OrganizationExclude].ObservableList;
			criterions[FilterTypes.OrganizationExclude].SubcribeWithClearOld((string obj) => {
				ylabelOrg.Text = String.Format("Искл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Исключаемые поставщики";
		}

		protected void OnButtonDiscountReasonSelectClicked(object sender, EventArgs e)
		{
			ytreeviewSelectedList.ItemsDataSource = criterions[FilterTypes.DiscountReasonInclude].ObservableList;
			criterions[FilterTypes.DiscountReasonInclude].SubcribeWithClearOld((string obj) => {
				ylabelDiscountReason.Text = String.Format("Вкл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Включаемые номенклатуры";
		}

		protected void OnButtonDiscountReasonUnselectClicked(object sender, EventArgs e)
		{
			ytreeviewSelectedList.ItemsDataSource = criterions[FilterTypes.DiscountReasonExclude].ObservableList;
			criterions[FilterTypes.DiscountReasonExclude].SubcribeWithClearOld((string obj) => {
				ylabelDiscountReason.Text = String.Format("Искл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Исключаемые основания скидок";
		}

		protected void OnButtonSelectAllClicked(object sender, EventArgs e)
		{
			object source = ytreeviewSelectedList.ItemsDataSource;
			if(source is GenericObservableList<SalesReportNode>) {
				foreach(SalesReportNode item in (source as GenericObservableList<SalesReportNode>)) {
					item.Selected = true;
				}
			}
		}

		protected void OnButtonUnselectAllClicked(object sender, EventArgs e)
		{
			object source = ytreeviewSelectedList.ItemsDataSource;
			if(source is GenericObservableList<SalesReportNode>) {
				foreach(SalesReportNode item in (source as GenericObservableList<SalesReportNode>)) {
					item.Selected = false;
				}
			}
		}
	}
}

