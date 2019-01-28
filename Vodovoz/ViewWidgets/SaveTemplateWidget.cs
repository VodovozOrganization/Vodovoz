using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using QSDocTemplates;
using QSProjectsLib;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SaveTemplateWidget : Gtk.Bin
	{
		public BindingControler<SaveTemplateWidget> Binding { get; private set; }

		private FileWorker worker = new FileWorker();

		public event EventHandler BeforeOpen;

		public bool CanOpenDocument = true;

		public byte[] File {
			get {
				return Template?.ChangedDocFile;
			}
			set {
				if(Template == null)
					return;
				Template.ChangedDocFile = value;
				UpdateSize();
				UpdateState();
				Binding.FireChange(x => x.File);
			}
		}

		public IList<IDocTemplate> AvailableTemplates { get; set; }

		IDocTemplate template;
		public IDocTemplate Template {
			get {
				return template;
			}
			set {
				template = value;
				UpdateState();
				UpdateSize();
				Binding.FireChange(x => x.Template);
			}
		}

		public SaveTemplateWidget()
		{
			this.Build();

			Binding = new BindingControler<SaveTemplateWidget>(this, new Expression<Func<SaveTemplateWidget, object>>[] {
				(w => w.Template),
				(w => w.File),
			});

			worker.FileUpdated += Worker_FileUpdated;
		}


		public void LoadFile()
		{
			if(Template == null || File != null)
				return;
			OdtWorks odt = new OdtWorks(Template.File);
			odt.DocParser = template.DocParser;
			odt.DocParser.UpdateFields();
			odt.UpdateFields();
			if(odt.DocParser.FieldsHasValues) {
				odt.FillValues();
			}
			var file = odt.GetArray();
			File = new byte[file.Length];
			for(int i = 0; i < file.Length; i++)
				File[i] = file[i];
			odt.Close();
		}

		#region Update
		void Worker_FileUpdated(object sender, FileUpdatedEventArgs e)
		{
			Binding.FireChange(x => x.File);
			UpdateState();
			UpdateSize();
		}

		void UpdateState()
		{
			if(Template == null) {
				labelStatus.Markup = "<span foreground=\"red\">Шаблон не определен!</span>";
				buttonEdit.Sensitive = buttonPrint.Sensitive = buttonOpen.Sensitive = false;
			} else if(Template.DocParser == null) {
				labelStatus.Markup = "<span foreground=\"red\">Парсер не задан!</span>";
				buttonEdit.Sensitive = buttonPrint.Sensitive = buttonOpen.Sensitive = false;
			} else if(Template.ChangedDocFile == null) {
				labelStatus.Markup = "<span foreground=\"red\">Документ не сформирован!</span>";
				buttonEdit.Sensitive = buttonPrint.Sensitive = buttonOpen.Sensitive = false;
			} else {
				labelStatus.Markup = "<span foreground=\"green\">Собственный документ</span>";
				buttonEdit.Sensitive = buttonPrint.Sensitive = buttonOpen.Sensitive = true;
			}
		}

		void UpdateSize()
		{
			label1.LabelProp = String.Empty;
			if(File != null)
				label1.LabelProp = StringWorks.BytesToIECUnitsString((uint)File.LongLength);
		}
		#endregion

		#region ButtonEventHandler
		protected void OpenFile(object sender, EventArgs e, bool readOnly, bool print = false)
		{
			BeforeOpen?.Invoke(this, EventArgs.Empty);
			worker.OpenInOffice(Template, readOnly, FileEditMode.Document, IsSavedFile: true);
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e) => OpenFile(sender, e, false, true);

		protected void OnButtonEditClicked(object sender, EventArgs e) => OpenFile(sender, e, false);

		protected void OnButtonOpenClicked(object sender, EventArgs e) => OpenFile(sender, e, true);
		#endregion
	}

}
