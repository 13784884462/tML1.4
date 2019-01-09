﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI
{
	// TODO: In-game version of this UI.
	// TODO: Dictionary
	// TODO: Item/NPC UIElements w/special ui.	
	// TODO: Revert individual button.	
	// TODO: Initialize List if null.
	// TODO: DefaultValue for new elements in Lists
	internal class UIModConfig : UIState
	{
		private UIElement uIElement;
		private UITextPanel<string> headerTextPanel;
		private UITextPanel<string> message;
		private UITextPanel<string> previousConfigButton;
		private UITextPanel<string> nextConfigButton;
		private UITextPanel<string> saveConfigButton;
		private UITextPanel<string> backButton;
		private UITextPanel<string> revertConfigButton;
		private UITextPanel<string> restoreDefaultsConfigButton;
		private UIPanel uIPanel;
		private UIList mainConfigList;
		private UIScrollbar uIScrollbar;
		private Stack<UIPanel> configPanelStack = new Stack<UIPanel>();
		private Stack<string> subPageStack = new Stack<string>();
		//private UIList currentConfigList;
		private Mod mod;
		private List<ModConfig> modConfigs;
		// This is from ConfigManager.Configs
		private ModConfig modConfig;
		// the clone we modify. 
		private ModConfig modConfigClone;

		public override void OnInitialize()
		{
			uIElement = new UIElement();
			uIElement.Width.Set(0f, 0.8f);
			uIElement.MaxWidth.Set(600f, 0f);
			uIElement.Top.Set(160f, 0f);
			uIElement.Height.Set(-180f, 1f);
			uIElement.HAlign = 0.5f;

			uIPanel = new UIPanel();
			uIPanel.Width.Set(0f, 1f);
			uIPanel.Height.Set(-110f, 1f);
			uIPanel.BackgroundColor = UICommon.mainPanelBackground;
			uIElement.Append(uIPanel);

			message = new UITextPanel<string>("Notification: ");
			message.Width.Set(-80f, 1f);
			message.Height.Set(20f, 0f);
			message.HAlign = 0.5f;
			message.VAlign = 1f;
			message.Top.Set(-65f, 0f);
			uIElement.Append(message);

			mainConfigList = new UIList();
			mainConfigList.Width.Set(-25f, 1f);
			mainConfigList.Height.Set(0f, 1f);
			//mainConfigList.Top.Set(40f, 0f);
			mainConfigList.ListPadding = 5f;
			uIPanel.Append(mainConfigList);
			configPanelStack.Push(uIPanel);
			//currentConfigList = mainConfigList;

			uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(0f, 1f);
			uIScrollbar.HAlign = 1f;
			uIPanel.Append(uIScrollbar);
			mainConfigList.SetScrollbar(uIScrollbar);

			headerTextPanel = new UITextPanel<string>("Mod Config", 0.8f, true);
			headerTextPanel.HAlign = 0.5f;
			headerTextPanel.Top.Set(-50f, 0f);
			headerTextPanel.SetPadding(15f);
			headerTextPanel.BackgroundColor = UICommon.defaultUIBlue;
			uIElement.Append(headerTextPanel);

			previousConfigButton = new UITextPanel<string>("<", 1f, false);
			previousConfigButton.Width.Set(25f, 0);
			previousConfigButton.Height.Set(25f, 0f);
			previousConfigButton.VAlign = 1f;
			previousConfigButton.Top.Set(-65f, 0f);
			previousConfigButton.HAlign = 0f;
			previousConfigButton.WithFadedMouseOver();
			previousConfigButton.OnClick += PreviousConfig;
			//uIElement.Append(previousConfigButton);

			nextConfigButton = new UITextPanel<string>(">", 1f, false);
			nextConfigButton.CopyStyle(previousConfigButton);
			nextConfigButton.WithFadedMouseOver();
			nextConfigButton.HAlign = 1f;
			nextConfigButton.OnClick += NextConfig;
			//uIElement.Append(nextConfigButton);

			saveConfigButton = new UITextPanel<string>("Save Config", 1f, false);
			saveConfigButton.Width.Set(-10f, 1f / 4f);
			saveConfigButton.Height.Set(25f, 0f);
			saveConfigButton.Top.Set(-20f, 0f);
			saveConfigButton.WithFadedMouseOver();
			saveConfigButton.HAlign = 0.33f;
			saveConfigButton.VAlign = 1f;
			saveConfigButton.OnClick += SaveConfig;
			//uIElement.Append(saveConfigButton);

			backButton = new UITextPanel<string>("Back", 1f, false);
			backButton.CopyStyle(saveConfigButton);
			backButton.HAlign = 0;
			backButton.WithFadedMouseOver();
			backButton.OnMouseOver += (a, b) =>
			{
				if (pendingChanges)
					backButton.BackgroundColor = Color.Red;
			};
			backButton.OnMouseOut += (a, b) =>
			{
				if (pendingChanges)
					backButton.BackgroundColor = Color.Red * 0.7f;
			};
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);

			revertConfigButton = new UITextPanel<string>("Revert Changes", 1f, false);
			revertConfigButton.CopyStyle(saveConfigButton);
			revertConfigButton.WithFadedMouseOver();
			revertConfigButton.HAlign = 0.66f;
			revertConfigButton.OnClick += RevertConfig;
			//uIElement.Append(revertConfigButton);

			//float scale = Math.Min(1f, 130f/Main.fontMouseText.MeasureString("Restore Defaults").X);
			restoreDefaultsConfigButton = new UITextPanel<string>("Restore Defaults", 1f, false);
			restoreDefaultsConfigButton.CopyStyle(saveConfigButton);
			restoreDefaultsConfigButton.WithFadedMouseOver();
			restoreDefaultsConfigButton.HAlign = 1f;
			restoreDefaultsConfigButton.OnClick += RestoreDefaults;
			uIElement.Append(restoreDefaultsConfigButton);

			uIPanel.BackgroundColor = UICommon.mainPanelBackground;

			Append(uIElement);
		}


		private void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuClose);
			Main.menuMode = Interface.modsMenuID;

			//Main.menuMode = 1127;
			if (!Main.gameMenu)
			{
				Main.InGameUI.SetState(Interface.modConfigList);
			}
			//IngameFancyUI.Close();
			//if (ConfigManager.ModNeedsReload(mod))
			//{
			//	Main.menuMode = Interface.reloadModsID;
			//}
		}

		// TODO: with in-game version, disable MultiplayerSyncMode.ServerDictates configs (View Only maybe?)
		private void PreviousConfig(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			//DiscardChanges();
			int index = modConfigs.IndexOf(modConfig);
			modConfig = modConfigs[index - 1 < 0 ? modConfigs.Count - 1 : index - 1];
			//modConfigClone = modConfig.Clone();
			DoMenuModeState();
		}

		private void NextConfig(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			DiscardChanges();
			int index = modConfigs.IndexOf(modConfig);
			modConfig = modConfigs[index + 1 > modConfigs.Count ? 0 : index + 1];
			//modConfigClone = modConfig.Clone();
			DoMenuModeState();
		}

		private void DoMenuModeState()
		{
			if (Main.gameMenu)
				Main.menuMode = Interface.modConfigID;
			else
				Main.InGameUI.SetState(Interface.modConfig);
		}

		// TODO: With in-game version prevent save that would cause ReloadRequired to be true?
		// Applies the changes to 
		private void SaveConfig(UIMouseEvent evt, UIElement listeningElement)
		{
			if (Main.gameMenu)
			{
				Main.PlaySound(SoundID.MenuOpen);
				// save takes clone and saves to disk, but how can we know if we need to reload
				ConfigManager.Save(modConfigClone);
				ConfigManager.Load(modConfig); // Changes not taken effect?
											   // Reload will be forced by Back Button in UIMods if needed
			}
			else
			{
				// TODO: Server request.
				if (modConfigClone.Mode == MultiplayerSyncMode.ServerDictates && Main.netMode == NetmodeID.MultiplayerClient)
				{
					SetMessage("Asking server to accept changes...", Color.Yellow);

					var requestChanges = new ModPacket(MessageID.InGameChangeConfig);
					requestChanges.Write(modConfigClone.mod.Name);
					requestChanges.Write(modConfigClone.Name);
					string json = JsonConvert.SerializeObject(modConfigClone, ConfigManager.serializerSettings);
					requestChanges.Write(json);
					requestChanges.Send();

					//IngameFancyUI.Close();

					// Make a packet with just this config and send to server.
					// Server responds. On receive, Save and Load? No. Swap out pendingConfig<NetConfig> and PopulateObject
					//                      -- if UI is open, update message and reload: DoMenuModeState();

					return;
				}
				if (modConfigClone.NeedsReload(modConfig))
				{
					Main.PlaySound(SoundID.MenuClose);
					SetMessage("Can't save because changes would require a reload.", Color.Red);
					return;
				}
				else
				{
					Main.PlaySound(SoundID.MenuOpen);
					ConfigManager.Save(modConfigClone);
					ConfigManager.Load(modConfig);
				}
			}


			//if (ConfigManager.ModNeedsReload(modConfig.mod))
			//{
			//	Main.menuMode = Interface.reloadModsID;
			//}
			//else
			//{
			DoMenuModeState();
			//}

			// RELOAD HERE!
		}

		private void RestoreDefaults(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			//ConfigManager.Reset(modConfigClone);
			//ConfigManager.Save(modConfigClone);
			//ConfigManager.Load(modConfig);
			pendingRevertDefaults = true;
			DoMenuModeState();
		}

		private void RevertConfig(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(ID.SoundID.MenuOpen);
			DiscardChanges();
		}

		private void DiscardChanges()
		{
			// reclone, then reload this UI
			//modConfigClone = modConfig.Clone();
			DoMenuModeState();
		}

		bool pendingChanges;
		bool pendingChangesUIUpdate;
		public void SetPendingChanges(bool changes = true)
		{
			pendingChangesUIUpdate |= changes;
			pendingChanges |= changes;
		}

		public void SetMessage(string text, Color color)
		{
			message.TextScale = 1f;
			message.SetText("Notification: " + text);
			float width = Main.fontMouseText.MeasureString(text).X;
			if (width > 400)
			{
				message.TextScale = 400 / width;
				message.Recalculate();
			}
			message.TextColor = color;
		}

		bool netUpdate;
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (pendingChangesUIUpdate)
			{
				uIElement.Append(saveConfigButton);
				uIElement.Append(revertConfigButton);
				backButton.BackgroundColor = Color.Red * 0.7f;
				pendingChangesUIUpdate = false;
			}
			if (netUpdate)
			{
				DoMenuModeState();
				netUpdate = false;
			}
		}

		public static string tooltip;
		public override void Draw(SpriteBatch spriteBatch)
		{
			tooltip = null;
			base.Draw(spriteBatch);
			if (!string.IsNullOrEmpty(tooltip))
			{
				float x = Main.fontMouseText.MeasureString(tooltip).X;
				Vector2 vector = vector = new Vector2((float)Main.mouseX, (float)Main.mouseY) + new Vector2(16f);
				if (vector.Y > (float)(Main.screenHeight - 30))
				{
					vector.Y = (float)(Main.screenHeight - 30);
				}
				if (vector.X > (float)(GetDimensions().Width + GetDimensions().X - x - 16))
				{
					vector.X = (float)(GetDimensions().Width + GetDimensions().X - x - 16);
				}
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, tooltip, vector.X, vector.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);
			}
		}

		// do we need 2 copies? We can discard changes by reloading.
		// We can save pending changes by saving file then loading/reloading mods.
		// when we get new server configs from server...replace, don't save?
		// reload manually, reload fresh server config?
		// need some CopyTo method to preserve references....hmmm
		internal void SetMod(Mod mod, ModConfig config = null)
		{
			this.mod = mod;
			if (ConfigManager.Configs.ContainsKey(mod))
			{
				modConfigs = ConfigManager.Configs[mod];
				modConfig = modConfigs[0];
				if (config != null)
				{
					modConfig = ConfigManager.Configs[mod].First(x => x == config);
					// TODO, decide which configs to show in game: modConfigs = ConfigManager.Configs[mod].Where(x => x.Mode == MultiplayerSyncMode.UniquePerPlayer).ToList();
				}
				//modConfigClone = modConfig.Clone();

				// if in game, maybe have all configs open

			}
			else
			{
				throw new Exception($"There are no ModConfig for {mod.DisplayName}, how did this happen?");
			}
		}

		static bool pendingRevertDefaults;
		public override void OnActivate()
		{
			SetMessage("", Color.White);
			string configDisplayName = ((LabelAttribute)Attribute.GetCustomAttribute(modConfig.GetType(), typeof(LabelAttribute)))?.Label ?? modConfig.Name;
			headerTextPanel.SetText(modConfig.mod.DisplayName + ": " + configDisplayName);
			modConfigClone = modConfig.Clone();
			pendingChanges = pendingRevertDefaults;
			if (pendingRevertDefaults)
			{
				pendingRevertDefaults = false;
				ConfigManager.Reset(modConfigClone);
				pendingChangesUIUpdate = true;
			}

			int index = modConfigs.IndexOf(modConfig);
			int count = modConfigs.Count;
			//pendingChanges = false;
			backButton.BackgroundColor = UICommon.defaultUIBlueMouseOver;
			uIElement.RemoveChild(saveConfigButton);
			uIElement.RemoveChild(revertConfigButton);
			uIElement.RemoveChild(previousConfigButton);
			uIElement.RemoveChild(nextConfigButton);
			if (index + 1 < count)
				uIElement.Append(nextConfigButton);
			if (index - 1 >= 0)
				uIElement.Append(previousConfigButton);

			uIElement.RemoveChild(configPanelStack.Peek());
			uIElement.Append(uIPanel);
			mainConfigList.Clear();
			configPanelStack.Clear();
			configPanelStack.Push(uIPanel);
			subPageStack.Clear();
			//currentConfigList = mainConfigList;
			int i = 0;
			int top = 0;
			// load all mod config options into UIList
			// TODO: Inheritance with ModConfig? DeclaredOnly?

			uIPanel.BackgroundColor = UICommon.mainPanelBackground;
			var backgroundColorAttribute = (BackgroundColorAttribute)Attribute.GetCustomAttribute(modConfigClone.GetType(), typeof(BackgroundColorAttribute));
			if (backgroundColorAttribute != null) {
				uIPanel.BackgroundColor = backgroundColorAttribute.color;
			}

			int order = 0;
			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(modConfigClone))
			{
				if (variable.isProperty && variable.Name == "Mode")
					continue;
				if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) && !Attribute.IsDefined(variable.MemberInfo, typeof(LabelAttribute))) // TODO, appropriately named attribute
					continue;

				WrapIt(mainConfigList, ref top, variable, modConfigClone, order++);
			}
		}

		public static Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object array = null, Type arrayType = null, int index = -1)
		{
			int elementHeight = 0;
			Type type = memberInfo.Type;
			if (arrayType != null)
			{
				type = arrayType;
			}
			//int original = sliderIDInPage;
			UIElement e = null;

			// TODO: Vector2, other common structs?
			CustomModConfigItemAttribute customUI = ConfigManager.GetCustomAttribute<CustomModConfigItemAttribute>(memberInfo, null, null);
			if (customUI != null)
			{
				Type customUIType = customUI.t;
				ConstructorInfo ctor = customUIType.GetConstructor(new[] { typeof(PropertyFieldWrapper), typeof(object), typeof(int), typeof(IList), typeof(int) });
				if (ctor != null)
				{
					object[] arguments = new object[] { memberInfo, item, 0, array, index };
					object instance = ctor.Invoke(arguments);
					//sliderIDInPage = (int)arguments[2];
					e = instance as UIElement;
					if (e != null)
					{
						//e.Recalculate();
						//elementHeight = (int)e.GetOuterDimensions().Height;
						//elementHeight = 400; //e.GetHeight();
					}
					else
					{
						e = new UIText($"CustomUI for {memberInfo.Name} does not inherit from UIElement.");
					}
				}
				else
				{
					e = new UIText($"CustomUI for {memberInfo.Name} does not have the correct constructor.");
				}
			}
			else if (type == typeof(ItemDefinition))
			{
				e = new ItemDefinitionElement(memberInfo, item, (IList<ItemDefinition>)array, index);
			}
			else if (type == typeof(Color))
			{
				e = new ColorElement(memberInfo, item, (IList<Color>)array, index);
				//elementHeight = (int)(e as UIModConfigColorItem).GetHeight();
			}
			else if (type == typeof(Vector2)) {
				e = new Vector2Element(memberInfo, item, (IList<Vector2>)array, index);
			}
			else if (type == typeof(bool)) // isassignedfrom?
			{
				e = new BooleanElement(memberInfo, item, (IList<bool>)array, index);
			}
			else if (type == typeof(float))
			{
				e = new FloatElement(memberInfo, item,  (IList<float>)array, index);
			}
			else if (type == typeof(byte))
			{
				e = new ByteElement(memberInfo, item,  (IList<byte>)array, index);
			}
			else if (type == typeof(uint))
			{
				e = new UIntElement(memberInfo, item,  (IList<uint>)array, index);
			}
			else if (type == typeof(int))
			{
				RangeAttribute rangeAttribute = ConfigManager.GetCustomAttribute<RangeAttribute>(memberInfo, item, array);
				if (rangeAttribute != null)
					e = new IntRangeElement(memberInfo, item,(IList<int>)array, index);
				else
					e = new IntInputElement(memberInfo, item, (IList<int>)array, index);
			}
			else if (type == typeof(string))
			{
				OptionStringsAttribute ost = ConfigManager.GetCustomAttribute<OptionStringsAttribute>(memberInfo, item, array);
				if (ost != null)
				{
					e = new StringOptionElement(memberInfo, item, (IList<string>)array, index);
				}
				else
				{
					e = new StringInputElement(memberInfo, item, (IList<string>)array, index);
				}
			}
			else if (type.IsEnum)
			{
				if (array != null)
				{
					e = new UIText($"{memberInfo.Name} not handled yet ({type.Name}).");
				}
				else
				{
					e = new EnumElement(memberInfo, item);
				}
			}
			else if (type.IsArray)
			{
				e = new ArrayElement(memberInfo, item);
				//elementHeight = 225;
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
			{
				e = new ListElement(memberInfo, item);
				//elementHeight = 225;
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
			{
				e = new SetElement(memberInfo, item);
			}
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
			{
				e = new DictionaryElement(memberInfo, item);
				//elementHeight = 300;
			}
			else if (type.IsClass)
			{
				if (array != null)
				{
					object listItem = ((IList)array)[index];
					if (listItem == null)
					{
						listItem = Activator.CreateInstance(type);
						JsonConvert.PopulateObject("{}", listItem, ConfigManager.serializerSettings);
						((IList)array)[index] = listItem;
					}
					e = new ObjectElement(memberInfo, listItem, (IList)array, index);
					//elementHeight = (int)(e as UIModConfigObjectItem).GetHeight();
				}
				else
				{
					object subitem = memberInfo.GetValue(item);
					if (subitem == null)
					{
						subitem = Activator.CreateInstance(type);
						// Crashes JSONItem
						JsonConvert.PopulateObject("{}", subitem, ConfigManager.serializerSettings); // Seems to fail on all data structures?

						//JsonDefaultValueAttribute jsonDefaultValueAttribute = (JsonDefaultValueAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(JsonDefaultValueAttribute));
						//if (jsonDefaultValueAttribute != null)
						//{
						//	JsonConvert.PopulateObject(jsonDefaultValueAttribute.json, subitem, ConfigManager.serializerSettings);
						//}

						memberInfo.SetValue(item, subitem);
					}
					SeparatePageAttribute att = (SeparatePageAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(SeparatePageAttribute));
					if (att != null)
					{
						UIPanel separateListPanel = MakeSeparateListPanel(subitem, memberInfo);

						string name = ConfigManager.GetCustomAttribute<LabelAttribute>(memberInfo, subitem, null)?.Label ?? memberInfo.Name;
						e = new UITextPanel<string>(name);
						e.HAlign = 0.5f;
						//e.Recalculate();
						//elementHeight = (int)e.GetOuterDimensions().Height;
						e.OnClick += (a, c) =>
						{
							Interface.modConfig.uIElement.RemoveChild(Interface.modConfig.configPanelStack.Peek());
							Interface.modConfig.uIElement.Append(separateListPanel);
							Interface.modConfig.configPanelStack.Push(separateListPanel);
							//separateListPanel.SetScrollbar(Interface.modConfig.uIScrollbar);

							//UIPanel panel = new UIPanel();
							//panel.Width.Set(200, 0);
							//panel.Height.Set(200, 0);
							//panel.Left.Set(200, 0);
							//panel.Top.Set(200, 0);
							//Interface.modConfig.Append(panel);

							//Interface.modConfig.subMenu.Enqueue(subitem);
							//Interface.modConfig.DoMenuModeState();
						};
						//e = new UIText($"{memberInfo.Name} click for more ({type.Name}).");
						//e.OnClick += (a, b) => { };
					}
					else
					{
						e = new ObjectElement(memberInfo, subitem);
						//elementHeight = (int)(e as UIModConfigObjectItem).GetHeight();
					}
				}
			}
			else if (type.IsValueType && !type.IsPrimitive)
			{
				e = new UIText($"{memberInfo.Name} not handled yet ({type.Name}) Structs need special UI.");
				//e.Top.Pixels += 6;
				e.Height.Pixels += 6;
				e.Left.Pixels += 4;

				object subitem = memberInfo.GetValue(item);
			}
			else
			{
				e = new UIText($"{memberInfo.Name} not handled yet ({type.Name})");
				e.Top.Pixels += 6;
				e.Left.Pixels += 4;
			}
			if (e != null)
			{
				e.Recalculate();
				elementHeight = (int)e.GetOuterDimensions().Height;

				var container = GetContainer(e, index == -1 ? order : index);
				container.Height.Pixels = elementHeight;
				UIList list = parent as UIList;
				if (list != null)
				{
					list.Add(container);
					float p = list.GetTotalHeight();
				}
				else
				{
					container.Top.Pixels = top;
					container.Width.Pixels = -20;
					container.Left.Pixels = 20;
					top += elementHeight + 4; // or use list and padding?
					parent.Append(container);
					parent.Height.Set(top, 0);
				}

				return new Tuple<UIElement, UIElement>(container, e);
			}
			return null;
		}

		private static UIPanel MakeSeparateListPanel(object subitem, PropertyFieldWrapper memberInfo)
		{
			UIPanel uIPanel = new UIPanel();
			uIPanel.CopyStyle(Interface.modConfig.uIPanel);
			uIPanel.BackgroundColor = UICommon.mainPanelBackground;

			BackgroundColorAttribute bca = ConfigManager.GetCustomAttribute<BackgroundColorAttribute>(memberInfo, subitem, null);
			if (bca != null)
			{
				uIPanel.BackgroundColor = bca.color;
			}

			//uIElement.Append(uIPanel);

			UIList separateList = new UIList();
			separateList.CopyStyle(Interface.modConfig.mainConfigList);
			separateList.Height.Set(-40f, 1f);
			separateList.Top.Set(40f, 0f);
			uIPanel.Append(separateList);
			int i = 0;
			int top = 0;

			UIScrollbar uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(-40f, 1f);
			uIScrollbar.Top.Set(40f, 0f);
			uIScrollbar.HAlign = 1f;
			uIPanel.Append(uIScrollbar);
			separateList.SetScrollbar(uIScrollbar);

			string name = ConfigManager.GetCustomAttribute<LabelAttribute>(memberInfo, subitem, null)?.Label ?? memberInfo.Name;
			Interface.modConfig.subPageStack.Push(name);
			//UIPanel heading = new UIPanel();
			//UIText headingText = new UIText(name);

			name = string.Join(" > ", Interface.modConfig.subPageStack.Reverse()); //.Aggregate((current, next) => current + "/" + next);

			UITextPanel<string> heading = new UITextPanel<string>(name);
			heading.HAlign = 0f;
			//heading.Width.Set(-10, 0.5f);
			//heading.Left.Set(60, 0f);
			heading.Top.Set(-6, 0);
			heading.Height.Set(40, 0);
			//var headingContainer = GetContainer(heading, i++);
			//headingContainer.Height.Pixels = 40;
			uIPanel.Append(heading);
			//headingText.Top.Set(6, 0);
			//headingText.Left.Set(0, .5f);
			//headingText.HAlign = .5f;
			//uIPanel.Append(headingText);
			//top += 40;

			UITextPanel<string> back = new UITextPanel<string>("Back");
			back.HAlign = 1f;
			back.Width.Set(50, 0f);
			back.Top.Set(-6, 0);
			//top += 40;
			//var capturedCurrent = Interface.modConfig.currentConfigList;
			back.OnClick += (a, c) =>
			{
				Interface.modConfig.uIElement.RemoveChild(uIPanel);
				Interface.modConfig.configPanelStack.Pop();
				Interface.modConfig.uIElement.Append(Interface.modConfig.configPanelStack.Peek());
				//Interface.modConfig.configPanelStack.Peek().SetScrollbar(Interface.modConfig.uIScrollbar);
				//Interface.modConfig.currentConfigList = capturedCurrent;
			};
			back.WithFadedMouseOver();
			//var backContainer = GetContainer(back, i++);
			//backContainer.Height.Pixels = 40;
			uIPanel.Append(back);

			//var b = new UIText("Test");
			//separateList.Add(b);
			// Make rest of list


			// load all mod config options into UIList
			// TODO: Inheritance with ModConfig? DeclaredOnly?

			int order = 0;
			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(subitem))
			{
				if (variable.isProperty && variable.Name == "Mode")
					continue;
				if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)) && !Attribute.IsDefined(variable.MemberInfo, typeof(LabelAttribute))) // TODO, appropriately named attribute
					continue;

				WrapIt(separateList, ref top, variable, subitem, order++);
			}
			Interface.modConfig.subPageStack.Pop();
			return uIPanel;
		}

		internal static UIElement GetContainer(UIElement containee, int sortid)
		{
			UIElement container = new UISortableElement(sortid);
			container.Width.Set(0f, 1f);
			container.Height.Set(30f, 0f);
			//container.HAlign = 1f;
			container.Append(containee);
			return container;
		}

		//public override void Recalculate()
		//{
		//	base.Recalculate();
		//	mainConfigList?.Recalculate();
		//}
	}
}
