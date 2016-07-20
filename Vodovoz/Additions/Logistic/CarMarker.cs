using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using GMap.NET;
using GMap.NET.GtkSharp;

namespace Vodovoz.Additions.Logistic
{

   public enum CarMarkerType
   {
      	none = 0,
      	GreenCar,
		BlueCar,
		RedCar
   }
		
   [Serializable]
   public class CarMarker : GMapMarker, ISerializable, IDeserializationCallback
   {
      Bitmap Bitmap;

      public readonly CarMarkerType Type;

      public CarMarker(PointLatLng p, CarMarkerType type)
         : base(p)
      {
         this.Type = type;

         if(type != CarMarkerType.none)
         {
            LoadBitmap();
         }
      }

      void LoadBitmap()
      {
         	Bitmap = GetIcon(Type.ToString());
         	Size = new System.Drawing.Size(Bitmap.Width, Bitmap.Height);

        	Offset = new Point(-Size.Width / 2 + 1, -Size.Height / 2 + 1);
      }

      /// <summary>
      /// marker using manual bitmap, NonSerialized
      /// </summary>
      /// <param name="p"></param>
      /// <param name="Bitmap"></param>
      public CarMarker(PointLatLng p, Bitmap Bitmap)
         : base(p)
      {
         this.Bitmap = Bitmap;
         Size = new System.Drawing.Size(Bitmap.Width, Bitmap.Height);
         Offset = new Point(-Size.Width / 2, -Size.Height);
      }

      static readonly Dictionary<string, Bitmap> iconCache = new Dictionary<string, Bitmap>();

      internal static Bitmap GetIcon(string name)
      {
         Bitmap ret;
         if(!iconCache.TryGetValue(name, out ret))
         {
				string resourceName = String.Format("Vodovoz.icons.map.{0}.png", name);
				ret = new Bitmap(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName));
            	iconCache.Add(name, ret);
         }
         return ret;
      }


      public override void OnRender(Graphics g)
      {
            g.DrawImage(Bitmap, LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
      }

      public override void Dispose()
      {
         if(Bitmap != null)
         {
            if(!iconCache.ContainsValue(Bitmap))
            {
               Bitmap.Dispose();
               Bitmap = null;
            }
         }

         base.Dispose();
      }

		#region ISerializable Members

      void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
      {
         info.AddValue("type", this.Type);
         //info.AddValue("Bearing", this.Bearing);

         base.GetObjectData(info, context);
      }

      protected CarMarker(SerializationInfo info, StreamingContext context)
         : base(info, context)
      {
         this.Type = Extensions.GetStruct<CarMarkerType>(info, "type", CarMarkerType.none);
         //this.Bearing = Extensions.GetStruct<float>(info, "Bearing", null);
      }

      #endregion

      #region IDeserializationCallback Members

      public void OnDeserialization(object sender)
      {
         if(Type != CarMarkerType.none)
         {
            LoadBitmap();
         }
      }

      #endregion
   }
}
