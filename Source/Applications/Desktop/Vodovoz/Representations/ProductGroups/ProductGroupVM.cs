using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Gamma.Binding;
using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.StoredResources;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Parameters;
using Vodovoz.Settings.Database;
using Vodovoz.TreeModels;
using UseForSearchAttribute = QSOrmProject.RepresentationModel.UseForSearchAttribute;

namespace Vodovoz.Representations.ProductGroups
{
	public class ProductGroupVM: RepresentationModelEntityBase<ProductGroup, ProductGroupVMNode>
	{
		private static Pixbuf _img = new Pixbuf(typeof(Startup).Assembly, "Vodovoz.icons.common.folder16.png");
		private static Pixbuf _emptyImg = new Pixbuf(typeof(Startup).Assembly, "Vodovoz.icons.common.empty16.png");

		public ProductGroupVM(IUnitOfWork uow)
		{
			UoW = uow;
			CreateRepresentationFilter = () => {
				var filter = new ProductGroupFilterViewModel();
				return filter;
			};
		}

		public ProductGroupVM() : this(UnitOfWorkFactory.CreateWithoutRoot()) { }
		
		public ProductGroupVM(IUnitOfWork uow, ProductGroupFilterViewModel filterViewModel) : this(uow)
		{
			Filter = filterViewModel;
		}
		
		public override IList ItemsList => _filteredItemsList ?? itemsList as IList;

		private IList _filteredItemsList;
		
		public ProductGroupFilterViewModel Filter {
			get => RepresentationFilter as ProductGroupFilterViewModel;
			set => RepresentationFilter = value;
		}

		private IyTreeModel iyTreeModel;
		public override IyTreeModel YTreeModel {
			get => iyTreeModel;
			protected set => SetField(ref iyTreeModel, value);
		}

		protected override PropertyInfo[] SearchPropCache
		{
			get
			{
				if(searchPropCache != null)
					return searchPropCache;

				searchPropCache = typeof(INameNode).GetProperties()
					.Where(prop => prop.GetCustomAttributes(typeof(UseForSearchAttribute), true).Length > 0
						|| prop.GetCustomAttributes(typeof(QS.RepresentationModel.GtkUI.UseForSearchAttribute), true).Length > 0)
					.ToArray();

				return searchPropCache;
			}
		}
		
		protected override void RefilterList()
		{
			var filtredParam = string.Join(", ", SearchStrings.Select(x => $"<{x}>"));
			logger.Info("Фильтрация таблицы по {0}...", filtredParam);
			var searchStarted = DateTime.Now;
			var childs =
				itemsList.AsParallel()
					.Where(x => x.ChildNomenclatures != null)
					.SelectMany(x => x.ChildNomenclatures).ToList();
			var childMatches =
				childs.AsParallel().Where(SearchFilterFunc).OfType<INameNode>().ToList();
			var groupsMatches = itemsList.AsParallel().Where(SearchFilterFunc).OfType<INameNode>().ToList();
			_filteredItemsList = groupsMatches;

			foreach(var item in childMatches)
			{
				_filteredItemsList.Add(item);
			}

			var delay = DateTime.Now.Subtract (searchStarted);
			logger.Debug ($"Поиск нашел {groupsMatches.Count + childMatches.Count} элементов за {delay.TotalSeconds} секунд.");
			logger.Info("Ок");
			Gtk.Application.Invoke(delegate {
				OnItemsListUpdated ();
			});
		}

		private bool SearchFilterFunc(INameNode item)
		{
			foreach (var searchString in SearchStrings)
			{
				var found = false;
				foreach (var prop in SearchPropCache)
				{
					string Str = (prop.GetValue(item, null) ?? string.Empty).ToString();
					if(Str.IndexOf(searchString, StringComparison.CurrentCultureIgnoreCase) > -1)
					{
						found = true;
						break;
					}
				}
				if(!found)
				{
					return false;
				}
			}
			return true;
		}

		public override IColumnsConfig ColumnsConfig { get; } = FluentColumnsConfig<INameNode>.Create()
			.AddColumn("Код")
				.AddTextRenderer(node => node.Id.ToString())
				.AddPixbufRenderer(node => node.GetType() != typeof(NomenclatureNode) ? _img : _emptyImg)
			.AddColumn("Название").AddTextRenderer(node => node.Name)
			.RowCells()
			.AddSetter<CellRendererText>(
				(c, n) => c.Foreground = n.IsArchive ? "grey" : "black"
			)
			.Finish();

		public override void UpdateNodes()
		{
			NomenclatureNode nomenclatureNodeAlias = null;
			ProductGroupVMNode resultAlias = null;
			var query = UoW.Session.QueryOver<ProductGroup>();

			if(Filter.HideArchive)
			{
				query.Where(x => !x.IsArchive);
			}
			
			var allProductGroupNodes = query
				.SelectList(list => list
					.Select(p => p.Id).WithAlias(() => resultAlias.Id)
					.Select(p => p.Name).WithAlias(() => resultAlias.Name)
					.Select(p => p.Parent.Id).WithAlias(() => resultAlias.ParentId)
					.Select(p => p.IsArchive).WithAlias(() => resultAlias.IsArchive))
				.TransformUsing(Transformers.AliasToBean<ProductGroupVMNode>())
				.List<ProductGroupVMNode>();
			
			var nomenclaturesQuery = UoW.Session.QueryOver<Nomenclature>();
				
			if(Filter.HideArchive)
			{
				nomenclaturesQuery.Where(x => !x.IsArchive);
			}
				
			var nomenclatures = nomenclaturesQuery.SelectList(list => list
				.Select(n => n.Id).WithAlias(() => nomenclatureNodeAlias.Id)
				.Select(n => n.Name).WithAlias(() => nomenclatureNodeAlias.Name)
				.Select(n => n.ProductGroup.Id).WithAlias(() => resultAlias.ParentId)
				.Select(n => n.IsArchive).WithAlias(() => nomenclatureNodeAlias.IsArchive))
			.TransformUsing(Transformers.AliasToBean<NomenclatureNode>())
			.List<NomenclatureNode>();

			Parallel.ForEach(allProductGroupNodes, SetChildren);

			void SetChildren(ProductGroupVMNode node)
			{
				var children = allProductGroupNodes.Where(s => s.ParentId == node.Id).ToList();
				node.ChildGroups = children;
				var nomenclatureNodes = nomenclatures.Where(x => x.ParentId == node.Id).ToList();

				if(nomenclatureNodes.Any())
				{
					foreach(var item in nomenclatureNodes)
					{
						item.Parent = node;
						node.ChildNomenclatures.Add(item);
					}
				}

				foreach(var n in children)
				{
					n.Parent = node;
				}
			}

			SetItemsSource(allProductGroupNodes);

			var parentsList = allProductGroupNodes.Where(x => x.Parent == null).ToList();
			
			var config = new List<IModelConfig>
			{
				new ModelConfig<ProductGroupVMNode, ProductGroupVMNode, NomenclatureNode>(
					x => x.Parent,
					x => x.ChildGroups,
					x => x.ChildNomenclatures),
				new ModelConfig<NomenclatureNode, ProductGroupVMNode>(
					x => x.Parent)
			};
			
			YTreeModel = new RecursiveTreeModelWithCustomModel<ProductGroupVMNode>(parentsList, config);
		}

		public bool NeedUpdate { get; set; }
		protected override bool NeedUpdateFunc(ProductGroup updatedSubject) => NeedUpdate;
	}
}
