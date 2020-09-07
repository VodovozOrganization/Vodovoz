using System.Collections.Generic;
using System.Linq;
using Gamma.Binding;
using Gamma.ColumnConfig;
using Gtk;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Goods;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Representations
{
    public class ProductGroupVM: RepresentationModelEntityBase<ProductGroup, ProductGroupVMNode>
    {
        public ProductGroupVM(IUnitOfWork uow)
        {
            UoW = uow;
            CreateRepresentationFilter = () => {
                filter = new ProductGroupFilterViewModel();
                return filter;
            };
        }

        public ProductGroupVM() : this(UnitOfWorkFactory.CreateWithoutRoot()) { }
        
        public ProductGroupVM(IUnitOfWork uow, ProductGroupFilterViewModel filterViewModel) : this(uow)
        {
            Filter = filterViewModel;
        }
        
        
        private ProductGroupFilterViewModel filter;
        public ProductGroupFilterViewModel Filter {
            get => filter;
            set 
            {
                if(filter != value) {
                    filter = value;
                    filter.OnFiltered += (sender, e) => UpdateNodes();
                }
            }
        }

        private IyTreeModel iyTreeModel;
        public override IyTreeModel YTreeModel {
            get => iyTreeModel;
            protected set => SetField(ref iyTreeModel, value);
        }

        public override IColumnsConfig ColumnsConfig { get; } = FluentColumnsConfig<ProductGroupVMNode>.Create()
            .AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
            .AddColumn("Название").AddTextRenderer(node => node.Name)
            .RowCells()
            .AddSetter<CellRendererText>(
                (c, n) => c.Foreground = n.IsArchive ? "grey" : "black"
            )
            .Finish();

        public override void UpdateNodes()
        {
            ProductGroup productGroupAlias = null;
            ProductGroupVMNode resultAlias = null;
            var tasksQuery = UoW.Session.QueryOver<ProductGroup>(() => productGroupAlias);

            if (Filter.HideArchive)
            {
                tasksQuery.Where(x => !x.IsArchive);
            }
            
            var allProductGroupNodes = tasksQuery.SelectList(list => list
                .Select(() => productGroupAlias.Id).WithAlias(() => resultAlias.Id)
                .Select(() => productGroupAlias.Name).WithAlias(() => resultAlias.Name)
                .Select(() => productGroupAlias.Parent.Id).WithAlias(() => resultAlias.ParentId)
                .Select(() => productGroupAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
                )
                .TransformUsing(Transformers.AliasToBean<ProductGroupVMNode>())
                .List<ProductGroupVMNode>();
            
            foreach(var r in allProductGroupNodes)
                SetChildren(r);
            
            void SetChildren(ProductGroupVMNode node)
            {
                var children = allProductGroupNodes.Where(s => s.ParentId == node.Id).ToList();
                node.Children = children;
                foreach(var n in children) {
                    n.Parent = node;
                    SetChildren(n);
                }
            }

            SetItemsSource(allProductGroupNodes);
            
            YTreeModel = new RecursiveTreeModel<ProductGroupVMNode>(allProductGroupNodes.Where((x) => x.ParentId == null).ToList(),
                x => x.Parent,
                x => x.Children);
        }

        public bool NeedUpdate { get; set; }
        protected override bool NeedUpdateFunc(ProductGroup updatedSubject) => NeedUpdate;
        
    }
    
    public class ProductGroupVMNode
    {
        [UseForSearch]
        [SearchHighlight]
        public int Id { get; set; }
        [UseForSearch]
        [SearchHighlight]
        public string Name { get; set; }
        public bool IsArchive { get; set; }
        public IList<ProductGroupVMNode> Children { get; set; } = new List<ProductGroupVMNode>();
        public ProductGroupVMNode Parent { get; set; }
        public int? ParentId { get; set; }

    }
    
}