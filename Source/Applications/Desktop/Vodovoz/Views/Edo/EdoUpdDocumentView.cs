using Gamma.Binding;
using Gamma.Binding.Core.RecursiveTreeConfig;
using Gamma.ColumnConfig;
using QS.Views.Dialog;
using Vodovoz.ViewModels.Edo;

namespace Vodovoz.Views.Edo
{
	/// <summary>
	/// Таблица значений сохранённого документа УПД.
	/// </summary>
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoUpdDocumentView : DialogViewBase<EdoUpdDocumentViewModel>
	{
		/// <summary>Создаёт представление УПД.</summary>
		/// <param name="viewModel">Модель просмотра документа.</param>
		public EdoUpdDocumentView(EdoUpdDocumentViewModel viewModel) : base(viewModel)
		{
			Build();
			ytreeviewFields.ColumnsConfig = FluentColumnsConfig<UpdCompareViewModel>.Create()
				.AddColumn("Поле").MinWidth(250).AddTextRenderer(x => x.Name)
				.AddColumn("Значение").AddTextRenderer(x => x.SentValue)
					.WrapMode(Pango.WrapMode.WordChar).WrapWidth(700)
				.AddColumn("").Finish();
			ytreeviewFields.EnableTreeLines = true;
			ytreeviewFields.EnableGridLines = Gtk.TreeViewGridLines.Both;
			var config = new RecursiveConfig<UpdCompareViewModel>(x => x.ParentNode, x => x.ChildNodes);
			ytreeviewFields.YTreeModel = new RecursiveTreeModel<UpdCompareViewModel>(ViewModel.Fields, config);
			ytreeviewFields.ExpandAll();
		}
	}
}
