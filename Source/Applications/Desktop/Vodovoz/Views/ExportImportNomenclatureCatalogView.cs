using System;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels;

namespace Vodovoz.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExportImportNomenclatureCatalogView : TabViewBase<ExportImportNomenclatureCatalogViewModel>
	{
		public ExportImportNomenclatureCatalogView(ExportImportNomenclatureCatalogViewModel ViewModel) : base(ViewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			ybuttonExport.Clicked += (sender, e) => {
				var parentWindow = GetParentWindow(this);
				var folderChooser = new FileChooserDialog("Выберите папку выгрузки", parentWindow, FileChooserAction.SelectFolder,
					Stock.Cancel, ResponseType.Cancel, Stock.Open, ResponseType.Accept)
				{
					DoOverwriteConfirmation = true,
				};
				folderChooser.ShowAll();
				if((ResponseType)folderChooser.Run() == ResponseType.Accept) {
					if(folderChooser.CurrentFolder == null) {
						folderChooser.Destroy();
						return;
					}
					ViewModel.FolderPath = folderChooser.CurrentFolder;
					var shortpath = folderChooser.CurrentFolder;
					if(folderChooser.CurrentFolder.Length > 25)
						shortpath = "..." + shortpath.Substring(shortpath.Length - 25);
					ybuttonExport.Label = shortpath;
					folderChooser.Destroy();
				}
				else
					folderChooser.Destroy();
			};

			enummenubuttonExport.ItemsEnum = typeof(ExportType);
			enummenubuttonExport.EnumItemClicked += (sender, e) => {
				ViewModel.ExportCommand.Execute(e.ItemEnum);
			};

			enummenubuttonLoadActions.ItemsEnum = typeof(LoadAction);
			enummenubuttonLoadActions.EnumItemClicked += (sender, e) => {
				ViewModel.LoadActionCommand.Execute(e.ItemEnum);
			};

			enummenubuttonConfirmUpdate.ItemsEnum = typeof(ConfirmUpdateAction);
			enummenubuttonConfirmUpdate.EnumItemClicked += (sender, e) => {
				ViewModel.ConfirmUpdateDataCommand.Execute(e.ItemEnum);
			};

			ycheckDontChangeSellPrice.Binding.AddBinding(ViewModel, vm => vm.DontChangeSellPrice, w => w.Active);
			ycheckDontChangeSellPrice.Active = true;
			ycheckDontChangeSellPrice.TooltipText = "При включении у всех заменяемых номенклатур будут удалены все старые цены и будет создана одна новая цена, указанная в файле";

			ybuttonConfirmLoadNew.Clicked += (sender, e) => {ViewModel.ConfirmLoadNewCommand.Execute();};

			yfilechooserbuttonImport.Binding.AddBinding(ViewModel, vm => vm.FilePath, w => w.Filename);
			var fileFilter = new FileFilter();
			fileFilter.AddPattern("*.csv");
			yfilechooserbuttonImport.Filter = fileFilter;
			yfilechooserbuttonImport.Title = "Выберите csv файл";

			ytreeviewNomenclatures.ColumnsConfig = FluentColumnsConfig<NomenclatureCatalogNode>.Create()
				.AddColumn("Действие")
					.MinWidth(120)
					.AddComboRenderer(x => x.ConflictSolveAction)
					.SetDisplayFunc(x => x.GetEnumTitle())
					.FillItems(((ConflictSolveAction[])Enum.GetValues(typeof(ConflictSolveAction))).ToList())
					.AddSetter((c, n) => {
						c.Editable = n.Source == Source.File && n.Status == NodeStatus.Conflict && n.DuplicateOf != null && ViewModel.CurrentState == LoadAction.LoadNew;
						switch(ViewModel.CurrentState) {
							case LoadAction.LoadNew:
								if(n.DuplicateOf == null || n.Source == Source.Base || !c.Editable)
									c.Text = "";
								break;
							case LoadAction.UpdateData:
								if(n.ConflictSolveAction != ConflictSolveAction.Ignore)
									c.Text = "";
								break;
						}
					})
				.AddColumn("Источник")
					.AddEnumRenderer((x) => x.Source)
					.XAlign(0.5f)
				.AddColumn("Статус")
					.AddTextRenderer((x) => x.Status.GetEnumTitle())
					.XAlign(0.5f)
				.AddColumn("ID номенклатуры")
					.AddTextRenderer(x => x.Id.ToString())
					.AddSetter((c, n) => { if(n.Id == null) c.Text = "Новый"; })
					.XAlign(0.5f)
				.AddColumn("Наименование")
					.AddTextRenderer(x => x.Name)
					.WrapMode(Pango.WrapMode.WordChar)
					.WrapWidth(400)
				.AddColumn("ID группы товаров")
					.AddNumericRenderer(x => x.GroupId)
					.XAlign(0.5f)
				.AddColumn("ID поставщика")
					.AddNumericRenderer(x => x.ShipperCounterpartyId)
					.XAlign(0.5f)
				.AddColumn("Цена продажи")
					.AddNumericRenderer(x => x.SellPrice).Digits(2).WidthChars(10)
					.XAlign(1)
				.AddColumn("Единицы измерения")
					.AddNumericRenderer(x => x.MeasurementUnit)
					.XAlign(0.5f)
				.AddColumn("Папка 1С")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.Folder1cName)
					.XAlign(0.5f)
				.AddColumn("Категория")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.NomenclatureCategory)
					.XAlign(0.5f)
				.AddColumn("Объем тары")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.TareVolume)
					.XAlign(0.5f)
				.AddColumn("Вид оборудования")
					.AddTextRenderer(x => x.EquipmentKindName)
					.XAlign(0.5f)
				.AddColumn("Доступность для продажи")
					.AddTextRenderer(x => x.SaleCategory)
					.XAlign(0.5f)
				.AddColumn("Тип залога")
					.AddTextRenderer(x => x.TypeOfDepositCategory)
					.XAlign(0.5f)
				.AddColumn("Тип топлива")
					.AddTextRenderer(x => x.FuelTypeName)
					.XAlign(0.5f)
				.RowCells()
				.AddSetter((CellRendererText c, NomenclatureCatalogNode n) => {
						c.CellBackgroundGdk = GetGdkColor(n.BackgroundColor);
						c.ForegroundGdk = GetGdkColor(n.ForegroundColor);
				})
			.Finish();

			ViewModel.PropertyChanged += (sender, e) => {
				Gtk.Application.Invoke((s, args) => {
					if(e.PropertyName == nameof(ViewModel.ProgressBarValue))
						progressbar.Adjustment.Value = ViewModel.ProgressBarValue;
					if(e.PropertyName == nameof(ViewModel.ProgressBarUpper))
						progressbar.Adjustment.Upper = ViewModel.ProgressBarUpper;
					if(e.PropertyName == nameof(ViewModel.Items))
						ytreeviewNomenclatures.ItemsDataSource = ViewModel.Items;
					if(e.PropertyName == nameof(ViewModel.IsConfirmUpdateDataButtonVisible))
						enummenubuttonConfirmUpdate.Visible = ViewModel.IsConfirmUpdateDataButtonVisible;
					if(e.PropertyName == nameof(ViewModel.IsConfirmLoadNewButtonVisible))
						ycheckDontChangeSellPrice.Visible = ViewModel.IsConfirmUpdateDataButtonVisible;
					if(e.PropertyName == nameof(ViewModel.IsConfirmLoadNewButtonVisible))
						ybuttonConfirmLoadNew.Visible = ViewModel.IsConfirmLoadNewButtonVisible;
				});
			};

			TextTagTable textTags = new TextTagTable();
			var darkredtag = new TextTag("DarkRed");
			darkredtag.ForegroundGdk = GetGdkColor(ConsoleColor.DarkRed);
			textTags.Add(darkredtag);
			var redtag = new TextTag("red");
			redtag.ForegroundGdk = GetGdkColor(ConsoleColor.Red);
			textTags.Add(redtag);
			var greentag = new TextTag("Green");
			greentag.ForegroundGdk = GetGdkColor(ConsoleColor.Green);
			textTags.Add(greentag);
			var darkgreentag = new TextTag("DrakGreen");
			darkgreentag.ForegroundGdk = GetGdkColor(ConsoleColor.DarkGreen);
			textTags.Add(darkgreentag);
			var blackTag = new TextTag("Black");
			blackTag.ForegroundGdk = GetGdkColor(ConsoleColor.Black);
			textTags.Add(blackTag);
			var yellowTag = new TextTag("Yellow");
			yellowTag.ForegroundGdk = GetGdkColor(ConsoleColor.DarkYellow);
			textTags.Add(yellowTag);

			ViewModel.ProgressBarMessagesUpdated += (aList, aIdx) => {
				Gtk.Application.Invoke((s, args) => {
					TextBuffer tempBuffer = new TextBuffer(textTags);
					foreach(ColoredMessage message in ViewModel.ProgressBarMessages) {
						TextIter iter = tempBuffer.EndIter;
						switch(message.Color) {
							case ConsoleColor.Black: tempBuffer.InsertWithTags(ref iter, "\n" + message.Text, blackTag); break;
							case ConsoleColor.DarkRed: tempBuffer.InsertWithTags(ref iter, "\n" + message.Text, darkredtag); break;
							case ConsoleColor.Green: tempBuffer.InsertWithTags(ref iter, "\n" + message.Text, greentag); break;
							case ConsoleColor.Yellow: tempBuffer.InsertWithTags(ref iter, "\n" + message.Text, yellowTag); break;
							case ConsoleColor.DarkGreen: tempBuffer.InsertWithTags(ref iter, "\n" + message.Text, darkgreentag); break;
							case ConsoleColor.Red: tempBuffer.InsertWithTags(ref iter, "\n" + message.Text, redtag); break;
							default: throw new NotImplementedException("Цвет не настроен");
						}
					}
					ytextviewProgressStatus.Buffer = tempBuffer;
					ScrollToEnd();
				});
			};

			ytreeviewNomenclatures.Selection.Changed += (sender, e) => {
				Gtk.Application.Invoke((s, args) => {
					ytextviewNodeMessages.Buffer.Clear();
					TextBuffer tempBuffer = new TextBuffer(textTags);
					var node = ytreeviewNomenclatures.GetSelectedObject<NomenclatureCatalogNode>();
					if(node == null) {
						ytextviewNodeMessages.Buffer.Text = "Выберите запись для просмотра ошибок";
						return;
					}
					foreach(ColoredMessage message in node.ErrorMessages) {
						TextIter iter = tempBuffer.EndIter;
						switch(message.Color) {
							case ConsoleColor.Black: tempBuffer.InsertWithTags(ref iter, message.Text + "\n", blackTag); break;
							case ConsoleColor.DarkRed: tempBuffer.InsertWithTags(ref iter, message.Text + "\n", darkredtag); break;
							case ConsoleColor.Green: tempBuffer.InsertWithTags(ref iter, message.Text + "\n", greentag); break;
							case ConsoleColor.Yellow: tempBuffer.InsertWithTags(ref iter, message.Text + "\n", yellowTag); break;
							case ConsoleColor.DarkGreen: tempBuffer.InsertWithTags(ref iter, message.Text + "\n", darkgreentag); break;
							case ConsoleColor.Red: tempBuffer.InsertWithTags(ref iter, message.Text + "\n", redtag); break;
							default: throw new NotImplementedException("Цвет не настроен");
						}
					}
					if(!node.ErrorMessages.Any() && node.Source == Source.File) {
						TextIter iter = tempBuffer.EndIter;
						tempBuffer.InsertWithTags(ref iter, "Ошибок нет\n", darkgreentag);
					}
					if(node.Source == Source.Base) {
						TextIter iter = tempBuffer.EndIter;
						tempBuffer.InsertWithTags(ref iter, "Данные из базы\n", blackTag);
					}
					ytextviewNodeMessages.Buffer = tempBuffer;
					ScrollToEnd();
				});
			};
		}

		#region Helpers

		private Window GetParentWindow(Widget widget)
		{
			if(!(widget is Window w))
				w = GetParentWindow(widget.Parent);
			return w;
		}

		private void ScrollToEnd()
		{
			TextIter ti = ytextviewProgressStatus.Buffer.GetIterAtLine(ytextviewProgressStatus.Buffer.LineCount - 1);
			TextMark tm = ytextviewProgressStatus.Buffer.CreateMark("eot", ti, false);
			ytextviewProgressStatus.ScrollToMark(tm, 0, false, 0, 0);
		}

		private Gdk.Color GetGdkColor(ConsoleColor color)
		{
			switch(color) {
				case ConsoleColor.Black:
					return GdkColors.PrimaryText;
				case ConsoleColor.White:
					return GdkColors.PrimaryBase;
				case ConsoleColor.Gray:
					return GdkColors.InsensitiveBase;
				case ConsoleColor.Red: 
					return GdkColors.DangerBase;
				case ConsoleColor.DarkRed:
					return GdkColors.DarkRed;
				case ConsoleColor.Green:
					return GdkColors.SuccessBase;
				case ConsoleColor.DarkGreen:
					return GdkColors.DarkGreen;
				case ConsoleColor.DarkYellow:
					return GdkColors.WarningBase;
				default:
					return GetGdkColor(ConsoleColor.Black);
			}
		}

		#endregion
	}
}
