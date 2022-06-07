using System;
using System.Collections.Generic;
using System.Linq;
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
	public class AdvancedCleaner : RocketPlugin<Configuration>
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

		private bool TryGetIDs(string Ids, out uint[] IDs)
		{
			string[] possibleIds = Ids.Split(',');
			IDs = new uint[possibleIds.Length];
			for (int i = 0; i < possibleIds.Length; i++)
			{
				if (!uint.TryParse(possibleIds[i], out uint Id)) return false;
				IDs[i] = Id;
			}
			return true;
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

		public static List<InteractableVehicle> Vehicles => VehicleManager.vehicles;

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
		public static List<Transform> FindAllB()
		{
			return AdvancedCleaner.Barricades;
		}

		public static List<Transform> FindAllS()
		{
			return AdvancedCleaner.Structures;
		}

		public void CleanUnclaimedBarricadesStructuresID(IRocketPlayer caller, float radius, uint[] Id)
		{

			UnturnedPlayer p = (UnturnedPlayer)caller;
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAll().FindAll((Transform x) => Vector3.Distance(p.Position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				var check = 0;
				List<Transform> cprotected = new List<Transform>();
				foreach (Transform transform in list)
				{
					uint.TryParse(transform.name, out uint tname);
					bool hasid = Id.Contains(tname);
					if (hasid)
					{
						AdvancedCleaner.Item = transform;
						check++;
						if (AdvancedCleaner.IsClaim(transform.position))
						{
							BarricadeManager.damage(transform, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
							StructureManager.damage(transform, Vector3.zero, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
							cprotected.Add(transform);
							if (check == list.Count)
							{
								UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("SuccessClaimeduncbsid", new object[]
								{
								cprotected.Count,
								radius
								}));
								return;
							}
						}
						else
						{
							if (cprotected.Count == 0 && check == list.Count)
							{
								UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("FoundButClaimeduncbsid", new object[]
								{
								list.Count,
								radius
								}));
							}
							else if (cprotected.Count != 0 && check == list.Count)
							{
								UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("SuccessClaimeduncbsid", new object[]
								{
								cprotected.Count,
								radius
								}));
								return;
							}
						}
					}
					else
					{
						UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Failid", new object[]
						{
						radius
						}));
					}
				}
				return;
			}
			else
			{
				UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Fail", new object[]
				{
						radius
				}));
			}
			return;
		}

		public void CleanUnclaimedBarricadesStructures(IRocketPlayer caller, float radius)
		{
			UnturnedPlayer p = (UnturnedPlayer)caller;
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAll().FindAll((Transform x) => Vector3.Distance(p.Position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				var check = 0;
				List<Transform> cprotected = new List<Transform>();
				foreach (Transform transform in list)
				{
					AdvancedCleaner.Item = transform;
					check++;
					if (AdvancedCleaner.IsClaim(transform.position))
					{
						BarricadeManager.damage(transform, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
						StructureManager.damage(transform, Vector3.zero, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
						cprotected.Add(transform);
						if (check == list.Count)
						{
							UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("SuccessClaimeduncbs", new object[]
							{
								cprotected.Count,
								radius
							}));
							return;
						}
					}
					else
					{
						if (cprotected.Count == 0 && check == list.Count)
						{
							UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("FoundButClaimeduncbs", new object[]
							{
								list.Count,
								radius
							}));
						}
						else if (cprotected.Count != 0 && check == list.Count)
						{
							UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("SuccessClaimeduncbs", new object[]
							{
								cprotected.Count,
								radius
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
						radius
				}));
			}
			return;
		}

		public void CleanUnclaimedBarricadesStructuresConsole(float radius)
		{
			transform.position = new Vector3(1.0f, 2.0f, 3.0f);
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAll().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				var check = 0;
				List<Transform> cprotected = new List<Transform>();
				foreach (Transform transform in list)
				{
					AdvancedCleaner.Item = transform;
					check++;
					if (AdvancedCleaner.IsClaim(transform.position))
					{
						BarricadeManager.damage(transform, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
						StructureManager.damage(transform, Vector3.zero, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
						cprotected.Add(transform);
						if (check == list.Count)
						{
							Logger.Log(AdvancedCleaner.Instance.Translate("SuccessClaimeduncbs", new object[]
							{
								cprotected.Count,
								radius
							}));
							return;
						}
					}
					else
					{
						if (cprotected.Count == 0 && check == list.Count)
						{
							Logger.Log(AdvancedCleaner.Instance.Translate("FoundButClaimeduncbs", new object[]
							{
								list.Count,
								radius
							}));
						}
						else if (cprotected.Count != 0 && check == list.Count)
						{
							Logger.Log(AdvancedCleaner.Instance.Translate("SuccessClaimeduncbs", new object[]
							{
								cprotected.Count,
								radius
							}));
							return;
						}
					}
				}
				return;
			}
			else
			{
				Logger.Log(AdvancedCleaner.Instance.Translate("Fail", new object[]
				{
						radius
				}));
			}
			return;
		}

		public void CleanitemsConsole(float radius)
		{
			Logger.Log(AdvancedCleaner.Instance.Translate("Successi", new object[]
			{
					radius
			}));
			ItemManager.askClearAllItems();
			return;
		}

		public void Cleanitems(IRocketPlayer caller, float radius)
		{
			UnturnedPlayer p = (UnturnedPlayer)caller;
			UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Successi", new object[]
			{
					radius
			}));
			ItemManager.ServerClearItemsInSphere(p.Position, radius);
			return;
		}

		public void CleanEmptyVehicles(IRocketPlayer caller, float radius)
		{
			UnturnedPlayer player = (UnturnedPlayer)caller;
			List<InteractableVehicle> vehiclesInRadius = Vehicles.Where(v => (v.transform.position - player.Position).sqrMagnitude <= Mathf.Pow(radius, 2) && !v.anySeatsOccupied).ToList();
			int dvehicles = vehiclesInRadius.Count;
			for (int v = vehiclesInRadius.Count - 1; v >= 0; v--)
			{
				VehicleManager.askVehicleDestroy(vehiclesInRadius[v]);
			}
			if (dvehicles > 0)
            {
				UnturnedChat.Say(caller, Translate("Successev", radius, dvehicles));
			}
            else
            {
				UnturnedChat.Say(caller, Translate("Failv", radius));
				return;
            }
		}

		public void CleanEmptyVehiclesID(IRocketPlayer caller, float radius, uint[] Id)
		{
			UnturnedPlayer player = (UnturnedPlayer)caller;
			List<InteractableVehicle> vehiclesInRadius = Vehicles.Where(v => (v.transform.position - player.Position).sqrMagnitude <= Mathf.Pow(radius, 2) && !v.anySeatsOccupied).ToList();
			int dvehicles = vehiclesInRadius.Count;
			var check = 0;
			for (int v = vehiclesInRadius.Count - 1; v >= 0; v--)
			{
				uint.TryParse(transform.name, out uint tname);
				bool hasid = Id.Contains(tname);
				if (hasid)
				{
					check++;
					VehicleManager.askVehicleDestroy(vehiclesInRadius[v]);
				}
			}
			if (check > 0)
			{
				UnturnedChat.Say(caller, Translate("Successevid", radius, check));
			}
			else
			{
				UnturnedChat.Say(caller, Translate("Failevid", radius));
				return;
			}
		}


		public void CleanEmptyVehiclesConsole(float radius)
		{
			transform.position = new Vector3(1.0f, 2.0f, 3.0f);
			List<InteractableVehicle> vehiclesInRadius = Vehicles.Where(v => (transform.position - v.transform.position).sqrMagnitude <= Mathf.Pow(radius, 2) && !v.anySeatsOccupied).ToList();
			int dvehicles = vehiclesInRadius.Count;
			for (int v = vehiclesInRadius.Count - 1; v >= 0; v--)
			{
				VehicleManager.askVehicleDestroy(vehiclesInRadius[v]);
			}
			if (dvehicles > 0)
			{
				Logger.Log(Translate("Successev", radius, dvehicles));
			}
			else
			{
				Logger.Log(Translate("Failv", radius));
				return;
			}
		}

		public void CleanVehicles(IRocketPlayer caller, float radius)
		{
			UnturnedPlayer player = (UnturnedPlayer)caller;
			List<InteractableVehicle> vehiclesInRadius = Vehicles.Where(v => (v.transform.position - player.Position).sqrMagnitude <= Mathf.Pow(radius, 2)).ToList();
			int dvehicles = vehiclesInRadius.Count;
			for (int v = vehiclesInRadius.Count - 1; v >= 0; v--)
			{
				VehicleManager.askVehicleDestroy(vehiclesInRadius[v]);
			}
			if (dvehicles > 0)
			{
				UnturnedChat.Say(caller, Translate("Successv", radius, dvehicles));
			}
			else
			{
				UnturnedChat.Say(caller, Translate("Failv", radius));
				return;
			}
		}


		public void CleanVehiclesID(IRocketPlayer caller, float radius, uint[] Id)
		{
			UnturnedPlayer player = (UnturnedPlayer)caller;
			List<InteractableVehicle> vehiclesInRadius = Vehicles.Where(v => (v.transform.position - player.Position).sqrMagnitude <= Mathf.Pow(radius, 2)).ToList();
			int dvehicles = vehiclesInRadius.Count;
			var check = 0;
			for (int v = vehiclesInRadius.Count - 1; v >= 0; v--)
			{
				uint.TryParse(transform.name, out uint tname);
				bool hasid = Id.Contains(tname);
				if (hasid)
                {
					check++;
					VehicleManager.askVehicleDestroy(vehiclesInRadius[v]);
				}
			}
			if (check > 0)
			{
				UnturnedChat.Say(caller, Translate("Successvid", radius, check));
			}
			else
			{
				UnturnedChat.Say(caller, Translate("Failvid", radius));
				return;
			}
		}

		public void CleanVehiclesConsole(float radius)
		{
			transform.position = new Vector3(1.0f, 2.0f, 3.0f);
			List<InteractableVehicle> vehiclesInRadius = Vehicles.Where(v => (transform.position - v.transform.position).sqrMagnitude <= Mathf.Pow(radius, 2)).ToList();
			int dvehicles = vehiclesInRadius.Count;
			for (int v = vehiclesInRadius.Count - 1; v >= 0; v--)
			{
				VehicleManager.askVehicleDestroy(vehiclesInRadius[v]);
			}
			if (dvehicles > 0)
			{
				Logger.Log(Translate("Successv", radius, dvehicles));
			}
			else
			{
				Logger.Log(Translate("Failv", radius));
				return;
			}
		}


		public void CleanBarricadesStructuresID(IRocketPlayer caller, float radius, uint[] Id)
		{
			UnturnedPlayer p = (UnturnedPlayer)caller;
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAll().FindAll((Transform x) => Vector3.Distance(p.Position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				var check = 0;
				foreach (Transform transform in list)
				{
					uint.TryParse(transform.name, out uint tname);
					bool hasid = Id.Contains(tname);
					if (hasid)
					{
						check++;
						BarricadeManager.damage(transform, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
						StructureManager.damage(transform, Vector3.zero, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
					}
				}
				if (check > 0)
				{
					UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Successbsid", new object[]
					{
						list.Count,
						radius
					}));
				}
				else
				{
					UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Fail", new object[]
					{
						radius
					}));
				}
			}
			else
			{
				UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Failid", new object[]
				{
						radius
				}));
			}
			return;
		}

		public void CleanBarricadesStructures(IRocketPlayer caller, float radius)
        {
			UnturnedPlayer p = (UnturnedPlayer)caller;
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAll().FindAll((Transform x) => Vector3.Distance(p.Position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Successbs", new object[]
				{
						list.Count,
						radius
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
						radius
				}));
			}
			return;
		}

		public void CleanBarricadesStructuresConsole(float radius)
		{
			transform.position = new Vector3(1.0f, 2.0f, 3.0f);
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAll().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				Logger.Log(AdvancedCleaner.Instance.Translate("Successbs", new object[]
				{
						list.Count,
						radius
				}));
				foreach (Transform transform in list)
				{
					BarricadeManager.damage(transform, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
					StructureManager.damage(transform, Vector3.zero, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
				}
			}
			else
			{
				Logger.Log(AdvancedCleaner.Instance.Translate("Fail", new object[]
				{
						radius
				}));
			}
			return;
		}


		public void CleanBarricadesID(IRocketPlayer caller, float radius, uint[] Id)
		{
			UnturnedPlayer p = (UnturnedPlayer)caller;
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAllB().FindAll((Transform x) => Vector3.Distance(p.Position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				var check = 0;
				foreach (Transform transform in list)
				{
					uint.TryParse(transform.name, out uint tname);
					bool hasid = Id.Contains(tname);
					if (hasid)
					{
						check++;
						BarricadeManager.damage(transform, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
					}
				}
				if (check > 0)
				{
					UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Successbid", new object[]
					{
						list.Count,
						radius
					}));
				}
				else
				{
					UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Fail", new object[]
					{
						radius
					}));
				}
			}
			else
			{
				UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Failid", new object[]
				{
						radius
				}));
			}
			return;
		}

		public void CleanBarricades(IRocketPlayer caller, float radius)
		{
			UnturnedPlayer p = (UnturnedPlayer)caller;
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAllB().FindAll((Transform x) => Vector3.Distance(p.Position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Successbid", new object[]
				{
						list.Count,
						radius
				}));
				foreach (Transform transform in list)
				{
					BarricadeManager.damage(transform, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
				}
			}
			else
			{
				UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Fail", new object[]
				{
						radius
				}));
			}
			return;
		}

		public void CleanBarricadesConsole(float radius)
		{
			transform.position = new Vector3(1.0f, 2.0f, 3.0f);
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAllB().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				Logger.Log(AdvancedCleaner.Instance.Translate("Successb", new object[]
				{
						list.Count,
						radius
				}));
				foreach (Transform transform in list)
				{
					BarricadeManager.damage(transform, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
				}
			}
			else
			{
				Logger.Log(AdvancedCleaner.Instance.Translate("Fail", new object[]
				{
						radius
				}));
			}
			return;
		}

		public void CleanStructures(IRocketPlayer caller, float radius)
		{
			UnturnedPlayer p = (UnturnedPlayer)caller;
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAllS().FindAll((Transform x) => Vector3.Distance(p.Position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Successs", new object[]
				{
						list.Count,
						radius
				}));
				foreach (Transform transform in list)
				{
					StructureManager.damage(transform, Vector3.zero, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
				}
			}
			else
			{
				UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Fail", new object[]
				{
						radius
				}));
			}
			return;
		}

		public void CleanStructuresID(IRocketPlayer caller, float radius, uint[] Id)
		{
			UnturnedPlayer p = (UnturnedPlayer)caller;
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAllS().FindAll((Transform x) => Vector3.Distance(p.Position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				var check = 0;
				foreach (Transform transform in list)
				{
					uint.TryParse(transform.name, out uint tname);
					bool hasid = Id.Contains(tname);
					if (hasid)
                    {
						check++;
						StructureManager.damage(transform, Vector3.zero, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
					}
				}
				if (check > 0) {
					UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Successsid", new object[]
					{
						list.Count,
						radius
					}));
				}
				else
				{
					UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Fail", new object[]
					{
						radius
					}));
				}
			}
			else
			{
				UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Failid", new object[]
				{
						radius
				}));
			}
			return;
		}

		public void CleanStructuresConsole(float radius)
		{
			transform.position = new Vector3(1.0f, 2.0f, 3.0f);
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAllS().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				Logger.Log(AdvancedCleaner.Instance.Translate("Successs", new object[]
				{
						list.Count,
						radius
				}));
				foreach (Transform transform in list)
				{
					StructureManager.damage(transform, Vector3.zero, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
				}
			}
			else
			{
				Logger.Log(AdvancedCleaner.Instance.Translate("Fail", new object[]
				{
						radius
				}));
			}
			return;
		}

		public void CleanUnclaimedBarricadesConsole(float radius)
		{
			transform.position = new Vector3(1.0f, 2.0f, 3.0f);
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAllB().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				var check = 0;
				List<Transform> cprotected = new List<Transform>();
				foreach (Transform transform in list)
				{
					AdvancedCleaner.Item = transform;
					check++;
					if (AdvancedCleaner.IsClaim(transform.position))
					{
						BarricadeManager.damage(transform, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
						cprotected.Add(transform);
						if (check == list.Count)
						{
							Logger.Log(AdvancedCleaner.Instance.Translate("SuccessClaimeduncb", new object[]
							{
								cprotected.Count,
								radius
							}));
							return;
						}
					}
					else
					{
						if (cprotected.Count == 0 && check == list.Count)
						{
							Logger.Log(AdvancedCleaner.Instance.Translate("FoundButClaimeduncb", new object[]
							{
								list.Count,
								radius
							}));
						}
						else if (cprotected.Count != 0 && check == list.Count)
						{
							Logger.Log(AdvancedCleaner.Instance.Translate("SuccessClaimeduncb", new object[]
							{
								cprotected.Count,
								radius
							}));
							return;
						}
					}
				}
				return;
			}
			else
			{
				Logger.Log(AdvancedCleaner.Instance.Translate("Fail", new object[]
				{
						radius
				}));
			}
			return;
		}


		public void CleanUnclaimedBarricadesID(IRocketPlayer caller, float radius, uint[] Id)
		{

			UnturnedPlayer p = (UnturnedPlayer)caller;
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAllB().FindAll((Transform x) => Vector3.Distance(p.Position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				var check = 0;
				List<Transform> cprotected = new List<Transform>();
				foreach (Transform transform in list)
				{
					uint.TryParse(transform.name, out uint tname);
					bool hasid = Id.Contains(tname);
					if (hasid)
					{
						AdvancedCleaner.Item = transform;
						check++;
						if (AdvancedCleaner.IsClaim(transform.position))
						{
							BarricadeManager.damage(transform, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
							cprotected.Add(transform);
							if (check == list.Count)
							{
								UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("SuccessClaimeduncbsid", new object[]
								{
								cprotected.Count,
								radius
								}));
								return;
							}
						}
						else
						{
							if (cprotected.Count == 0 && check == list.Count)
							{
								UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("FoundButClaimeduncbsid", new object[]
								{
								list.Count,
								radius
								}));
							}
							else if (cprotected.Count != 0 && check == list.Count)
							{
								UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("SuccessClaimeduncbsid", new object[]
								{
								cprotected.Count,
								radius
								}));
								return;
							}
						}
					}
					else
					{
						UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Failid", new object[]
						{
						radius
						}));
					}
				}
				return;
			}
			else
			{
				UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Fail", new object[]
				{
						radius
				}));
			}
			return;
		}

		public void CleanUnclaimedBarricades(IRocketPlayer caller, float radius)
		{
			UnturnedPlayer p = (UnturnedPlayer)caller;
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAllB().FindAll((Transform x) => Vector3.Distance(p.Position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				var check = 0;
				List<Transform> cprotected = new List<Transform>();
				foreach (Transform transform in list)
				{
					AdvancedCleaner.Item = transform;
					check++;
					if (AdvancedCleaner.IsClaim(transform.position))
					{
						BarricadeManager.damage(transform, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
						cprotected.Add(transform);
						if (check == list.Count)
						{
							UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("SuccessClaimeduncb", new object[]
							{
								cprotected.Count,
								radius
							}));
							return;
						}
					}
					else
					{
						if (cprotected.Count == 0 && check == list.Count)
						{
							UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("FoundButClaimeduncb", new object[]
							{
								list.Count,
								radius
							}));
						}
						else if (cprotected.Count != 0 && check == list.Count)
						{
							UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("SuccessClaimeduncb", new object[]
							{
								cprotected.Count,
								radius
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
						radius
				}));
			}
			return;
		}

		public void CleanUnclaimedStructuresID(IRocketPlayer caller, float radius, uint[] Id)
		{

			UnturnedPlayer p = (UnturnedPlayer)caller;
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAllS().FindAll((Transform x) => Vector3.Distance(p.Position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				var check = 0;
				List<Transform> cprotected = new List<Transform>();
				foreach (Transform transform in list)
				{
					uint.TryParse(transform.name, out uint tname);
					bool hasid = Id.Contains(tname);
					if (hasid)
					{
						AdvancedCleaner.Item = transform;
						check++;
						if (AdvancedCleaner.IsClaim(transform.position))
						{
							StructureManager.damage(transform, Vector3.zero, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
							cprotected.Add(transform);
							if (check == list.Count)
							{
								UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("SuccessClaimeduncbsid", new object[]
								{
								cprotected.Count,
								radius
								}));
								return;
							}
						}
						else
						{
							if (cprotected.Count == 0 && check == list.Count)
							{
								UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("FoundButClaimeduncbsid", new object[]
								{
								list.Count,
								radius
								}));
							}
							else if (cprotected.Count != 0 && check == list.Count)
							{
								UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("SuccessClaimeduncbsid", new object[]
								{
								cprotected.Count,
								radius
								}));
								return;
							}
						}
					}
					else
					{
						UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Failid", new object[]
						{
						radius
						}));
					}
				}
				return;
			}
			else
			{
				UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("Fail", new object[]
				{
						radius
				}));
			}
			return;
		}

		public void CleanUnclaimedStructures(IRocketPlayer caller, float radius)
		{
			UnturnedPlayer p = (UnturnedPlayer)caller;
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAllS().FindAll((Transform x) => Vector3.Distance(p.Position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				var check = 0;
				List<Transform> cprotected = new List<Transform>();
				foreach (Transform transform in list)
				{
					AdvancedCleaner.Item = transform;
					check++;
					if (AdvancedCleaner.IsClaim(transform.position))
					{
						StructureManager.damage(transform, Vector3.zero, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
						cprotected.Add(transform);
						if (check == list.Count)
						{
							UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("SuccessClaimeduncs", new object[]
							{
								cprotected.Count,
								radius
							}));
							return;
						}
					}
					else
					{
						if (cprotected.Count == 0 && check == list.Count)
						{
							UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("FoundButClaimeduncs", new object[]
							{
								list.Count,
								radius
							}));
						}
						else if (cprotected.Count != 0 && check == list.Count)
						{
							UnturnedChat.Say(caller, AdvancedCleaner.Instance.Translate("SuccessClaimeduncs", new object[]
							{
								cprotected.Count,
								radius
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
						radius
				}));
			}
			return;
		}

		public void CleanUnclaimedStructuresConsole(float radius)
		{
			transform.position = new Vector3(1.0f, 2.0f, 3.0f);
			List<Transform> list = new List<Transform>();
			list = AdvancedCleaner.FindAllS().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
			bool flag = list.Count > 0;
			if (flag)
			{
				var check = 0;
				List<Transform> cprotected = new List<Transform>();
				foreach (Transform transform in list)
				{
					AdvancedCleaner.Item = transform;
					check++;
					if (AdvancedCleaner.IsClaim(transform.position))
					{
						StructureManager.damage(transform, Vector3.zero, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
						cprotected.Add(transform);
						if (check == list.Count)
						{
							Logger.Log(AdvancedCleaner.Instance.Translate("SuccessClaimeduncs", new object[]
							{
								cprotected.Count,
								radius
							}));
							return;
						}
					}
					else
					{
						if (cprotected.Count == 0 && check == list.Count)
						{
							Logger.Log(AdvancedCleaner.Instance.Translate("FoundButClaimeduncs", new object[]
							{
								list.Count,
								radius
							}));
						}
						else if (cprotected.Count != 0 && check == list.Count)
						{
							Logger.Log(AdvancedCleaner.Instance.Translate("SuccessClaimeduncs", new object[]
							{
								cprotected.Count,
								radius
							}));
							return;
						}
					}
				}
				return;
			}
			else
			{
				Logger.Log(AdvancedCleaner.Instance.Translate("Fail", new object[]
				{
						radius
				}));
			}
			return;
		}


		public static AdvancedCleaner Instance { get; set; }

		protected override void Load()
		{
			AdvancedCleaner.Instance = this;
			Logger.Log("AdvancedCleaner by Commando53 is activated.", ConsoleColor.Yellow);
			Logger.Log("Thank you for downloading AdvancedCleaner.", ConsoleColor.Yellow);
			Logger.Log("Please dont forget to make a review on unturnedstore.com ^^", ConsoleColor.Yellow);
		}

		protected override void Unload()
		{
			Logger.Log("AdvancedCleaner is now unloaded.", ConsoleColor.Yellow);
		}

		public static Transform Item;
		public static bool IsClaim(Vector3 Position)
        {
			return ClaimManager.checkCanBuild(Position, default(CSteamID), default(CSteamID), false);
        }

		[RocketCommand("clean", "/clean <UncBS|UncB|UncS|BS|B|S|EV|V|I> (Radius) (ID)", "/clean <UncBS|UncB|UncS|BS|B|S|EV|V|I> (Radius) (ID)", AllowedCaller.Both)]
		public void Execute(IRocketPlayer caller, string[] commands)
		{
			if (commands.Length < 1)
			{
				UnturnedChat.Say(caller, "Usage: /clean <UncBS|UncB|UncS|BS|B|S|EV|V|I> (Radius) (ID)");
				return;
			}
			var req = commands[0].ToLower();
				if (req == "uncbs")
				{
				if(caller is ConsolePlayer)
                {
					float radius = 9999999999;
					CleanUnclaimedBarricadesStructuresConsole(radius);
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					CleanUnclaimedBarricadesStructures(caller, radius);
					return;
				}
				else if(commands.Length < 3)
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					else
					{
						CleanUnclaimedBarricadesStructures(caller, radius);
					}
				}
				else if (commands.Length < 4)
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					if (!TryGetIDs(commands[2], out uint[] ids))
					{
						UnturnedChat.Say(caller, "Id's must be split by using commas. Example \"1091,1092\"");
						return;
					}
					else
					{
						CleanUnclaimedBarricadesStructuresID(caller, radius, ids);
					}
					return;
				}
				}
			else if (req == "uncb")
			{
				if (caller is ConsolePlayer)
				{
					float radius = 9999999999;
					CleanUnclaimedBarricadesConsole(radius);
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					CleanUnclaimedBarricades(caller, radius);
				}
				else if (commands.Length < 3)
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					else
					{
						CleanUnclaimedBarricades(caller, radius);
					}
				}
				else if (commands.Length < 4)
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					if (!TryGetIDs(commands[2], out uint[] ids))
					{
						UnturnedChat.Say(caller, "Id's must be split by using commas. Example \"1091,1092\"");
						return;
					}
					else
					{
						CleanUnclaimedBarricadesID(caller, radius, ids);
					}
					return;
				}
			}
			else if (req == "uncs")
			{
				if (caller is ConsolePlayer)
				{
					float radius = 9999999999;
					CleanUnclaimedStructuresConsole(radius);
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					CleanUnclaimedStructures(caller, radius);
				}
				else if (commands.Length < 3)
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					else
					{
						CleanUnclaimedStructures(caller, radius);
					}
				}
				else if (commands.Length < 4)
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					if (!TryGetIDs(commands[2], out uint[] ids))
					{
						UnturnedChat.Say(caller, "Id's must be split by using commas. Example \"1091,1092\"");
						return;
					}
					else
					{
						CleanUnclaimedStructuresID(caller, radius, ids);
					}
					return;
				}
			}
			else if (req == "i")
			{
				if (caller is ConsolePlayer)
				{
					float radius = 9999999999;
					CleanitemsConsole(radius);
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					Cleanitems(caller, radius);
				}
				else
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					else
					{
						Cleanitems(caller, radius);
					}
				}
			}
			else if (req == "v")
			{
				if (caller is ConsolePlayer)
				{
					float radius = 9999999999;
					CleanVehiclesConsole(radius);
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					CleanVehicles(caller, radius);
				}
				else
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					else
					{
						CleanVehicles(caller, radius);
					}
				}
			}
			else if (req == "ev")
			{
				if (caller is ConsolePlayer)
				{
					float radius = 9999999999;
					CleanEmptyVehiclesConsole(radius);
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					CleanEmptyVehicles(caller, radius);
				}
				else
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					else
					{
						CleanEmptyVehicles(caller, radius);
					}
				}
			}
			else if (req == "bs")
				{
				if (caller is ConsolePlayer)
				{
					float radius = 9999999999;
					CleanBarricadesStructuresConsole(radius);
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					CleanBarricadesStructures(caller, radius);
				}
				else if (commands.Length < 3)
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					else
					{
						CleanBarricadesStructures(caller, radius);
					}
				}
				else if (commands.Length < 4)
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					if (!TryGetIDs(commands[2], out uint[] ids))
					{
						UnturnedChat.Say(caller, "Id's must be split by using commas. Example \"1091,1092\"");
						return;
					}
					else
					{
						CleanBarricadesStructuresID(caller, radius, ids);
					}
					return;
				}
			}
			else if (req == "b")
			{
				if (caller is ConsolePlayer)
				{
					float radius = 9999999999;
					CleanBarricadesConsole(radius);
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					CleanBarricades(caller, radius);
				}
				else if (commands.Length < 3)
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					else
					{
						CleanBarricades(caller, radius);
					}
				}
				else if (commands.Length < 4)
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					if (!TryGetIDs(commands[2], out uint[] ids))
					{
						UnturnedChat.Say(caller, "Id's must be split by using commas. Example \"1091,1092\"");
						return;
					}
					else
					{
						CleanBarricadesID(caller, radius, ids);
					}
					return;
				}
			}
			else if (req == "s")
			{
				if (caller is ConsolePlayer)
				{
					float radius = 9999999999;
					CleanStructuresConsole(radius);
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					CleanStructures(caller, radius);
				}
				else if (commands.Length < 3)
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					else
					{
						CleanStructures(caller, radius);
					}
				}
				else if (commands.Length < 4)
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					if (!TryGetIDs(commands[2], out uint[] ids))
					{
						UnturnedChat.Say(caller, "Id's must be split by using commas. Example \"1091,1092\"");
						return;
					}
					else
					{
						CleanStructuresID(caller, radius, ids);
					}
					return;
				}
			}
			else
			if (caller is ConsolePlayer)
            {
				Logger.Log("Wrong type. /clean <UncBS|UncB|UncS|BS|B|S|EV|V|I> (Radius)");
			}
			else
			{
				UnturnedChat.Say(caller, "Wrong type. /clean <UncBS|UncB|UncS|BS|B|S|EV|V|I> (Radius)");
				return;
			}

		}

		public override TranslationList DefaultTranslations
		{
			get
			{
				TranslationList translationList = new TranslationList();
				translationList.Add("FoundButClaimeduncbs", "\"{0}\" amount of Barricade&Structure found in \"{1}\" radius but was protected therefore not deleted.");
				translationList.Add("SuccessClaimeduncbs", "\"{0}\" amount of unprotected Barricade&Structure found in \"{1}\" radius and deleted.");
				translationList.Add("SuccessClaimeduncb", "\"{0}\" amount of unprotected Barricades found in \"{1}\" radius and deleted.");
				translationList.Add("FoundButClaimeduncb", "\"{0}\" amount of Barricades found in \"{1}\" radius but was protected therefore not deleted.");
				translationList.Add("SuccessClaimeduncs", "\"{0}\" amount of unprotected Structures found in \"{1}\" radius and deleted.");
				translationList.Add("FoundButClaimeduncs", "\"{0}\" amount of Structures found in \"{1}\" radius but was protected therefore not deleted.");
				translationList.Add("Successev", "\"{1}\" amount of Empty Vehicles found in \"{0}\" radius and deleted.");
				translationList.Add("Successv", "\"{1}\" amount of Vehicles found in \"{0}\" radius and deleted.");
				translationList.Add("Successbs", "\"{0}\" amount of Barricade&Structure found in \"{1}\" radius and deleted.");
				translationList.Add("Successb", "\"{0}\" amount of Barricades found in \"{1}\" radius and deleted.");
				translationList.Add("Successs", "\"{0}\" amount of Structures found in \"{1}\" radius and deleted.");
				translationList.Add("Successi", "Any dropped items found in \"{0}\" radius were deleted.");
				translationList.Add("Failid", "There arent any Barricades or Structures with the specified id in \"{0}\" radius.");
				translationList.Add("Fail", "There arent any Barricades or Structures in \"{0}\" radius.");
				translationList.Add("Failv", "There arent any Vehicles in \"{0}\" radius.");
				translationList.Add("FoundButClaimeduncbsid", "\"{0}\" amount of Barricade&Structure with specified id found in \"{1}\" radius but was protected therefore not deleted.");
				translationList.Add("SuccessClaimeduncbsid", "\"{0}\" amount of unprotected Barricade&Structure with specified id found in \"{1}\" radius and deleted.");
				translationList.Add("SuccessClaimeduncbid", "\"{0}\" amount of unprotected Barricades with specified id found in \"{1}\" radius and deleted.");
				translationList.Add("FoundButClaimeduncbid", "\"{0}\" amount of Barricades with specified id found in \"{1}\" radius but was protected therefore not deleted.");
				translationList.Add("SuccessClaimeduncsid", "\"{0}\" amount of unprotected Structures with specified id found in \"{1}\" radius and deleted.");
				translationList.Add("FoundButClaimeduncsid", "\"{0}\" amount of Structures with specified id found in \"{1}\" radius but was protected therefore not deleted.");
				translationList.Add("Successbsid", "\"{0}\" amount of Barricade&Structure with specified id found in \"{1}\" radius and deleted.");
				translationList.Add("Successbid", "\"{0}\" amount of Barricades with specified id found in \"{1}\" radius and deleted.");
				translationList.Add("Successsid", "\"{0}\" amount of Structures with specified id found in \"{1}\" radius and deleted.");
				translationList.Add("Successvid", "\"{1}\" amount of Vehicles with specified id found in \"{0}\" radius and deleted.");
				translationList.Add("Failv", "There arent any Vehicles with specified id in \"{0}\" radius.");
				translationList.Add("Successevid", "\"{1}\" amount of Empty Vehicles with specified id found in \"{0}\" radius and deleted.");
				translationList.Add("Failev", "There arent any Empty Vehicles with specified id in \"{0}\" radius.");
				return translationList;
			}
		}
	}
}
