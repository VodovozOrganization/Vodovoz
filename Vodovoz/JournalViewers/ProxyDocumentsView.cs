using System;
using Gtk;
using NLog;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.UpdateNotification;
using QS.Tdi;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModel;

namespace Vodovoz.JournalViewers
{
	public partial class ProxyDocumentsView : QS.Dialog.Gtk.TdiTabBase
	{
		static Logger logger = LogManager.GetCurrentClassLogger();

		IUnitOfWork uow;

		public ProxyDocumentsView()
		{
			this.Build();
			this.TabName = "Журнал доверенностей";
			tableDocuments.RepresentationModel = new ProxyDocumentsVM();
			/*реализовать фильтр*/
			/*hboxFilter.Add(tableDocuments.RepresentationModel.RepresentationFilter as Widget);
			(tableDocuments.RepresentationModel.RepresentationFilter as Widget).Show();*/
			tableDocuments.RepresentationModel.UpdateNodes();
			uow = tableDocuments.RepresentationModel.UoW;
			tableDocuments.Selection.Changed += OnSelectionChanged;
			buttonEdit.Sensitive = buttonDelete.Sensitive = false;
			buttonAdd.ItemsEnum = typeof(ProxyDocumentType);

			/*реализовать права пользователей, если надо*/
			/*foreach(ProxyDocumentType proxyType in Enum.GetValues(typeof(ProxyDocumentType))) {
				var allPermissions = CurrentPermissions.Warehouse.AnyEntities();
				if(allPermissions.Any(x => x.GetAttributes<DocumentTypeAttribute>().Any(at => at.Type.Equals(doctype))))
					continue;
				buttonAdd.SetSensitive(proxyType, false);
			}*/
			buttonFilter.Sensitive = false;
		}

		void OnRefObjectUpdated(object sender, OrmObjectUpdatedEventArgs e)
		{
			tableDocuments.RepresentationModel.UpdateNodes();
		}

		void OnSelectionChanged(object sender, EventArgs e)
		{
			buttonEdit.Sensitive = buttonDelete.Sensitive = tableDocuments.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonAddEnumItemClicked(object sender, EnumItemClickedEventArgs e)
		{
			ProxyDocumentType type = (ProxyDocumentType)e.ItemEnum;
			var dlg = OrmMain.CreateObjectDialog(ProxyDocument.GetProxyDocumentClass(type));
			dlg.EntitySaved += Dlg_EntitySaved;
			TabParent.AddSlaveTab(this, dlg);
		}

		protected void OnTableDocumentsRowActivated(object o, RowActivatedArgs args)
		{
			buttonEdit.Click();
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			if(tableDocuments.GetSelectedObjects().GetLength(0) > 0) {
				int id = (tableDocuments.GetSelectedObjects()[0] as ProxyDocument).Id;
				ProxyDocumentType type = (tableDocuments.GetSelectedObjects()[0] as ProxyDocument).Type;
				var dlg = OrmMain.CreateObjectDialog(ProxyDocument.GetProxyDocumentClass(type), id);
				dlg.EntitySaved += Dlg_EntitySaved;
				TabParent.AddSlaveTab(this, dlg);
			}
		}

		void Dlg_EntitySaved(object sender, EntitySavedEventArgs e)
		{
			tableDocuments.RepresentationModel.UpdateNodes();
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			var item = tableDocuments.GetSelectedObject<ProxyDocument>();
			if(OrmMain.DeleteObject(ProxyDocument.GetProxyDocumentClass(item.Type), item.Id))
				tableDocuments.RepresentationModel.UpdateNodes();
		}

		protected void OnButtonFilterToggled(object sender, EventArgs e)
		{
			//hboxFilter.Visible = buttonFilter.Active;
		}

		protected void OnSearchentity1TextChanged(object sender, EventArgs e)
		{
			tableDocuments.SearchHighlightText = searchentity1.Text;
			tableDocuments.RepresentationModel.SearchString = searchentity1.Text;
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			tableDocuments.RepresentationModel.UpdateNodes();
		}
	}
}
