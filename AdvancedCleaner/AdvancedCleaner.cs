using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Commands;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace AdvancedCleaner
{
	public class AdvancedCleaner : RocketPlugin
	{
		public static List<Transform> Barricades
		{
			get
			{
				return AdvancedCleaner.FindBarricades();
			}
		}

		public static List<Transform> Structures
		{
			get
			{
				return AdvancedCleaner.FindStructures();
			}
		}

		private static List<Transform> FindBarricades()
		{
			List<Transform> list = new List<Transform>();
			for (int i = 0; i < BarricadeManager.BarricadeRegions.GetLength(0); i++)
			{
				for (int j = 0; j < BarricadeManager.BarricadeRegions.GetLength(1); j++)
				{
					BarricadeRegion barricadeRegion = BarricadeManager.BarricadeRegions[i, j];
					for (int k = 0; k < barricadeRegion.drops.Count; k++)
					{
						BarricadeDrop barricadeDrop = barricadeRegion.drops[k];
						list.Add(barricadeDrop.model);
					}
				}
			}
			return list;
		}

		private static List<Transform> FindStructures()
		{
			List<Transform> list = new List<Transform>();
			for (int i = 0; i < StructureManager.regions.GetLength(0); i++)
			{
				for (int j = 0; j < StructureManager.regions.GetLength(1); j++)
				{
					StructureRegion structureRegion = StructureManager.regions[i, j];
					for (int k = 0; k < structureRegion.drops.Count; k++)
					{
						StructureDrop structureDrop = structureRegion.drops[k];
						list.Add(structureDrop.model);
					}
				}
			}
			return list;
		}

		public static List<Transform> FindAll()
		{
			List<Transform> barricades = AdvancedCleaner.Barricades;
			List<Transform> structures = AdvancedCleaner.Structures;
			foreach (Transform item in structures)
			{
				barricades.Add(item);
			}
			return barricades;
		}

		public static AdvancedCleaner Instance { get; set; }

		protected override void Load()
		{
			AdvancedCleaner.Instance = this;
			Logger.Log("AdvancedCleaner is activated.", ConsoleColor.Yellow);
			Logger.Log("Thank you for downloading AdvancedCleaner.", ConsoleColor.Yellow);
		}

		protected override void Unload()
		{
			base.Unload();
		}

		public static Transform Item;
		public static bool IsClaim
        {
			get
            {
				return ClaimManager.checkCanBuild(AdvancedCleaner.Item.position, default(CSteamID), default(CSteamID), false);
			}
        }

		[RocketCommand("clean", "/clean c/all radius", "/clean c/all radius", AllowedCaller.Player)]
		public void Execute(IRocketPlayer caller, string[] commands)
		{
			if (commands.Length < 1)
			{
				UnturnedChat.Say(caller, "Usage: /clean c/all radius");
				return;
			}
            try
            {
				int final = Int32.Parse(commands[1]);
            }
			catch (FormatException)
            {
				UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
				return;
			}
			var req = commands[0].ToLower();
				if (req == "c")
				{
					UnturnedPlayer p = (UnturnedPlayer)caller;
					List<Transform> list = new List<Transform>();
					float dist = float.Parse(commands[1]);
					list = AdvancedCleaner.FindAll().FindAll((Transform x) => Vector3.Distance(p.Position, x.position) <= dist);
					bool flag = list.Count > 0;
					if (flag)
					{
						var check = 0;
						List<Transform> cprotected = new List<Transform>();
						foreach (Transform transform in list)
						{
							AdvancedCleaner.Item = transform;
							check++;
							if (AdvancedCleaner.IsClaim)
							{
								BarricadeManager.damage(transform, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
								StructureManager.damage(transform, Vector3.zero, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
								cprotected.Add(transform);
								if(check == list.Count)
								{
								UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("SuccessClaimed", new object[]
								{
								cprotected.Count,
								commands[1]
								}));
								return;
								}
							}
							else
							{
								if (cprotected.Count == 0 && check == list.Count)
								{
								UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("FoundButClaimed", new object[]
								{
								list.Count,
								commands[1]
								}));
								}
								else if (cprotected.Count != 0 && check == list.Count)
						        {
								UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("SuccessClaimed", new object[]
								{
								cprotected.Count,
								commands[1]
								}));
								return;
								}
							}
						}
						return;
					}
					else
					{
						UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Fail", new object[]
						{
						commands[1]
						}));
					}
					return;
				}
				else if (req == "all")
				{
					UnturnedPlayer p = (UnturnedPlayer)caller;
					List<Transform> list = new List<Transform>();
					float dist = float.Parse(commands[1]);
					list = AdvancedCleaner.FindAll().FindAll((Transform x) => Vector3.Distance(p.Position, x.position) <= dist);
					bool flag = list.Count > 0;
					if (flag)
					{
						UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Success", new object[]
						{
						list.Count,
						commands[1]
						}));
						foreach (Transform transform in list)
						{
							BarricadeManager.damage(transform, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
							StructureManager.damage(transform, Vector3.zero, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
						}
					}
					else
					{
						UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Fail", new object[]
						{
						commands[1]
						}));
					}
					return;
				}
				else if (req != "all" && req != "c")
				{
					UnturnedChat.Say(caller, "Wrong Usage. You have to use \"c\" or \"all\"");
					return;
				}

		}

		public override TranslationList DefaultTranslations
		{
			get
			{
				TranslationList translationList = new TranslationList();
				translationList.Add("FoundButClaimed", "\"{0}\" amount Barricade&Structure found in \"{1}\" radius but was protected therefore not deleted.");
				translationList.Add("SuccessClaimed", "\"{0}\" amount of unprotected Barricade&Structure found in \"{1}\" radius and deleted.");
				translationList.Add("Success", "\"{0}\" amount Barricade&Structure found in \"{1}\" radius and deleted.");
				translationList.Add("Fail", "There arent any Barricade&Structure in \"{0}\" radius.");
				return translationList;
			}
		}
	}
}
