using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.StoredResources
{
	[Appellative(Gender = GrammaticalGender.Masculine,
	NominativePlural = "Изображения",
	Nominative = "Изображение"
	)]
	public class StoredImageResource : StoredResource
	{
	
		[Display(Name = "Изображение")]
		public virtual Image Image {
			get { return GetImageFromBinary(); }
			set { SetBinaryFromImage(value);}
		}

		public StoredImageResource()
		{
			this.Type = ResoureceType.Image;
		}

		private Image GetImageFromBinary()
		{
			Image resImage; 
			if(BinaryFile == null) {
				return null;
			}

			using(var ms = new MemoryStream(BinaryFile)) {
				resImage = Image.FromStream(ms);
			}
			return resImage;
		}

		private void SetBinaryFromImage(Image image)
		{
			if(image == null) {
				return;
			}
			using(var ms = new MemoryStream()) {
				Image.Save(ms, Image.RawFormat);
				BinaryFile = ms.ToArray();
			}
		}
	}
}
