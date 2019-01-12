﻿using System;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;

namespace Terraria.ModLoader.Config.UI
{
	internal class ArrayElement : ConfigElement
	{
		private object data;
		private NestedUIList dataList;

		// does not apply?
		//public override int NumberTicks => 0;
		//public override float TickIncrement => 0;

		public ArrayElement(PropertyFieldWrapper memberInfo, object item) : base(memberInfo, item, null)
		{
			MaxHeight.Set(300, 0f);

			drawLabel = false;
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
			panel.Height.Set(-34, 1f);
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

			data = memberInfo.GetValue(item);

			SetupList();
		}

		private void SetupList()
		{
			Type itemType = memberInfo.Type.GetElementType();
			dataList.Clear();
			Array array = memberInfo.GetValue(item) as Array;
			int count = array.Length;
			int top = 0;
			for (int i = 0; i < count; i++)
			{
				int index = i;
				UIModConfig.WrapIt(dataList, ref top, memberInfo, item, 0, data, itemType, index);
			}
			dataList.RecalculateChildren();
			float h = dataList.GetTotalHeight();
			MinHeight.Set(Math.Min(Math.Max(h + 54, 100), 300), 0f);
			this.Recalculate();
			if (Parent != null && Parent is UISortableElement)
			{
				Parent.Height.Pixels = GetOuterDimensions().Height;
			}
		}

		//protected override void DrawSelf(SpriteBatch spriteBatch)
		//{
		////	Rectangle hitbox = GetInnerDimensions().ToRectangle();
		////	Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.Purple * 0.6f);
		//	base.DrawSelf(spriteBatch);
		//}
	}
}
