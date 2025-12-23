using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QSOrmProject.RepresentationModel;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Infrastructure;

namespace Vodovoz.ViewModel
{
	public class ProxyDocumentsVM : RepresentationModelEntityBase<ProxyDocument, ProxyDocumentsVMNode>
	{
		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			ProxyDocumentsVMNode resultAlias = null;
			ProxyDocument proxyDocumentAlias = null;
			
			var userMetadata = UoW.Session.SessionFactory.GetClassMetadata(typeof(ProxyDocument)) as NHibernate.Persister.Entity.AbstractEntityPersister;
			var columnName = userMetadata.DiscriminatorColumnName;

			var proxyDocumentList = UoW.Session.QueryOver<ProxyDocument>(() => proxyDocumentAlias)
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Date).WithAlias(() => resultAlias.Date)
					.Select(x => x.ExpirationDate).WithAlias(() => resultAlias.ExpirationDate)
					.Select(
						Projections.SqlFunction(
							new SQLFunctionTemplate(
								NHibernateUtil.String, $"{columnName}"
							), NHibernateUtil.String
						)
					).WithAlias(() => resultAlias.StringType)
				
				).OrderBy(d => d.Id).Desc
				.TransformUsing(Transformers.AliasToBean<ProxyDocumentsVMNode>())
				.List<ProxyDocumentsVMNode>();

			//Переделываем string в enum
			var proxyTypes = new Dictionary<string, ProxyDocumentType>();
			foreach (ProxyDocumentType proxyType in Enum.GetValues(typeof(ProxyDocumentType)))
			{
				proxyTypes.Add(proxyType.ToString(), proxyType);
			}
			foreach (var proxyDocument in proxyDocumentList)
			{
				proxyDocument.Type = proxyTypes[proxyDocument.StringType];
			}

			SetItemsSource(proxyDocumentList);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ProxyDocumentsVMNode>.Create()
			.AddColumn("Тип и номер")
				.AddTextRenderer(node => $"{node.Type.GetEnumTitle()} №{node.Id} от {node.Date:d}")
			.AddColumn("Начало действия")
				.AddTextRenderer(node => $"{node.Date:d}")
			.AddColumn("Окончание действия")
				.AddTextRenderer(node => $"{node.ExpirationDate:d}")
			.RowCells()
				.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = (DateTime.Today > n.ExpirationDate) ? GdkColors.InsensitiveText : GdkColors.PrimaryText)
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
		public string StringType { get; set; }
	}
}
