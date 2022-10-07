using System;
using Gamma.ColumnConfig;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Client;

namespace Vodovoz.Representations
{
	public class TagVM : RepresentationModelEntityBase<Tag, Tag>
	{
		#region IRepresentationModel implementation

		public override void UpdateNodes()
		{
			var tagList = UoW.Session.QueryOver<Tag>().List();

			SetItemsSource(tagList);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<Tag>.Create()
			.AddColumn("Название").AddTextRenderer(node => node.Name)
			.AddColumn("Цвет").AddTextRenderer()
			.AddSetter((cell, node) => { cell.Markup = String.Format("<span foreground=\"{0}\">♥</span>", node.ColorText); })
			.Finish();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		#endregion

		#region implemented abstract members of RepresentationModelBase

		protected override bool NeedUpdateFunc(Tag updatedSubject)
		{
			return true;
		}

		#endregion

		public TagVM() : this(UnitOfWorkFactory.CreateWithoutRoot())
		{
		}

		public TagVM(IUnitOfWork uow)
		{
			this.UoW = uow;
		}
	}
}
