using System;
using Gtk;
using NLog;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;
using Vodovoz.Representations;

namespace Vodovoz.Dialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NuanceDlg : OrmGtkDialogBase<Comments>
	{
		object UoWforeign;

		public NuanceDlg(object uow)
		{
			UoWforeign = uow;
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Comments>();

			ConfigureDlg();
		}

		public NuanceDlg(object uow, int id)
		{
			UoWforeign = uow;
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Comments>(id);

			ConfigureDlg();
		}

		protected void ConfigureDlg()
		{
			referenceAuthor.ItemsQuery = EmployeeRepository.ActiveEmployeeOrderedQuery();
			referenceAuthor.SetObjectDisplayFunc<Employee>(e => e.ShortName);
			referenceAuthor.Binding.AddBinding(Entity, s => s.Author, w => w.Subject).InitializeFromSource();
			referenceAuthor.Sensitive = false;

			representationtreeview1.RepresentationModel = new CommentsTemplatesVM();
			representationtreeview1.Selection.Changed += Representationtreeview1_Selection_Changed;

			enumAncorPoint.ItemsEnum = typeof(CommentsAncorPoint);
			enumAncorPoint.Binding.AddBinding(Entity, s => s.AncorPoint, w => w.SelectedItem).InitializeFromSource();

			checkCommentFixed.Binding.AddBinding(Entity, s => s.IsFixed, w => w.Active).InitializeFromSource();

			ytextCommentFinal.Binding.AddSource(Entity)
					  .AddBinding(x => x.Text, w => w.Buffer.Text)
					  .AddBinding(x => x.CanTextEdit, w => w.Editable).InitializeFromSource();

			var counterpartyFilter = new CounterpartyFilter(UoW);
			counterpartyFilter.SetAndRefilterAtOnce(x => x.RestrictIncludeArhive = false);

			referenceClient.RepresentationModel = new ViewModel.CounterpartyVM(counterpartyFilter);
			referenceClient.Binding.AddBinding(Entity, s => s.Counterparty, w => w.Subject).InitializeFromSource();

			referenceDeliveryPoint.Binding.AddBinding(Entity, s => s.DeliveryPoint, w => w.Subject).InitializeFromSource();

			referenceOrder.SubjectType = typeof(Order);
			referenceOrder.Binding.AddBinding(Entity, s => s.Order, w => w.Subject).InitializeFromSource();

			var uowDeliveryPoint = UoWforeign as DeliveryPoint;
			if(uowDeliveryPoint != null)
				Entity.DeliveryPoint = uowDeliveryPoint;

			var uowCounterparty = UoWforeign as Counterparty;
			if(uowCounterparty != null)
				Entity.Counterparty = uowCounterparty;

			//var uowOrder = UoWforeign as Order;
			//if(uowOrder != null)
			//  Entity.Order = uowOrder;

		}

		private void Representationtreeview1_Selection_Changed(object sender, EventArgs e)
		{
			bool selected = representationtreeview1.Selection.CountSelectedRows() > 0;
			buttonSelectTemplate.Sensitive = selected;
		}

		public override bool Save()
		{

			Entity.Author = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(Entity.Author == null) {
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать , так как некого указывать в качестве автора документа.");
				FailInitialize = true;
			}

			UoWGeneric.Save();
			return true;

		}

		protected void OnButtonCreateTemplateClicked(object sender, EventArgs e)
		{
			TabParent.OpenTab(OrmMain.GenerateDialogHashName<CommentsTemplates>(0),
						   () => new CommentDlg(), this
					   );
		}

		protected void OnSearchentity2TextChanged(object sender, EventArgs e)
		{
			representationtreeview1.SearchHighlightText = searchentity2.Text;
			representationtreeview1.RepresentationModel.SearchString = searchentity2.Text;
		}

		protected void OnButtonSelectTemplateClicked(object sender, EventArgs e)
		{
			var node = representationtreeview1.GetSelectedObject<CommentsTemplatesVMNode>();
			var template = UoW.GetById<CommentsTemplates>(node.Id);
			Entity.Text = template.CommentTemplate;
			Entity.CommentsGroups = template.CommentsTmpGroups;

		}

		protected void OnRepresentationtreeview1RowActivated(object o, RowActivatedArgs args)
		{
			buttonSelectTemplate.Click();
		}

		protected void OnEnumAncorPointChangedByUser(object sender, EventArgs e)
		{
			if(Entity.AncorPoint == CommentsAncorPoint.Counterparty) {
				Entity.DeliveryPoint = null;
				Entity.Order = null;
			}
		}
	}
}
