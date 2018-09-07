using System;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModel
{
	public class ProxyDocumentsVM : RepresentationModelEntityBase<ProxyDocument, ProxyDocument>
	{
		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			var proxiesList = UoW.Session.QueryOver<ProxyDocument>()
								 .OrderBy(d => d.Id).Desc
								 .List<ProxyDocument>();

			SetItemsSource(proxiesList);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ProxyDocument>.Create()
		.AddColumn("Тип и номер").AddTextRenderer(node => String.Format("{0} №{1} от {2:d}", node.Type.GetEnumTitle(), node.Id, node.Date))
		.AddColumn("Начало действия").AddTextRenderer(node => String.Format("{0:d}", node.Date))
		.AddColumn("Окончание действия").AddTextRenderer(node => String.Format("{0:d}", node.ExpirationDate))
		.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = (DateTime.Today > n.ExpirationDate) ? "grey" : "black")
		.Finish();
		
		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(ProxyDocument updatedSubject)
		{
			return true;
		}

		#endregion

		#region Конструкторы

		public ProxyDocumentsVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
			//CreateRepresentationFilter = () => new ProxyDocumentFilter(UoW);
		}

		public ProxyDocumentsVM(IUnitOfWork uow) : base()
		{
			this.UoW = uow;
		}

		#endregion
	}

	public class ProxyDocumentsVMNode
	{
		public int Id { get; set; }

		public DateTime Date { get; set; }

		public DateTime ExpirationDate { get; set; }

		[UseForSearch]
		public string Title => String.Format("{0} №{1} от {2:d}", Type.GetEnumTitle(), Id, Date);

		public string Start => String.Format("{0:d}", Date);

		public string End => String.Format("{0:d}", ExpirationDate);

		public string RowColor => (DateTime.Today > ExpirationDate) ? "grey" : "black";

		public ProxyDocumentType Type { get; set; }

		public String StrType { get; set; }

		public Employee Employee { get; set; }
	}
}