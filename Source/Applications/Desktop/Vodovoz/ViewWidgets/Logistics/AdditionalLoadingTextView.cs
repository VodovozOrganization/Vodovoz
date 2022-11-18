using System;
using System.Linq.Expressions;
using Gamma.Binding.Core;
using Gtk;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewWidgets.Logistics
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AdditionalLoadingTextView : Bin
	{
		private AdditionalLoadingDocument _additionalLoadingDocument;
		private TextTag _defaultTag;
		private TextTag _boldTag;

		public AdditionalLoadingTextView()
		{
			Build();
			Binding = new BindingControler<AdditionalLoadingTextView>(this, new Expression<Func<AdditionalLoadingTextView, object>>[]
			{
				w => w.AdditionalLoadingDocument
			});
			InitializeBuffer();
		}

		public BindingControler<AdditionalLoadingTextView> Binding { get; }

		public AdditionalLoadingDocument AdditionalLoadingDocument
		{
			get => _additionalLoadingDocument;
			set
			{
				if(_additionalLoadingDocument == value)
				{
					return;
				}
				UnsubscribeFromChanges();
				_additionalLoadingDocument = value;
				SubscribeToChanges();
				Update();
			}
		}

		public void Update()
		{
			if(_additionalLoadingDocument == null)
			{
				return;
			}

			var buffer = ytextview.Buffer;
			buffer.Clear();
			var iter = buffer.EndIter;
			buffer.InsertWithTags(ref iter, "Запас: ", _boldTag);

			int lastIndex = AdditionalLoadingDocument.ObservableItems.Count - 1;
			for(var i = 0; i < AdditionalLoadingDocument.ObservableItems.Count; i++)
			{
				var item = AdditionalLoadingDocument.ObservableItems[i];
				iter = buffer.EndIter;
				buffer.InsertWithTags(ref iter, item.Nomenclature.ShortName + ": ", _defaultTag);
				iter = buffer.EndIter;
				buffer.InsertWithTags(ref iter, item.Amount.ToString("N0"), _boldTag);
				iter = buffer.EndIter;
				buffer.InsertWithTags(ref iter, " " + item.Nomenclature.Unit.Name + (i == lastIndex ? "" : ", "), _defaultTag);
			}
		}

		private void InitializeBuffer()
		{
			var textTags = new TextTagTable();
			_boldTag = new TextTag("Bold");
			_boldTag.Weight = Pango.Weight.Bold;
			textTags.Add(_boldTag);

			_defaultTag = new TextTag("Default");
			textTags.Add(_defaultTag);

			ytextview.Buffer = new TextBuffer(textTags);
			ytextview.Editable = false;
		}

		private void SubscribeToChanges()
		{
			if(_additionalLoadingDocument == null)
			{
				return;
			}
			_additionalLoadingDocument.ObservableItems.ElementAdded += OnElementAdded;
			_additionalLoadingDocument.ObservableItems.ElementRemoved += OnElementRemoved;
			_additionalLoadingDocument.ObservableItems.ElementChanged += OnElementChanged;
		}

		private void UnsubscribeFromChanges()
		{
			if(_additionalLoadingDocument == null)
			{
				return;
			}
			_additionalLoadingDocument.ObservableItems.ElementAdded -= OnElementAdded;
			_additionalLoadingDocument.ObservableItems.ElementRemoved -= OnElementRemoved;
			_additionalLoadingDocument.ObservableItems.ElementChanged -= OnElementChanged;
		}

		private void OnElementAdded(object alist, int[] aidx)
		{
			Update();
		}

		private void OnElementRemoved(object alist, int[] aidx, object aobject)
		{
			Update();
		}

		private void OnElementChanged(object alist, int[] aidx)
		{
			Update();
		}
	}
}
