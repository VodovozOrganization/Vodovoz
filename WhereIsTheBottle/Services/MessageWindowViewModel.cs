using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using QS.Dialog;
using QS.ViewModels;

namespace WhereIsTheBottle.Services
{
	public class MessageWindowViewModel : ViewModelBase
	{
		private readonly Dictionary<ImportanceLevel, BitmapImage> images;

		private readonly int minHeight;
		private readonly int minWidth;

		private int height;

		private BitmapImage image;

		private ImportanceLevel importanceLevel;

		private string message;

		private string title;

		private int width;

		public MessageWindowViewModel()
		{
			minHeight = 170;
			minWidth = 300;
			title = "";

			images = new Dictionary<ImportanceLevel, BitmapImage>
			{
				{ ImportanceLevel.Info, new BitmapImage(new Uri("/Resources/Images/Icons/48px/Info.png", UriKind.Relative)) },
				{ ImportanceLevel.Warning, new BitmapImage(new Uri("/Resources/Images/Icons/48px/Warning.png", UriKind.Relative)) },
				{ ImportanceLevel.Error, new BitmapImage(new Uri("/Resources/Images/Icons/48px/Error.png", UriKind.Relative)) }
			};
		}

		public ImportanceLevel ImportanceLevel
		{
			get => importanceLevel;
			set
			{
				if(SetField(ref importanceLevel, value))
				{
					OnPropertyChanged(nameof(Image));
				}
			}
		}
		public int Height
		{
			get => height < minHeight ? minHeight : height;
			set => SetField(ref height, value);
		}
		public int Width
		{
			get => width < minWidth ? minWidth : width;
			set => SetField(ref width, value);
		}
		public string Title
		{
			get => title;
			set => SetField(ref title, value);
		}
		public string Message
		{
			get => message;
			set => SetField(ref message, value);
		}
		public BitmapImage Image
		{
			get => image ?? images[ImportanceLevel];
			set => SetField(ref image, value);
		}

		public void FillWindow(ImportanceLevel level, string messageString, string titleString = null)
		{
			ImportanceLevel = level;
			Message = messageString;
			Title = titleString ?? "";
		}
	}
}