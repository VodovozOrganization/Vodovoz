using System;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModel
{
	public class ProxyDocumentsVM : RepresentationModelEntityBase<ProxyDocument, ProxyDocumentsVMNode>
	{
		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			var proxiesList = UoW.Session.QueryOver<ProxyDocument>()
								 .OrderBy(d => d.Id).Desc
								 .TransformUsing(Transformers.AliasToBean<ProxyDocumentsVMNode>())
								 .List<ProxyDocumentsVMNode>();

			SetItemsSource(proxiesList);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ProxyDocumentsVMNode>.Create()
		.AddColumn("Тип и номер").AddTextRenderer(node => String.Format("{0} №{1} от {2:d}", node.Type.GetEnumTitle(), node.Id, node.Date))
		.AddColumn("Начало действия").AddTextRenderer(node => String.Format("{0:d}", node.Date))
		.AddColumn("Окончание действия").AddTextRenderer(node => String.Format("{0:d}", node.ExpirationDate))
		.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = (DateTime.Today > n.ExpirationDate) ? "grey" : "black")
		.Finish();
		
		public override IColumnsConfig ColumnsConfig => columnsConfig;

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(ProxyDocument updatedSubject)
		{
			return true;
		}

		#endregion

		#region Конструкторы

		public ProxyDocumentsVM() { }

		public ProxyDocumentsVM(IUnitOfWork uow) : base()
		{
			this.UoW = uow;
		}

		#endregion
	}

	public class ProxyDocumentsVMNode
	{
		[UseForSearch]
		public int Id { get; set; }
		public DateTime Date { get; set; }
		public DateTime ExpirationDate { get; set; }
		[UseForSearch]
		public ProxyDocumentType Type { get; set; }
	}
}