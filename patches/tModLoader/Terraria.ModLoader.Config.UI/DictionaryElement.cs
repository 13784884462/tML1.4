﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Graphics;

namespace Terraria.ModLoader.Config.UI
{
	internal abstract class IDictionaryElementWrapper
	{
		//public UIModConfigDictionaryElementWrapper() { }
		internal virtual object Key => null;
		internal virtual object Value => null;
		//internal UIModConfigDictionaryItem parent;
		//public UIModConfigDictionaryElementWrapper(object key, object value, UIModConfigDictionaryItem parent)
		//{
		//	this.key = key;
		//	this.value = value;
		//	this.parent = parent;
		//}
	}

	internal class DictionaryElementWrapper<K, V> : IDictionaryElementWrapper
	{
		private IDictionary dictionary;
		//private object _key;

		//internal override object Key
		//{
		//	get { return _key; }
		//	set {
		//		dictionary.Remove(key);
		//		_key = value as K;
		//	}
		//}

		private K _key;
		public K key
		{
			get { return _key; }
			set
			{
				if (dictionary.Contains(value))
				{

				}
				else
				{
					dictionary.Remove(_key);
					_key = value;
					dictionary.Add(_key, _value);
				}
			}
		}

		private V _value;
		public V value
		{
			get { return _value; }
			set
			{
				dictionary[key] = value;
				_value = value;
			}
		}



		//public K key;
		//public V value;
		internal override object Key => key;
		internal override object Value => value;
		//internal UIModConfigDictionaryItem parent;
		public DictionaryElementWrapper(K key, V value, IDictionary dictionary) //, UIModConfigDictionaryItem parent)
		{
			this.dictionary = dictionary;
			this._key = key;
			this._value = value;
			//this.parent = parent;
		}
	}

	//internal class UIModConfigDictionaryElementWrapperWrapper
	//{
	//	public dynamic w;
	//	public UIModConfigDictionaryElementWrapperWrapper(dynamic w)
	//	{
	//		this.w = w;
	//	}
	//}

	internal class DictionaryElement : ConfigElement
	{
		private object data;
		private List<object> dataAsList;
		internal Type keyType;
		internal Type valueType;
		//private int sliderIDStart;
		private NestedUIList dataList;
		internal UIText save;
		public List<IDictionaryElementWrapper> dataWrapperList;

		float scale = 1f;

		public DictionaryElement(PropertyFieldWrapper memberInfo, object item) : base(memberInfo, item, null)
		{
			drawLabel = false;

			//sliderIDStart = sliderIDInPage;
			//sliderIDInPage += 10000;

			string name = memberInfo.Name;
			if (labelAttribute != null)
			{
				name = labelAttribute.Label;
			}

			UISortableElement sortedContainer = new UISortableElement(-1);
			sortedContainer.Width.Set(0f, 1f);
			sortedContainer.Height.Set(30f, 0f);
			sortedContainer.HAlign = 0.5f;
			var text = new UIText(name);
			text.Top.Pixels += 6;
			text.Left.Pixels += 4;
			sortedContainer.Append(text);
			Append(sortedContainer);

			UIPanel panel = new UIPanel();
			panel.Width.Set(-20f, 1f);
			panel.Left.Set(20f, 0f);
			panel.Top.Set(30f, 0f);
			panel.Height.Set(-60, 1f);
			Append(panel);

			dataList = new NestedUIList();
			dataList.Width.Set(-20, 1f);
			dataList.Left.Set(0, 0f);
			dataList.Height.Set(0, 1f);
			dataList.ListPadding = 5f;
			panel.Append(dataList);

			UIScrollbar scrollbar = new UIScrollbar();
			scrollbar.SetView(100f, 1000f);
			scrollbar.Height.Set(0f, 1f);
			scrollbar.Top.Set(0f, 0f);
			scrollbar.Left.Pixels += 8;
			scrollbar.HAlign = 1f;
			dataList.SetScrollbar(scrollbar);
			panel.Append(scrollbar);

			keyType = memberInfo.Type.GetGenericArguments()[0];
			valueType = memberInfo.Type.GetGenericArguments()[1];

			data = memberInfo.GetValue(item);
			SetupList();

			sortedContainer = new UISortableElement(int.MaxValue);
			sortedContainer.Width.Set(0f, 1f);
			sortedContainer.Height.Set(30f, 0f);
			sortedContainer.Top.Set(-30f, 1f);
			sortedContainer.HAlign = 0.5f;
			text = new UIText(data == null ? "Click To Initialize" : "Click To Add");
			text.Top.Pixels += 6;
			text.Left.Pixels += 4;
			text.OnClick += (a, b) =>
			{
				Main.PlaySound(21);

				if (data == null) {
					data = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
					memberInfo.SetValue(item, data);
					text.SetText("Click To Add");
				}
				else {

					//DefaultListValueAttribute defaultListValueAttribute = (DefaultListValueAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(DefaultListValueAttribute));
					//if (defaultListValueAttribute != null)
					//{
					//	((IList)data).Add(defaultListValueAttribute.defaultValue);
					//}
					//else
					//{
					//((IList)data).Add(Activator.CreateInstance(listType));
					//}

					try {
						((IDictionary)data).Add(ConfigManager.AlternateCreateInstance(keyType), ConfigManager.AlternateCreateInstance(valueType));
					}
					catch (Exception e) {
						Interface.modConfig.SetMessage("Error: " + e.Message, Color.Red);
					}
				}

				SetupList();
				Interface.modConfig.SetPendingChanges();
			};
			sortedContainer.Append(text);


			//save = new UIText("Save");
			//save.Top.Pixels += 6;
			//save.Left.Pixels += 144;
			//save.OnClick += (a, b) =>
			//{
			//	Main.PlaySound(21);
			//	try
			//	{
			//		//((IDictionary)data).Add(Activator.CreateInstance(keyType), Activator.CreateInstance(valueType));
			//		((IDictionary)data).Clear();
			//		foreach (var dataElement in dataWrapperList)
			//		{
			//			((IDictionary)data).Add(dataElement.Key, dataElement.Value);
			//		}
			//	}
			//	catch (Exception e)
			//	{
			//		Interface.modConfig.SetMessage("Save Error: " + e.Message, Color.Red);
			//	}

			//	SetupList();
			//	Interface.modConfig.SetPendingChanges();
			//};
			//sortedContainer.Append(save);

			Append(sortedContainer);

			UIImageButton upButton = new UIImageButton(Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonIncrement.png")));
			upButton.Top.Set(40, 0f);
			upButton.Left.Set(0, 0f);
			upButton.OnClick += (a, b) =>
			{
				scale = Math.Min(2f, scale + 0.5f);
				dataList.RecalculateChildren();
				float h = dataList.GetTotalHeight();
				MinHeight.Set(Math.Min(Math.Max(h + 84, 100), 300) * scale, 0f);
				Recalculate();
				if (Parent != null && Parent is UISortableElement)
				{
					Parent.Height.Pixels = GetOuterDimensions().Height;
				}
			};
			Append(upButton);

			UIImageButton downButton = new UIImageButton(Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.Config.UI.ButtonDecrement.png")));
			downButton.Top.Set(52, 0f);
			downButton.Left.Set(0, 0f);
			downButton.OnClick += (a, b) =>
			{
				scale = Math.Max(1f, scale - 0.5f);
				dataList.RecalculateChildren();
				float h = dataList.GetTotalHeight();
				MinHeight.Set(Math.Min(Math.Max(h + 84, 100), 300) * scale, 0f);
				Recalculate();
				if (Parent != null && Parent is UISortableElement)
				{
					Parent.Height.Pixels = GetOuterDimensions().Height;
				}
			};
			Append(downButton);
		}

		private void SetupList()
		{
			//int sliderID = sliderIDStart;
			dataList.Clear();
			var deleteButtonTexture = TextureManager.Load("Images/UI/ButtonDelete");
			int top = 0;
			dataWrapperList = new List<IDictionaryElementWrapper>();

			//var genericListType = typeof(List<>);
			//var specificListType = genericListType.MakeGenericType(typeof(double));
			//var list = Activator.CreateInstance(specificListType);

			//var listType = typeof(List<>);
			//var constructedListType = listType.MakeGenericType(keyType);
			//var keyList = (IList)Activator.CreateInstance(constructedListType);

			//foreach (var item in ((IDictionary)data))
			//{
			//	//var wrapped = UIModConfig.WrapIt(dataList, ref top, memberInfo, item, ref sliderID, keys, keyType, i2);
			//	keyList.Add(item);
			//	//i2++;
			//}
			//((IDictionary)data).

			//string elementTypeName = Console.ReadLine();
			//Type elementType = Type.GetType(elementTypeName);
			//Type[] types = new Type[] { elementType };

			//Type listType = memberInfo.Type.GetGenericArguments()[0];
			//Type listType = typeof(Dictionary<>);
			//Type genericType = listType.MakeGenericType(types);
			//IProxy proxy = (IProxy)Activator.CreateInstance(genericType);

			//Type genericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
			Type genericType = typeof(DictionaryElementWrapper<,>).MakeGenericType(keyType, valueType);

			if (data != null) {
				var keys = ((IDictionary)data).Keys;
				var values = ((IDictionary)data).Values;
				var keysEnumerator = keys.GetEnumerator();
				var valuesEnumerator = values.GetEnumerator();
				int i = 0;
				while (keysEnumerator.MoveNext()) {

					valuesEnumerator.MoveNext();
					//var wrapper = new UIModConfigDictionaryElementWrapper<typeof(keysEnumerator.Current), typeof(keysEnumerator.Current)>(keysEnumerator.Current, valuesEnumerator.Current, this);
					//dynamic sampleObject = new ExpandoObject();
					//sampleObject.key = keysEnumerator.Current;
					//sampleObject.value = valuesEnumerator.Current;
					//var wrapperwrapper = new UIModConfigDictionaryElementWrapperWrapper(sampleObject);

					IDictionaryElementWrapper proxy = (IDictionaryElementWrapper)Activator.CreateInstance(genericType,
						new object[] { keysEnumerator.Current, valuesEnumerator.Current, (IDictionary)data });
					dataWrapperList.Add(proxy);
					//var v = new { Key = keysEnumerator.Current, Value = valuesEnumerator.Current };  

					//dataWrapperList.Add(wrapper);
					//}

					//var wrapperwrapper = new UIModConfigDictionaryElementWrapperWrapper(v);

					//	var keys = ((IDictionary)data).Keys.ToList();
					//var values = ((IDictionary)data).Values.ToList();
					//for (int i = 0; i < ((IDictionary)data).Count; i++)
					//{
					//((IDictionary)data).
					//	int index = i;
					//((IDictionary)data).
					//Type tupleType = typeof(Tuple<,>);
					//	var wrapper = new UIModConfigDictionaryElementWrapper(((IDictionary)data)[], , this);
					Type itemType = memberInfo.Type.GetGenericArguments()[0];
					var wrappermemberInfo = ConfigManager.GetFieldsAndProperties(this).ToList()[0];
					int index = i;
					var wrapped = UIModConfig.WrapIt(dataList, ref top, wrappermemberInfo, this, 0, dataWrapperList, genericType, i); // TODO: Sometime key is below value for some reason. IntFloatDictionary.
					//var wrapped = UIModConfig.WrapIt(dataList, ref top, wrappermemberInfo, wrapperwrapper, ref sliderID);
					// save wrap, pre save check?
					wrapped.Item2.Left.Pixels += 24;
					wrapped.Item2.Width.Pixels -= 24;

					// Add delete button.
					UIImageButton deleteButton = new UIImageButton(deleteButtonTexture);
					deleteButton.VAlign = 0.5f;

					// fix delete.
					object o = keysEnumerator.Current;
					deleteButton.OnClick += (a, b) => {
						((IDictionary)data).Remove(o);
						SetupList();
						Interface.modConfig.SetPendingChanges();
					};
					wrapped.Item1.Append(deleteButton);

					i++;
				}
			}
			dataList.RecalculateChildren();
			float h = dataList.GetTotalHeight();
			MinHeight.Set(Math.Min(Math.Max(h + 84, 100), 300) * scale, 0f);
			this.Recalculate();
			if (Parent != null && Parent is UISortableElement)
			{
				Parent.Height.Pixels = GetOuterDimensions().Height;
			}
		}

		public override void Recalculate()
		{
			base.Recalculate();
			float h = dataList.GetTotalHeight() * scale;
			Height.Set(h, 0f);
		}
	}
}
