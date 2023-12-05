using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Commands;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace do53AdvancedCleaner
{
    public class do53AdvancedCleaner : RocketPlugin<do53AdvancedCleanerConfiguration>
	{
		public Dictionary<string, List<Transform>> Confirmation;
		public Dictionary<string, CLEAN> ConfirmationClean;
		public Dictionary<string, GROUPOROWNER> ConfirmationGOO;
		public Dictionary<string, List<InteractableVehicle>> ConfirmationVehicle;
		public Dictionary<string, float> ConfirmationRadius;
		public Dictionary<string, uint[]> ConfirmationIds;
		protected override void Load()
		{
			do53AdvancedCleaner.Instance = this;
			Confirmation = new Dictionary<string, List<Transform>>();
			ConfirmationClean = new Dictionary<string, CLEAN>();
			ConfirmationVehicle = new Dictionary<string, List<InteractableVehicle>>();
			ConfirmationGOO = new Dictionary<string, GROUPOROWNER>();
			ConfirmationRadius = new Dictionary<string, float>();
			ConfirmationIds = new Dictionary<string, uint[]>();
			Logger.Log("do53AdvancedCleaner by Commando53 is activated.", ConsoleColor.Yellow);
			Logger.Log("Thank you for downloading do53AdvancedCleaner.", ConsoleColor.Yellow);
			Logger.Log("If you encounter anything working unexpectedly please report it to me through my discord.", ConsoleColor.Yellow);
			Logger.Log("Discord: https://discord.gg/jnmpkxcV8c", ConsoleColor.Yellow);
			Logger.Log("Please dont forget to make a review on unturnedstore.com ^^", ConsoleColor.Yellow);
		}

		protected override void Unload()
		{
			Confirmation.Clear();
			ConfirmationClean.Clear();
			ConfirmationVehicle.Clear();
			ConfirmationGOO.Clear();
			ConfirmationRadius.Clear();
			ConfirmationIds.Clear();
			Confirmation = null;
			ConfirmationClean = null;
			ConfirmationVehicle = null;
			ConfirmationGOO = null;
			ConfirmationRadius = null;
			ConfirmationIds = null;
			Logger.Log("AdvancedCleaner is now unloaded.", ConsoleColor.Yellow);
		}
		public static do53AdvancedCleaner Instance { get; set; }
		public static Transform Item;
		public static bool IsClaim(Vector3 Position)
		{
			return ClaimManager.checkCanBuild(Position, default(CSteamID), default(CSteamID), false);
		}
		public static List<Transform> Barricades
		{
			get
			{
				return FindBarricades();
			}
		}

		public static List<Transform> Structures
		{
			get
			{
				return FindStructures();
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
			List<Transform> barricades = Barricades;
			List<Transform> structures = Structures;
			foreach (Transform item in structures)
			{
				barricades.Add(item);
			}
			return barricades;
		}
		public static List<Transform> FindAllB()
		{
			return Barricades;
		}

		public static List<Transform> FindAllS()
		{
			return Structures;
		}

		public enum CLEAN
        {
			B,
			S,
			BS,
			UNCB,
			UNCS,
			UNCBS,
			EV,
			V,
			ULEV,
			ULV,
			LEV,
			LV,
			ITEM
        }
		public enum GROUPOROWNER
        {
			GROUP,
			OWNER,
			NONE
        }
		public void Clean(IRocketPlayer caller, float radius, uint[] Id, bool grouporowner, GROUPOROWNER GROUPOROWNER, string steamid, bool confirmation, CLEAN clean, bool checkclaim)
		{
			List <InteractableVehicle> vehiclesInRadius = new List<InteractableVehicle>();
			List<InteractableVehicle> VehiclesFound = new List<InteractableVehicle>();
			List<Transform> list = new List<Transform>();
			if (caller is ConsolePlayer)
            {
				switch (clean)
                {
					case CLEAN.BS:
						transform.position = new Vector3(1.0f, 2.0f, 3.0f);
						list = FindAll().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
						if (list.Count > 0)
						{
							CheckObjects ObjectCheck = CheckTheObjects(list, grouporowner, GROUPOROWNER, steamid, confirmation, Id, checkclaim);
							List<Transform> checklist = ObjectCheck.checklist;
							List<Transform> cprotected = ObjectCheck.cprotected;
							if (checklist.Count == 0)
							{
								if (grouporowner)
                                {
									switch (GROUPOROWNER)
									{
										case GROUPOROWNER.GROUP:
											Logger.Log(Instance.Translate("Failgroup", new object[] { radius }));
											break;
										case GROUPOROWNER.OWNER:
											Logger.Log(Instance.Translate("Failowner", new object[] { radius }));
											break;
									}
									return;
                                }
								if (Id != null)
								{
									Logger.Log(Instance.Translate("Failid", new object[] { radius, }));
									return;
								}
								Logger.Log(Instance.Translate("Fail", new object[] { radius }));
								return;
							}
							if (confirmation)
							{	
								Confirmation[caller.Id] = checklist;
								ConfirmationClean[caller.Id] = clean;
								ConfirmationGOO[caller.Id] = GROUPOROWNER;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								Logger.Log(Instance.Translate("Confirm", new object[] { radius, checklist.Count }));
							}
							else
							{
                                if (grouporowner)
                                {
                                    switch (GROUPOROWNER)
                                    {
										case GROUPOROWNER.GROUP:
											Logger.Log(Instance.Translate("Successbsgroup", new object[] { radius, checklist.Count }));
											break;
										case GROUPOROWNER.OWNER:
											Logger.Log(Instance.Translate("Successbsowner", new object[] { radius, checklist.Count }));
											break;
                                    }
									return;
                                }
								if(Id != null)
                                {
									Logger.Log(Instance.Translate("Successbsid", new object[] { radius, checklist.Count }));
									return;
                                }
								Logger.Log(Instance.Translate("Successbs", new object[] { radius, checklist.Count }));
							}
						}
                        else
                        {
                            if (grouporowner)
                            {
                                switch (GROUPOROWNER)
                                {
									case GROUPOROWNER.GROUP:
										Logger.Log(Instance.Translate("Failgroup", new object[] { radius }));
										break;
									case GROUPOROWNER.OWNER:
										Logger.Log(Instance.Translate("Failowner", new object[] { radius }));
										break;
								}
								return;
                            }
							if (Id != null)
							{
								Logger.Log(Instance.Translate("Failid", new object[] { radius,}));
								return;
							}
							Logger.Log(Instance.Translate("Fail", new object[] { radius }));
						}
						break;
					case CLEAN.B:
						transform.position = new Vector3(1.0f, 2.0f, 3.0f);
						list = FindAllB().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
						if(list.Count > 0)
                        {
							CheckObjects ObjectCheck = CheckTheObjects(list, grouporowner, GROUPOROWNER, steamid, confirmation, Id, checkclaim);
							List<Transform> checklist = ObjectCheck.checklist;
							List<Transform> cprotected = ObjectCheck.cprotected;
							if (checklist.Count == 0)
							{
								if (Id != null)
								{
									Logger.Log(Instance.Translate("Failbid", new object[] { radius, }));
									return;
								}
								Logger.Log(Instance.Translate("Failb", new object[] { radius }));
								return;
							}
							if (confirmation)
                            {
								Confirmation[caller.Id] = checklist;
								ConfirmationClean[caller.Id] = clean;
								ConfirmationGOO[caller.Id] = GROUPOROWNER;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								Logger.Log(Instance.Translate("Confirm", new object[] { radius, checklist.Count }));
							}
                            else
                            {
								Logger.Log(Instance.Translate("Successb", new object[] { radius, checklist.Count }));
							}

						}
                        else
                        {
							if (Id != null)
							{
								Logger.Log(Instance.Translate("Failbid", new object[] { radius, }));
								return;
							}
							Logger.Log(Instance.Translate("Failb", new object[] { radius }));
						}
						break;
					case CLEAN.S:
						transform.position = new Vector3(1.0f, 2.0f, 3.0f);
						list = FindAllS().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
						if (list.Count > 0)
						{
							CheckObjects ObjectCheck = CheckTheObjects(list, grouporowner, GROUPOROWNER, steamid, confirmation, Id, checkclaim);
							List<Transform> checklist = ObjectCheck.checklist;
							List<Transform> cprotected = ObjectCheck.cprotected;
							if (checklist.Count == 0)
							{
								if (Id != null)
								{
									Logger.Log(Instance.Translate("Failsid", new object[] { radius, }));
									return;
								}
								Logger.Log(Instance.Translate("Fails", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								Confirmation[caller.Id] = checklist;
								ConfirmationClean[caller.Id] = clean;
								ConfirmationGOO[caller.Id] = GROUPOROWNER;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								Logger.Log(Instance.Translate("Confirm", new object[] { radius, checklist.Count }));
							}
							else
							{
								Logger.Log(Instance.Translate("Successs", new object[] { radius, checklist.Count }));
							}

						}
                        else
                        {
							if (Id != null)
							{
								Logger.Log(Instance.Translate("Failsid", new object[] { radius, }));
								return;
							}
							Logger.Log(Instance.Translate("Fails", new object[] { radius }));
						}
						break;
					case CLEAN.UNCB:
						transform.position = new Vector3(1.0f, 2.0f, 3.0f);
						list = FindAllB().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
						if (list.Count > 0)
						{
							CheckObjects ObjectCheck = CheckTheObjects(list, grouporowner, GROUPOROWNER, steamid, confirmation, Id, checkclaim);
							List<Transform> checklist = ObjectCheck.checklist;
							List<Transform> cprotected = ObjectCheck.cprotected;
							if(checklist.Count == 0)
                            {
								if (Id != null)
								{
									Logger.Log(Instance.Translate("Failuncbid", new object[] { radius, }));
									return;
								}
								Logger.Log(Instance.Translate("Failuncb", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								Confirmation[caller.Id] = checklist;
								ConfirmationClean[caller.Id] = clean;
								ConfirmationGOO[caller.Id] = GROUPOROWNER;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								Logger.Log(Instance.Translate("Confirm", new object[] { radius, checklist.Count }));
							}
							else
							{
								if (Id != null)
                                {
									if (list.Count == cprotected.Count)
									{
										Logger.Log(Instance.Translate("FoundButClaimeduncbid", new object[] { radius, cprotected.Count }));
										return;
									}
									Logger.Log(Instance.Translate("SuccessClaimeduncbid", new object[] { radius, checklist.Count }));
									return;
								}
                                if (list.Count == cprotected.Count)
                                {
									Logger.Log(Instance.Translate("FoundButClaimeduncb", new object[] { radius, cprotected.Count }));
									return;
								}
								Logger.Log(Instance.Translate("SuccessClaimeduncb", new object[] { radius, checklist.Count }));
								return;
							}
						}
                        else
                        {
							if (Id != null)
							{
								Logger.Log(Instance.Translate("Failuncbid", new object[] { radius, }));
								return;
							}
							Logger.Log(Instance.Translate("Failuncb", new object[] { radius }));
						}
						break;
					case CLEAN.UNCS:
						transform.position = new Vector3(1.0f, 2.0f, 3.0f);
						list = FindAllS().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
						if (list.Count > 0)
						{
							CheckObjects ObjectCheck = CheckTheObjects(list, grouporowner, GROUPOROWNER, steamid, confirmation, Id, checkclaim);
							List<Transform> checklist = ObjectCheck.checklist;
							List<Transform> cprotected = ObjectCheck.cprotected;
							if (checklist.Count == 0)
							{
								if (Id != null)
								{
									Logger.Log(Instance.Translate("Failuncsid", new object[] { radius, }));
									return;
								}
								Logger.Log(Instance.Translate("Failuncs", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								Confirmation[caller.Id] = checklist;
								ConfirmationClean[caller.Id] = clean;
								ConfirmationGOO[caller.Id] = GROUPOROWNER;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								Logger.Log(Instance.Translate("Confirm", new object[] { radius, checklist.Count }));
							}
							else
							{
								if (Id != null)
								{
									if (list.Count == cprotected.Count)
									{
										Logger.Log(Instance.Translate("FoundButClaimeduncsid", new object[] { radius, cprotected.Count }));
										return;
									}
									Logger.Log(Instance.Translate("SuccessClaimeduncsid", new object[] { radius, checklist.Count }));
									return;
								}
								if (list.Count == cprotected.Count)
								{
									Logger.Log(Instance.Translate("FoundButClaimeduncs", new object[] { radius, cprotected.Count }));
									return;
								}
								Logger.Log(Instance.Translate("SuccessClaimeduncs", new object[] { radius, checklist.Count }));
								return;
							}
						}
						else
						{
							if (Id != null)
							{
								Logger.Log(Instance.Translate("Failuncsid", new object[] { radius, }));
								return;
							}
							Logger.Log(Instance.Translate("Failuncs", new object[] { radius }));
						}
						break;
					case CLEAN.UNCBS:
						transform.position = new Vector3(1.0f, 2.0f, 3.0f);
						list = FindAll().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
						if (list.Count > 0)
						{
							CheckObjects ObjectCheck = CheckTheObjects(list, grouporowner, GROUPOROWNER, steamid, confirmation, Id, checkclaim);
							List<Transform> checklist = ObjectCheck.checklist;
							List<Transform> cprotected = ObjectCheck.cprotected;
							if (checklist.Count == 0)
							{
								if (Id != null)
								{
									Logger.Log(Instance.Translate("Failuncbsid", new object[] { radius, }));
									return;
								}
								Logger.Log(Instance.Translate("Failuncbs", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								Confirmation[caller.Id] = checklist;
								ConfirmationClean[caller.Id] = clean;
								ConfirmationGOO[caller.Id] = GROUPOROWNER;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								Logger.Log(Instance.Translate("Confirm", new object[] { radius, checklist.Count }));
							}
							else
							{
								if (Id != null)
								{
									if (list.Count == cprotected.Count)
									{
										Logger.Log(Instance.Translate("FoundButClaimeduncbsid", new object[] { radius, cprotected.Count }));
										return;
									}
									Logger.Log(Instance.Translate("SuccessClaimeduncbsid", new object[] { radius, checklist.Count }));
									return;
								}
								if (list.Count == cprotected.Count)
								{
									Logger.Log(Instance.Translate("FoundButClaimeduncbs", new object[] { radius, cprotected.Count }));
									return;
								}
								Logger.Log(Instance.Translate("SuccessClaimeduncbs", new object[] { radius, checklist.Count }));
								return;
							}
						}
						else
						{
							if (Id != null)
							{
								Logger.Log(Instance.Translate("Failuncbsid", new object[] { radius, }));
								return;
							}
							Logger.Log(Instance.Translate("Failuncbs", new object[] { radius }));
						}
						break;
					case CLEAN.ITEM:
						if(caller is ConsolePlayer)
                        {
							Logger.Log(Instance.Translate("Successi", new object[] { radius }));
							ItemManager.askClearAllItems();
						}
                        else
                        {
							UnturnedPlayer p = (UnturnedPlayer)caller;
							ItemManager.ServerClearItemsInSphere(p.Position, radius);
							Logger.Log(Instance.Translate("Successi", new object[] { radius }));
						}
						break;
					case CLEAN.V:
						transform.position = new Vector3(1.0f, 2.0f, 3.0f);
						vehiclesInRadius = Vehicles.Where(v => (transform.position - v.transform.position).sqrMagnitude <= Mathf.Pow(radius, 2)).ToList();
						VehiclesFound = CheckTheVehicles(vehiclesInRadius, confirmation, Id);
						if (VehiclesFound.Count > 0)
                        {
							if (VehiclesFound.Count == 0)
							{
								if (Id != null)
								{
									Logger.Log(Instance.Translate("Failvid", new object[] { radius, }));
									return;
								}
								Logger.Log(Instance.Translate("Failv", new object[] { radius }));
								return;
							}
							if (confirmation)
                            {
								ConfirmationClean[caller.Id] = clean;
								ConfirmationVehicle[caller.Id] = VehiclesFound;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								Logger.Log(Instance.Translate("Confirm", new object[] { radius, VehiclesFound.Count }));
							}
                            else
                            {
								Logger.Log(Translate("Successv", radius, VehiclesFound.Count));
							}
                        }
                        else
                        {
							if (Id != null)
							{
								Logger.Log(Instance.Translate("Failvid", new object[] { radius, }));
								return;
							}
							Logger.Log(Translate("Failv", radius));
						}
						break;
					case CLEAN.EV:
						transform.position = new Vector3(1.0f, 2.0f, 3.0f);
						vehiclesInRadius = Vehicles.Where(v => (transform.position - v.transform.position).sqrMagnitude <= Mathf.Pow(radius, 2) && !v.anySeatsOccupied).ToList();
						VehiclesFound = CheckTheVehicles(vehiclesInRadius, confirmation, Id);
						if (VehiclesFound.Count > 0)
						{
							if (VehiclesFound.Count == 0)
							{
								if (Id != null)
								{
									Logger.Log(Instance.Translate("Failevid", new object[] { radius, }));
									return;
								}
								Logger.Log(Instance.Translate("Failev", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								ConfirmationClean[caller.Id] = clean;
								ConfirmationVehicle[caller.Id] = VehiclesFound;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								Logger.Log(Instance.Translate("Confirm", new object[] { radius, VehiclesFound.Count }));
							}
							else
							{
								Logger.Log(Translate("Successev", radius, VehiclesFound.Count));
							}
						}
						else
						{
							if (Id != null)
							{
								Logger.Log(Instance.Translate("Failevid", new object[] { radius, }));
								return;
							}
							Logger.Log(Translate("Failev", radius));
						}
						break;
					case CLEAN.LV:
						transform.position = new Vector3(1.0f, 2.0f, 3.0f);
						vehiclesInRadius = Vehicles.Where(v => (transform.position - v.transform.position).sqrMagnitude <= Mathf.Pow(radius, 2) && v.isLocked).ToList();
						VehiclesFound = CheckTheVehicles(vehiclesInRadius, confirmation, Id);
						if (VehiclesFound.Count > 0)
						{
							if (VehiclesFound.Count == 0)
							{
								if (Id != null)
								{
									Logger.Log(Instance.Translate("Faillvid", new object[] { radius, }));
									return;
								}
								Logger.Log(Instance.Translate("Faillv", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								ConfirmationClean[caller.Id] = clean;
								ConfirmationVehicle[caller.Id] = VehiclesFound;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								Logger.Log(Instance.Translate("Confirm", new object[] { radius, VehiclesFound.Count }));
							}
							else
							{
								Logger.Log(Translate("Successlv", radius, VehiclesFound.Count));
							}
						}
						else
						{
							if (Id != null)
							{
								Logger.Log(Instance.Translate("Faillvid", new object[] { radius, }));
								return;
							}
							Logger.Log(Translate("Faillv", radius));
						}
						break;
					case CLEAN.LEV:
						transform.position = new Vector3(1.0f, 2.0f, 3.0f);
						vehiclesInRadius = Vehicles.Where(v => (transform.position - v.transform.position).sqrMagnitude <= Mathf.Pow(radius, 2) && v.isLocked && !v.anySeatsOccupied).ToList();
						VehiclesFound = CheckTheVehicles(vehiclesInRadius, confirmation, Id);
						if (VehiclesFound.Count > 0)
						{
							if (VehiclesFound.Count == 0)
							{
								if (Id != null)
								{
									Logger.Log(Instance.Translate("Faillevid", new object[] { radius, }));
									return;
								}
								Logger.Log(Instance.Translate("Faillev", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								ConfirmationClean[caller.Id] = clean;
								ConfirmationVehicle[caller.Id] = VehiclesFound;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								Logger.Log(Instance.Translate("Confirm", new object[] { radius, VehiclesFound.Count }));
							}
							else
							{
								Logger.Log(Translate("Successlev", radius, VehiclesFound.Count));
							}
						}
						else
						{
							if (Id != null)
							{
								Logger.Log(Instance.Translate("Faillevid", new object[] { radius, }));
								return;
							}
							Logger.Log(Translate("Faillev", radius));
						}
						break;
					case CLEAN.ULV:
						transform.position = new Vector3(1.0f, 2.0f, 3.0f);
						vehiclesInRadius = Vehicles.Where(v => (transform.position - v.transform.position).sqrMagnitude <= Mathf.Pow(radius, 2) && !v.isLocked).ToList();
						VehiclesFound = CheckTheVehicles(vehiclesInRadius, confirmation, Id);
						if (VehiclesFound.Count > 0)
						{
							if (VehiclesFound.Count == 0)
							{
								if (Id != null)
								{
									Logger.Log(Instance.Translate("Failulvid", new object[] { radius, }));
									return;
								}
								Logger.Log(Instance.Translate("Failulv", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								ConfirmationClean[caller.Id] = clean;
								ConfirmationVehicle[caller.Id] = VehiclesFound;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								Logger.Log(Instance.Translate("Confirm", new object[] { radius, VehiclesFound.Count }));
							}
							else
							{
								Logger.Log(Translate("Successulv", radius, VehiclesFound.Count));

							}
						}
						else
						{
							if (Id != null)
							{
								Logger.Log(Instance.Translate("Failulvid", new object[] { radius, }));
								return;
							}
							Logger.Log(Translate("Failulv", radius));
						}
						break;
					case CLEAN.ULEV:
						transform.position = new Vector3(1.0f, 2.0f, 3.0f);
						vehiclesInRadius = Vehicles.Where(v => (transform.position - v.transform.position).sqrMagnitude <= Mathf.Pow(radius, 2) && !v.isLocked && !v.anySeatsOccupied).ToList();
						VehiclesFound = CheckTheVehicles(vehiclesInRadius, confirmation, Id);
						if (VehiclesFound.Count > 0)
						{
							if (VehiclesFound.Count == 0)
							{
								if (Id != null)
								{
									Logger.Log(Instance.Translate("Failulevid", new object[] { radius, }));
									return;
								}
								Logger.Log(Instance.Translate("Failulev", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								ConfirmationClean[caller.Id] = clean;
								ConfirmationVehicle[caller.Id] = VehiclesFound;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								Logger.Log(Instance.Translate("Confirm", new object[] { radius, VehiclesFound.Count }));
							}
							else
							{
								Logger.Log(Translate("Successulev", radius, VehiclesFound.Count));

							}
						}
						else
						{
							if (Id != null)
							{
								Logger.Log(Instance.Translate("Failulevid", new object[] { radius, }));
								return;
							}
							Logger.Log(Translate("Failulev", radius));
						}
						break;
				}
            }
            else
            {
				UnturnedPlayer p = (UnturnedPlayer)caller;
				switch (clean)
				{
					case CLEAN.BS:
						transform.position = p.Position;
						list = FindAll().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
						if (list.Count > 0)
						{
							CheckObjects ObjectCheck = CheckTheObjects(list, grouporowner, GROUPOROWNER, steamid, confirmation, Id, checkclaim);
							List<Transform> checklist = ObjectCheck.checklist;
							List<Transform> cprotected = ObjectCheck.cprotected;
							if (checklist.Count == 0)
							{
								if (grouporowner)
								{
									switch (GROUPOROWNER)
									{
										case GROUPOROWNER.GROUP:
											UnturnedChat.Say(caller, Instance.Translate("Failgroup", new object[] { radius }));
											break;
										case GROUPOROWNER.OWNER:
											UnturnedChat.Say(caller, Instance.Translate("Failowner", new object[] { radius }));
											break;
									}
									return;
								}
								if (Id != null)
								{
									UnturnedChat.Say(caller, Instance.Translate("Failid", new object[] { radius }));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("Fail", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								Confirmation[caller.Id] = checklist;
								ConfirmationClean[caller.Id] = clean;
								ConfirmationGOO[caller.Id] = GROUPOROWNER;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								UnturnedChat.Say(caller, Instance.Translate("Confirm", new object[] { radius, checklist.Count }));
							}
							else
							{
								if (grouporowner)
								{
									switch (GROUPOROWNER)
									{
										case GROUPOROWNER.GROUP:
											UnturnedChat.Say(caller, Instance.Translate("Successbsgroup", new object[] { radius, checklist.Count }));
											break;
										case GROUPOROWNER.OWNER:
											UnturnedChat.Say(caller, Instance.Translate("Successbsowner", new object[] { radius, checklist.Count }));
											break;
									}
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("Successbs", new object[] { radius, checklist.Count }));
							}
						}
						else
						{
							if (grouporowner)
							{
								switch (GROUPOROWNER)
								{
									case GROUPOROWNER.GROUP:
										UnturnedChat.Say(caller, Instance.Translate("Failgroup", new object[] { radius }));
										break;
									case GROUPOROWNER.OWNER:
										UnturnedChat.Say(caller, Instance.Translate("Failowner", new object[] { radius }));
										break;
								}
								return;
							}
							if (Id != null)
							{
								UnturnedChat.Say(caller, Instance.Translate("Failid", new object[] { radius }));
								return;
							}
							UnturnedChat.Say(caller, Instance.Translate("Fail", new object[] { radius }));
						}
						break;
					case CLEAN.B:
						transform.position = p.Position;
						list = FindAllB().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
						if (list.Count > 0)
						{
							CheckObjects ObjectCheck = CheckTheObjects(list, grouporowner, GROUPOROWNER, steamid, confirmation, Id, checkclaim);
							List<Transform> checklist = ObjectCheck.checklist;
							List<Transform> cprotected = ObjectCheck.cprotected;
							if (checklist.Count == 0)
							{
								if (Id != null)
								{
									UnturnedChat.Say(caller, Instance.Translate("Failbid", new object[] { radius }));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("Failb", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								Confirmation[caller.Id] = checklist;
								ConfirmationClean[caller.Id] = clean;
								ConfirmationGOO[caller.Id] = GROUPOROWNER;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								UnturnedChat.Say(caller, Instance.Translate("Confirm", new object[] { radius, checklist.Count }));
							}
							else
							{
								UnturnedChat.Say(caller, Instance.Translate("Successb", new object[] { radius, checklist.Count }));
							}

						}
						else
						{
							if (Id != null)
							{
								UnturnedChat.Say(caller, Instance.Translate("Failbid", new object[] { radius }));
								return;
							}
							UnturnedChat.Say(caller, Instance.Translate("Failb", new object[] { radius }));
						}
						break;
					case CLEAN.S:
						transform.position = p.Position;
						list = FindAllS().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
						if (list.Count > 0)
						{
							CheckObjects ObjectCheck = CheckTheObjects(list, grouporowner, GROUPOROWNER, steamid, confirmation, Id, checkclaim);
							List<Transform> checklist = ObjectCheck.checklist;
							List<Transform> cprotected = ObjectCheck.cprotected;
							if (checklist.Count == 0)
							{
								if (Id != null)
								{
									UnturnedChat.Say(caller, Instance.Translate("Failsid", new object[] { radius }));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("Fails", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								Confirmation[caller.Id] = checklist;
								ConfirmationClean[caller.Id] = clean;
								ConfirmationGOO[caller.Id] = GROUPOROWNER;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								UnturnedChat.Say(caller, Instance.Translate("Confirm", new object[] { radius, checklist.Count }));
							}
							else
							{

								UnturnedChat.Say(caller, Instance.Translate("Successs", new object[] { radius, checklist.Count }));
							}

						}
						else
						{
							if (Id != null)
							{
								UnturnedChat.Say(caller, Instance.Translate("Failsid", new object[] { radius }));
								return;
							}
							UnturnedChat.Say(caller, Instance.Translate("Fails", new object[] { radius }));
						}
						break;
					case CLEAN.UNCB:
						transform.position = p.Position;
						list = FindAllB().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
						if (list.Count > 0)
						{
							CheckObjects ObjectCheck = CheckTheObjects(list, grouporowner, GROUPOROWNER, steamid, confirmation, Id, checkclaim);
							List<Transform> checklist = ObjectCheck.checklist;
							List<Transform> cprotected = ObjectCheck.cprotected;
							if (checklist.Count == 0)
							{
								if (Id != null)
								{
									UnturnedChat.Say(caller, Instance.Translate("Failuncbid", new object[] { radius}));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("Failuncb", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								Confirmation[caller.Id] = checklist;
								ConfirmationClean[caller.Id] = clean;
								ConfirmationGOO[caller.Id] = GROUPOROWNER;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								UnturnedChat.Say(caller, Instance.Translate("Confirm", new object[] { radius, checklist.Count }));
							}
							else
							{
								if (Id != null)
								{
									if (list.Count == cprotected.Count)
									{
										UnturnedChat.Say(caller, Instance.Translate("FoundButClaimeduncbid", new object[] { radius, cprotected.Count }));
										return;
									}
									UnturnedChat.Say(caller, Instance.Translate("SuccessClaimeduncbid", new object[] { radius, checklist.Count }));
									return;
								}
								if (list.Count == cprotected.Count)
								{
									UnturnedChat.Say(caller, Instance.Translate("FoundButClaimeduncb", new object[] { radius, cprotected.Count }));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("SuccessClaimeduncb", new object[] { radius, checklist.Count }));
								return;
							}
						}
						else
						{
							if (Id != null)
							{
								UnturnedChat.Say(caller, Instance.Translate("Failuncbid", new object[] { radius }));
								return;
							}
							UnturnedChat.Say(caller, Instance.Translate("Failuncb", new object[] { radius }));
						}
						break;
					case CLEAN.UNCS:
						transform.position = p.Position;
						list = FindAllS().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
						if (list.Count > 0)
						{
							CheckObjects ObjectCheck = CheckTheObjects(list, grouporowner, GROUPOROWNER, steamid, confirmation, Id, checkclaim);
							List<Transform> checklist = ObjectCheck.checklist;
							List<Transform> cprotected = ObjectCheck.cprotected;
							if (checklist.Count == 0)
							{
								if (Id != null)
								{
									UnturnedChat.Say(caller, Instance.Translate("Failuncsid", new object[] { radius }));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("Failuncs", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								Confirmation[caller.Id] = checklist;
								ConfirmationClean[caller.Id] = clean;
								ConfirmationGOO[caller.Id] = GROUPOROWNER;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								UnturnedChat.Say(caller, Instance.Translate("Confirm", new object[] { radius, checklist.Count }));
							}
							else
							{
								if (Id != null)
								{
									if (list.Count == cprotected.Count)
									{
										UnturnedChat.Say(caller, Instance.Translate("FoundButClaimeduncsid", new object[] { radius, cprotected.Count }));
										return;
									}
									UnturnedChat.Say(caller, Instance.Translate("SuccessClaimeduncsid", new object[] { radius, checklist.Count }));
									return;
								}
								if (list.Count == cprotected.Count)
								{
									UnturnedChat.Say(caller, Instance.Translate("FoundButClaimeduncs", new object[] { radius, cprotected.Count }));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("SuccessClaimeduncs", new object[] { radius, checklist.Count }));
								return;
							}
						}
						else
						{
							if (Id != null)
							{
								UnturnedChat.Say(caller, Instance.Translate("Failuncsid", new object[] { radius }));
								return;
							}
							UnturnedChat.Say(caller, Instance.Translate("Failuncs", new object[] { radius }));
						}
						break;
					case CLEAN.UNCBS:
						transform.position = p.Position;
						list = FindAll().FindAll((Transform x) => Vector3.Distance(transform.position, x.position) <= radius);
						if (list.Count > 0)
						{
							CheckObjects ObjectCheck = CheckTheObjects(list, grouporowner, GROUPOROWNER, steamid, confirmation, Id, checkclaim);
							List<Transform> checklist = ObjectCheck.checklist;
							List<Transform> cprotected = ObjectCheck.cprotected;
							if (checklist.Count == 0)
							{
								if (Id != null)
								{
									UnturnedChat.Say(caller, Instance.Translate("Failuncbsid", new object[] { radius }));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("Failuncbs", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								Confirmation[caller.Id] = checklist;
								ConfirmationClean[caller.Id] = clean;
								ConfirmationGOO[caller.Id] = GROUPOROWNER;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								UnturnedChat.Say(caller, Instance.Translate("Confirm", new object[] { radius, checklist.Count }));
							}
							else
							{
								if (Id != null)
								{
									if (list.Count == cprotected.Count)
									{
										UnturnedChat.Say(caller, Instance.Translate("FoundButClaimeduncbsid", new object[] { radius, cprotected.Count }));
										return;
									}
									UnturnedChat.Say(caller, Instance.Translate("SuccessClaimeduncbsid", new object[] { radius, checklist.Count }));
									return;
								}
								if (list.Count == cprotected.Count)
								{
									UnturnedChat.Say(caller, Instance.Translate("FoundButClaimeduncbs", new object[] { radius, cprotected.Count }));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("SuccessClaimeduncbs", new object[] { radius, checklist.Count }));
								return;
							}
						}
						else
						{
							if (Id != null)
							{
								UnturnedChat.Say(caller, Instance.Translate("Failuncbsid", new object[] { radius }));
								return;
							}
							UnturnedChat.Say(caller, Instance.Translate("Failuncbs", new object[] { radius }));
						}
						break;
					case CLEAN.ITEM:
						UnturnedChat.Say(caller, Instance.Translate("Successi", new object[] { radius }));
						ItemManager.askClearAllItems();
						break;
					case CLEAN.V:
						transform.position = p.Position;
						vehiclesInRadius = Vehicles.Where(v => (transform.position - v.transform.position).sqrMagnitude <= Mathf.Pow(radius, 2)).ToList();
						VehiclesFound = CheckTheVehicles(vehiclesInRadius, confirmation, Id);
						if (VehiclesFound.Count > 0)
						{
							if (VehiclesFound.Count == 0)
							{
								if (Id != null)
								{
									UnturnedChat.Say(caller, Instance.Translate("Failvid", new object[] { radius, }));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("Failv", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								ConfirmationClean[caller.Id] = clean;
								ConfirmationVehicle[caller.Id] = VehiclesFound;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								UnturnedChat.Say(caller, Instance.Translate("Confirm", new object[] { radius, VehiclesFound.Count }));
							}
							else
							{
								UnturnedChat.Say(caller, Translate("Successv", radius, VehiclesFound.Count));
							}
						}
						else
						{
							if (Id != null)
							{
								UnturnedChat.Say(caller, Instance.Translate("Failvid", new object[] { radius }));
								return;
							}
							UnturnedChat.Say(caller, Translate("Failv", radius));
						}
						break;
					case CLEAN.EV:
						transform.position = p.Position;
						vehiclesInRadius = Vehicles.Where(v => (transform.position - v.transform.position).sqrMagnitude <= Mathf.Pow(radius, 2) && !v.anySeatsOccupied).ToList();
						VehiclesFound = CheckTheVehicles(vehiclesInRadius, confirmation, Id);
						if (VehiclesFound.Count > 0)
						{
							if (VehiclesFound.Count == 0)
							{
								if (Id != null)
								{
									UnturnedChat.Say(caller, Instance.Translate("Failevid", new object[] { radius, }));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("Failev", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								ConfirmationClean[caller.Id] = clean;
								ConfirmationVehicle[caller.Id] = VehiclesFound;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								UnturnedChat.Say(caller, Instance.Translate("Confirm", new object[] { radius, VehiclesFound.Count }));
							}
							else
							{
								UnturnedChat.Say(caller, Translate("Successev", radius, VehiclesFound.Count));
							}
						}
						else
						{
							if (Id != null)
							{
								UnturnedChat.Say(caller, Instance.Translate("Failevid", new object[] { radius }));
								return;
							}
							UnturnedChat.Say(caller, Translate("Failev", radius));
						}
						break;
					case CLEAN.LV:
						transform.position = p.Position;
						vehiclesInRadius = Vehicles.Where(v => (transform.position - v.transform.position).sqrMagnitude <= Mathf.Pow(radius, 2) && v.isLocked).ToList();
						VehiclesFound = CheckTheVehicles(vehiclesInRadius, confirmation, Id);
						if (VehiclesFound.Count > 0)
						{
							if (VehiclesFound.Count == 0)
							{
								if (Id != null)
								{
									UnturnedChat.Say(caller, Instance.Translate("Faillvid", new object[] { radius, }));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("Faillv", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								ConfirmationClean[caller.Id] = clean;
								ConfirmationVehicle[caller.Id] = VehiclesFound;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								UnturnedChat.Say(caller, Instance.Translate("Confirm", new object[] { radius, VehiclesFound.Count }));
							}
							else
							{

								UnturnedChat.Say(caller, Translate("Successlv", radius, VehiclesFound.Count));
							}
						}
						else
						{
							if (Id != null)
							{
								UnturnedChat.Say(caller, Instance.Translate("Faillvid", new object[] { radius }));
								return;
							}
							UnturnedChat.Say(caller, Translate("Faillv", radius));
						}
						break;
					case CLEAN.LEV:
						transform.position = p.Position;
						vehiclesInRadius = Vehicles.Where(v => (transform.position - v.transform.position).sqrMagnitude <= Mathf.Pow(radius, 2) && v.isLocked && !v.anySeatsOccupied).ToList();
						VehiclesFound = CheckTheVehicles(vehiclesInRadius, confirmation, Id);
						if (VehiclesFound.Count > 0)
						{
							if (VehiclesFound.Count == 0)
							{
								if (Id != null)
								{
									UnturnedChat.Say(caller, Instance.Translate("Faillevid", new object[] { radius, }));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("Faillev", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								ConfirmationClean[caller.Id] = clean;
								ConfirmationVehicle[caller.Id] = VehiclesFound;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								UnturnedChat.Say(caller, Instance.Translate("Confirm", new object[] { radius, VehiclesFound.Count }));
							}
							else
							{
								UnturnedChat.Say(caller, Translate("Successlev", radius, VehiclesFound.Count));

							}
						}
						else
						{
							if (Id != null)
							{
								UnturnedChat.Say(caller, Instance.Translate("Faillevid", new object[] { radius }));
								return;
							}
							UnturnedChat.Say(caller, Translate("Faillev", radius));
						}
						break;
					case CLEAN.ULV:
						transform.position = p.Position;
						vehiclesInRadius = Vehicles.Where(v => (transform.position - v.transform.position).sqrMagnitude <= Mathf.Pow(radius, 2) && !v.isLocked).ToList();
						VehiclesFound = CheckTheVehicles(vehiclesInRadius, confirmation, Id);
						if (VehiclesFound.Count > 0)
						{
							if (VehiclesFound.Count == 0)
							{
								if (Id != null)
								{
									UnturnedChat.Say(caller, Instance.Translate("Failulvid", new object[] { radius, }));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("Failulv", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								ConfirmationClean[caller.Id] = clean;
								ConfirmationVehicle[caller.Id] = VehiclesFound;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								UnturnedChat.Say(caller, Instance.Translate("Confirm", new object[] { radius, VehiclesFound.Count }));
							}
							else
							{
								UnturnedChat.Say(caller, Translate("Successulv", radius, VehiclesFound.Count));

							}
						}
						else
						{
							if (Id != null)
							{
								UnturnedChat.Say(caller, Instance.Translate("Failulvid", new object[] { radius }));
								return;
							}
							UnturnedChat.Say(caller, Translate("Failulv", radius));
						}
						break;
					case CLEAN.ULEV:
						transform.position = p.Position;
						vehiclesInRadius = Vehicles.Where(v => (transform.position - v.transform.position).sqrMagnitude <= Mathf.Pow(radius, 2) && !v.isLocked && !v.anySeatsOccupied).ToList();
						VehiclesFound = CheckTheVehicles(vehiclesInRadius, confirmation, Id);
						if (VehiclesFound.Count > 0)
						{
							if (VehiclesFound.Count == 0)
							{
								if (Id != null)
								{
									UnturnedChat.Say(caller, Instance.Translate("Failulevid", new object[] { radius, }));
									return;
								}
								UnturnedChat.Say(caller, Instance.Translate("Failulev", new object[] { radius }));
								return;
							}
							if (confirmation)
							{
								ConfirmationClean[caller.Id] = clean;
								ConfirmationVehicle[caller.Id] = VehiclesFound;
								ConfirmationRadius[caller.Id] = radius;
								ConfirmationIds[caller.Id] = Id;
								UnturnedChat.Say(caller, Instance.Translate("Confirm", new object[] { radius, VehiclesFound.Count }));
							}
							else
							{
								UnturnedChat.Say(caller, Translate("Successulev", radius, VehiclesFound.Count));

							}
						}
						else
						{
							if (Id != null)
							{
								UnturnedChat.Say(caller, Instance.Translate("Failulevid", new object[] { radius }));
								return;
							}
							UnturnedChat.Say(caller, Translate("Failulev", radius));
						}
						break;
				}
			}
		}
		public List<InteractableVehicle> CheckTheVehicles(List<InteractableVehicle> vehiclesInRadius, bool confirm, uint[] Id)
        {
			List <InteractableVehicle> VehiclesFound = new List<InteractableVehicle>();
            foreach (InteractableVehicle vehicle in vehiclesInRadius)
            {
				if(Id != null)
                {
					uint.TryParse(vehicle.transform.name, out uint tname);
                    if (Id.Contains(tname))
                    {
						VehiclesFound.Add(vehicle);
						if (!confirm)
						{
							VehicleManager.askVehicleDestroy(vehicle);
						}
					}
                }
                else
                {
					VehiclesFound.Add(vehicle);
					if (!confirm)
                    {
						VehicleManager.askVehicleDestroy(vehicle);
                    }
                }
            }
			return VehiclesFound;
		}
		public class CheckObjects
        {
			public List<Transform> checklist;
			public List<Transform> cprotected;
		}
		public CheckObjects CheckTheObjects(List<Transform> list, bool grouporowner, GROUPOROWNER GROUPOROWNER, string steamid, bool confirmation, uint[] Id, bool checkclaim)
        {
			List<Transform> checklist = new List<Transform>();
			List<Transform> cprotected = new List<Transform>();
			foreach (Transform transform in list)
			{
				if (grouporowner)
				{
					BarricadeData datab = null;
					StructureData datas = null;
					var owneridb = new ulong();
					var ownerids = new ulong();
					var groupidb = new ulong();
					var groupids = new ulong();
					if (transform.CompareTag("Barricade"))
                    {
						datab = BarricadeManager.FindBarricadeByRootTransform(transform).GetServersideData();
						owneridb = datab.owner;
						groupidb = datab.group;
					}
					else if (transform.CompareTag("Structure"))
                    {
						datas = StructureManager.FindStructureByRootTransform(transform).GetServersideData();
						ownerids = datab.owner;
						groupids = datas.group;
					}
					switch (GROUPOROWNER)
					{
						case GROUPOROWNER.OWNER:
							if (owneridb.ToString() == steamid || ownerids.ToString() == steamid)
							{
								checklist.Add(transform);
								if (!confirmation)
								{
									DamageBarricadesAndStructures(transform);
								}
							}
							break;
						case GROUPOROWNER.GROUP:
							if (groupidb.ToString() == steamid || groupids.ToString() == steamid)
							{
								checklist.Add(transform);
								if (!confirmation)
								{
									DamageBarricadesAndStructures(transform);
								}
							}
							break;
					}
				}
				else
				{
                    if (checkclaim)
                    {
                        if (Id != null)
                        {
							uint.TryParse(transform.name, out uint tname);
							if (Id.Contains(tname))
							{
                                if (IsClaim(transform.position))
                                {
									checklist.Add(transform);
									if (!confirmation)
									{
										DamageBarricadesAndStructures(transform);
									}
								}
                                else
                                {
									cprotected.Add(transform);
                                }
							}
						}
                        else
                        {
							if (IsClaim(transform.position))
							{
								checklist.Add(transform);
								if (!confirmation)
								{
									DamageBarricadesAndStructures(transform);
								}
							}
                            else
                            {
								cprotected.Add(transform);
                            }
						}
                    }
                    else
                    {
						if (Id != null)
                        {
							uint.TryParse(transform.name, out uint tname);
                            if (Id.Contains(tname))
                            {
								checklist.Add(transform);
								if (!confirmation)
								{
									DamageBarricadesAndStructures(transform);
								}
							}
                        }
                        else
                        {
							checklist.Add(transform);
							if (!confirmation)
							{
								DamageBarricadesAndStructures(transform);
							}
						}
					}
				}
			}
			return new CheckObjects { checklist = checklist, cprotected = cprotected};
		}
		public void DamageBarricadesAndStructures(Transform transform)
        {
			BarricadeManager.damage(transform, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
			StructureManager.damage(transform, Vector3.zero, 30000f, 1f, true, default(CSteamID), EDamageOrigin.Unknown);
		}

		[RocketCommand("clean", "/clean <UncBS|UncB|UncS|BS|B|S|EV|V|UnlV|UnlEV|lV|lEV|I> (Radius) (ID)", "/clean <UncBS|UncB|UncS|BS|B|S|EV|V|UnlV|UnlEV|lV|lEV|I> (Radius) (ID)", AllowedCaller.Both)]
		public void Execute(IRocketPlayer caller, string[] commands)
		{
			bool confirm = Configuration.Instance.AskForConfirmBeforeCleaning;
			if (commands.Length < 1)
			{
				UnturnedChat.Say(caller, "Usage: /clean <UncBS|UncB|UncS|BS|B|S|EV|V|I> (Radius) (ID)");
				return;
			}
			var req = commands[0].ToLower();
			if (ConfirmationClean.ContainsKey(caller.Id))
            {
				if (req != "confirm" && req != "abort")
                {
					if (ConfirmationClean[caller.Id] == CLEAN.B || ConfirmationClean[caller.Id] == CLEAN.S || ConfirmationClean[caller.Id] == CLEAN.BS || ConfirmationClean[caller.Id] == CLEAN.UNCB || ConfirmationClean[caller.Id] == CLEAN.UNCS || ConfirmationClean[caller.Id] == CLEAN.UNCBS)
					{
						UnturnedChat.Say(caller, Instance.Translate("Pending", new object[] { Confirmation[caller.Id].Count, Enum.GetName(typeof(CLEAN), ConfirmationClean[caller.Id]) }));
					}
					else if (ConfirmationClean[caller.Id] == CLEAN.V || ConfirmationClean[caller.Id] == CLEAN.EV || ConfirmationClean[caller.Id] == CLEAN.LV || ConfirmationClean[caller.Id] == CLEAN.LEV || ConfirmationClean[caller.Id] == CLEAN.ULV || ConfirmationClean[caller.Id] == CLEAN.ULEV)
					{
						UnturnedChat.Say(caller, Instance.Translate("Pending", new object[] { ConfirmationVehicle[caller.Id].Count, Enum.GetName(typeof(CLEAN), ConfirmationClean[caller.Id]) }));
					}
					return;
				}
                else if (req == "confirm")
                {
					if (ConfirmationClean[caller.Id] == CLEAN.B || ConfirmationClean[caller.Id] == CLEAN.S || ConfirmationClean[caller.Id] == CLEAN.BS || ConfirmationClean[caller.Id] == CLEAN.UNCB || ConfirmationClean[caller.Id] == CLEAN.UNCS || ConfirmationClean[caller.Id] == CLEAN.UNCBS)
					{
						CheckTheObjects(Confirmation[caller.Id], false, GROUPOROWNER.NONE, null, false, null, false);
						bool GOO = false;
						bool ID = false;
                        if (ConfirmationGOO[caller.Id] != GROUPOROWNER.NONE)
                        {
							GOO = true;
                        }
						if (ConfirmationIds.ContainsKey(caller.Id))
                        {
							ID = true;
                        }
                        switch (ConfirmationClean[caller.Id])
                        {
							case CLEAN.B:
                                if (ID)
                                {
									UnturnedChat.Say(caller, Instance.Translate("Successbid", new object[] { ConfirmationRadius[caller.Id], Confirmation[caller.Id].Count }));
									break;
                                }
								UnturnedChat.Say(caller, Instance.Translate("Successb", new object[] { ConfirmationRadius[caller.Id], Confirmation[caller.Id].Count }));
								break;
							case CLEAN.S:
								if (ID)
								{
									UnturnedChat.Say(caller, Instance.Translate("Successsid", new object[] { ConfirmationRadius[caller.Id], Confirmation[caller.Id].Count }));
									break;
								}
								UnturnedChat.Say(caller, Instance.Translate("Successs", new object[] { ConfirmationRadius[caller.Id], Confirmation[caller.Id].Count }));
								break;
							case CLEAN.BS:
                                if (GOO)
                                {
                                    switch (ConfirmationGOO[caller.Id])
                                    {
										case GROUPOROWNER.GROUP:
											UnturnedChat.Say(caller, Instance.Translate("Successbsgroup", new object[] { ConfirmationRadius[caller.Id], Confirmation[caller.Id].Count }));
											break;
										case GROUPOROWNER.OWNER:
											UnturnedChat.Say(caller, Instance.Translate("Successbsowner", new object[] { ConfirmationRadius[caller.Id], Confirmation[caller.Id].Count }));
											break;
                                    }
									return;
								}
								if (ID)
								{
									UnturnedChat.Say(caller, Instance.Translate("Successbsid", new object[] { ConfirmationRadius[caller.Id], Confirmation[caller.Id].Count }));
									break;
								}
								UnturnedChat.Say(caller, Instance.Translate("Successbs", new object[] { ConfirmationRadius[caller.Id], Confirmation[caller.Id].Count }));
								break;
							case CLEAN.UNCB:
								if (ID)
								{
									UnturnedChat.Say(caller, Instance.Translate("SuccessClaimeduncbid", new object[] { ConfirmationRadius[caller.Id], Confirmation[caller.Id].Count }));
									break;
								}
								UnturnedChat.Say(caller, Instance.Translate("SuccessClaimeduncb", new object[] { ConfirmationRadius[caller.Id], Confirmation[caller.Id].Count }));
								break;
							case CLEAN.UNCS:
								if (ID)
								{
									UnturnedChat.Say(caller, Instance.Translate("SuccessClaimeduncsid", new object[] { ConfirmationRadius[caller.Id], Confirmation[caller.Id].Count }));
									break;
								}
								UnturnedChat.Say(caller, Instance.Translate("SuccessClaimeduncs", new object[] { ConfirmationRadius[caller.Id], Confirmation[caller.Id].Count }));
								break;
							case CLEAN.UNCBS:
								if (ID)
								{
									UnturnedChat.Say(caller, Instance.Translate("SuccessClaimeduncbsid", new object[] { ConfirmationRadius[caller.Id], Confirmation[caller.Id].Count }));
									break;
								}
								UnturnedChat.Say(caller, Instance.Translate("SuccessClaimeduncbs", new object[] { ConfirmationRadius[caller.Id], Confirmation[caller.Id].Count }));
								break;
                        }
						if (Confirmation.ContainsKey(caller.Id))
						{
							Confirmation.Remove(caller.Id);
						}
						if (ConfirmationClean.ContainsKey(caller.Id))
						{
							ConfirmationClean.Remove(caller.Id);
						}
						if (ConfirmationVehicle.ContainsKey(caller.Id))
						{
							ConfirmationVehicle.Remove(caller.Id);
						}
						if (ConfirmationGOO.ContainsKey(caller.Id))
						{
							ConfirmationGOO.Remove(caller.Id);
						}
						if (ConfirmationRadius.ContainsKey(caller.Id))
						{
							ConfirmationRadius.Remove(caller.Id);
						}
					}
					else if (ConfirmationClean[caller.Id] == CLEAN.V || ConfirmationClean[caller.Id] == CLEAN.EV || ConfirmationClean[caller.Id] == CLEAN.LV || ConfirmationClean[caller.Id] == CLEAN.LEV || ConfirmationClean[caller.Id] == CLEAN.ULV || ConfirmationClean[caller.Id] == CLEAN.ULEV)
					{
						bool ID = false;
						if (ConfirmationIds.ContainsKey(caller.Id))
						{
							ID = true;
						}
						CheckTheVehicles(ConfirmationVehicle[caller.Id], false, null);
						switch (ConfirmationClean[caller.Id])
						{
							case CLEAN.V:
								if (ID)
								{
									UnturnedChat.Say(caller, Instance.Translate("Successvid", new object[] { ConfirmationRadius[caller.Id], ConfirmationVehicle[caller.Id].Count }));
									break;
								}
								UnturnedChat.Say(caller, Instance.Translate("Successv", new object[] { ConfirmationRadius[caller.Id], ConfirmationVehicle[caller.Id].Count }));
								break;
							case CLEAN.EV:
								if (ID)
								{
									UnturnedChat.Say(caller, Instance.Translate("Successevid", new object[] { ConfirmationRadius[caller.Id], ConfirmationVehicle[caller.Id].Count }));
									break;
								}
								UnturnedChat.Say(caller, Instance.Translate("Successev", new object[] { ConfirmationRadius[caller.Id], ConfirmationVehicle[caller.Id].Count }));
								break;
							case CLEAN.LV:
								if (ID)
								{
									UnturnedChat.Say(caller, Instance.Translate("Successlvid", new object[] { ConfirmationRadius[caller.Id], ConfirmationVehicle[caller.Id].Count }));
									break;
								}
								UnturnedChat.Say(caller, Instance.Translate("Successlv", new object[] { ConfirmationRadius[caller.Id], ConfirmationVehicle[caller.Id].Count }));
								break;
							case CLEAN.LEV:
								if (ID)
								{
									UnturnedChat.Say(caller, Instance.Translate("Successlevid", new object[] { ConfirmationRadius[caller.Id], ConfirmationVehicle[caller.Id].Count }));
									break;
								}
								UnturnedChat.Say(caller, Instance.Translate("Successlev", new object[] { ConfirmationRadius[caller.Id], ConfirmationVehicle[caller.Id].Count }));
								break;
							case CLEAN.ULV:
								if (ID)
								{
									UnturnedChat.Say(caller, Instance.Translate("Successlvid", new object[] { ConfirmationRadius[caller.Id], ConfirmationVehicle[caller.Id].Count }));
									break;
								}
								UnturnedChat.Say(caller, Instance.Translate("Successulv", new object[] { ConfirmationRadius[caller.Id], ConfirmationVehicle[caller.Id].Count }));
								break;
							case CLEAN.ULEV:
								if (ID)
								{
									UnturnedChat.Say(caller, Instance.Translate("Successulevid", new object[] { ConfirmationRadius[caller.Id], ConfirmationVehicle[caller.Id].Count }));
									break;
								}
								UnturnedChat.Say(caller, Instance.Translate("Successulev", new object[] { ConfirmationRadius[caller.Id], ConfirmationVehicle[caller.Id].Count }));
								break;
						}
						if (Confirmation.ContainsKey(caller.Id))
						{
							Confirmation.Remove(caller.Id);
						}
						if (ConfirmationClean.ContainsKey(caller.Id))
						{
							ConfirmationClean.Remove(caller.Id);
						}
						if (ConfirmationVehicle.ContainsKey(caller.Id))
						{
							ConfirmationVehicle.Remove(caller.Id);
						}
						if (ConfirmationGOO.ContainsKey(caller.Id))
						{
							ConfirmationGOO.Remove(caller.Id);
						}
						if (ConfirmationRadius.ContainsKey(caller.Id))
						{
							ConfirmationRadius.Remove(caller.Id);
						}
					}
					return;
                }
				if (req == "abort")
                {
                    if (Confirmation.ContainsKey(caller.Id))
                    {
						Confirmation.Remove(caller.Id);
                    }
					if (ConfirmationClean.ContainsKey(caller.Id))
					{
						ConfirmationClean.Remove(caller.Id);
					}
					if (ConfirmationVehicle.ContainsKey(caller.Id))
					{
						ConfirmationVehicle.Remove(caller.Id);
					}
					if (ConfirmationGOO.ContainsKey(caller.Id))
					{
						ConfirmationGOO.Remove(caller.Id);
					}
					if (ConfirmationRadius.ContainsKey(caller.Id))
					{
						ConfirmationRadius.Remove(caller.Id);
					}
					UnturnedChat.Say(caller, Instance.Translate("Aborted"));
				}
				return;
			}
			if (req == "uncbs")
			{
				if (caller is ConsolePlayer)
				{
					if (commands.Length < 2)
					{
						float radius = 9999999999;
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCBS, true);
					}
					else if (commands.Length < 3)
					{
						if (!TryGetIDs(commands[1], out uint[] ids))
						{
							Logger.Log("Id's must be split by using commas. Example \"1091,1092\"");
							return;
						}
						else
						{
							float radius = 9999999999;
							Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCBS, true);
						}
					}
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCBS, true);
					return;
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
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCBS, true);

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
						Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCBS, true);
					}
					return;
				}
			}
			else if (req == "uncb")
			{
				if (caller is ConsolePlayer)
				{
					if (commands.Length < 2)
					{
						float radius = 9999999999;
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCB, true);
					}
					else if (commands.Length < 3)
					{
						if (!TryGetIDs(commands[1], out uint[] ids))
						{
							Logger.Log("Id's must be split by using commas. Example \"1091,1092\"");
							return;
						}
						else
						{
							float radius = 9999999999;
							Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCB, true);
						}
					}
					return;
				}
				UnturnedChat.Say("uncb");
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCB, true);
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
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCB, true);
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
						Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCB, true);
					}
					return;
				}
			}
			else if (req == "uncs")
			{
				if (caller is ConsolePlayer)
				{
					if (commands.Length < 2)
					{
						float radius = 9999999999;
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCS, true);
					}
					else if (commands.Length < 3)
					{
						if (!TryGetIDs(commands[1], out uint[] ids))
						{
							Logger.Log("Id's must be split by using commas. Example \"1091,1092\"");
							return;
						}
						else
						{
							float radius = 9999999999;
							Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCS, true);
						}
					}
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCS, true);
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
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCS, true);
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
						Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.UNCS, true);
					}
					return;
				}
			}
			else if (req == "i")
			{
				if (caller is ConsolePlayer)
				{
					float radius = 9999999999;
					Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.ITEM, false);
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.ITEM, false);
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
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.ITEM, false);
					}
				}
			}
			else if (req == "v")
			{
				if (caller is ConsolePlayer)
				{
					if (commands.Length < 2)
					{
						float radius = 9999999999;
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.V, false);
					}
					else if (commands.Length < 3)
					{
						if (!TryGetIDs(commands[1], out uint[] ids))
						{
							Logger.Log("Id's must be split by using commas. Example \"1091,1092\"");
							return;
						}
						else
						{
							float radius = 9999999999;
							Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.V, false);
						}
					}
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.V, false);
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
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.V, false);
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
						UnturnedChat.Say(caller, "Id's must be split by using commas. Example \"94,57\"");
						return;
					}
					else
					{
						Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.V, false);
					}
					return;
				}
			}
			else if (req == "lv")
			{
				if (caller is ConsolePlayer)
				{
					if (commands.Length < 2)
					{
						float radius = 9999999999;
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.LV, false);
					}
					else if (commands.Length < 3)
					{
						if (!TryGetIDs(commands[1], out uint[] ids))
						{
							Logger.Log("Id's must be split by using commas. Example \"1091,1092\"");
							return;
						}
						else
						{
							float radius = 9999999999;
							Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.LV, false);
						}
					}
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.LV, false);
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
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.LV, false);
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
						UnturnedChat.Say(caller, "Id's must be split by using commas. Example \"94,57\"");
						return;
					}
					else
					{
						Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.LV, false);
					}
					return;
				}
			}
			else if (req == "ulv")
			{
				if (caller is ConsolePlayer)
				{
					if (commands.Length < 2)
					{
						float radius = 9999999999;
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.ULV, false);
					}
					else if (commands.Length < 3)
					{
						if (!TryGetIDs(commands[1], out uint[] ids))
						{
							Logger.Log("Id's must be split by using commas. Example \"1091,1092\"");
							return;
						}
						else
						{
							float radius = 9999999999;
							Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.ULV, false);
						}
					}
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.ULV, false);
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
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.ULV, false);
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
						UnturnedChat.Say(caller, "Id's must be split by using commas. Example \"94,57\"");
						return;
					}
					else
					{
						Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.ULV, false);
					}
					return;
				}
			}
			else if (req == "ev")
			{
				if (caller is ConsolePlayer)
				{
					if (commands.Length < 2)
					{
						float radius = 9999999999;
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.EV, false);
					}
					else if (commands.Length < 3)
					{
						if (!TryGetIDs(commands[1], out uint[] ids))
						{
							Logger.Log("Id's must be split by using commas. Example \"1091,1092\"");
							return;
						}
						else
						{
							float radius = 9999999999;
							Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.EV, false);
						}
					}
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.EV, false);
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
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.EV, false);
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
						UnturnedChat.Say(caller, "Id's must be split by using commas. Example \"94,57\"");
						return;
					}
					else
					{
						Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.EV, false);
					}
					return;
				}
			}
			else if (req == "lev")
			{
				if (caller is ConsolePlayer)
				{
					if (commands.Length < 2)
					{
						float radius = 9999999999;
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.LEV, false);
					}
					else if (commands.Length < 3)
					{
						if (!TryGetIDs(commands[1], out uint[] ids))
						{
							Logger.Log("Id's must be split by using commas. Example \"1091,1092\"");
							return;
						}
						else
						{
							float radius = 9999999999;
							Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.LEV, false);
						}
					}
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.LEV, false);
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
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.LEV, false);
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
						UnturnedChat.Say(caller, "Id's must be split by using commas. Example \"94,57\"");
						return;
					}
					else
					{
						Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.LEV, false);
					}
					return;
				}
			}
			else if (req == "ulev")
			{
				if (caller is ConsolePlayer)
				{
					if (commands.Length < 2)
					{
						float radius = 9999999999;
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.ULEV, false);
					}
					else if (commands.Length < 3)
					{
						if (!TryGetIDs(commands[1], out uint[] ids))
						{
							Logger.Log("Id's must be split by using commas. Example \"1091,1092\"");
							return;
						}
						else
						{
							float radius = 9999999999;
							Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.ULEV, false);
						}
					}
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.ULEV, false);
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
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.ULEV, false);
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
						UnturnedChat.Say(caller, "Id's must be split by using commas. Example \"94,57\"");
						return;
					}
					else
					{
						Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.ULEV, false);
					}
					return;
				}
			}
			else if (req == "bs")
			{
				if (caller is ConsolePlayer)
				{
					if (commands.Length < 2)
					{
						float radius = 9999999999;
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.BS, false);
					}
					else if (commands.Length < 3)
					{
						if (!TryGetIDs(commands[1], out uint[] ids))
						{
							if (commands[1].ToLower() == "group" || commands[1].ToLower() == "owner")
							{
								Logger.Log("Usage: /clean bs " + commands[1] + " steamid");
								return;
							}
							Logger.Log("Id's must be split by using commas. Example \"1091,1092\"");
							return;
						}
						else
						{
							float radius = 9999999999;
							Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.BS, false);
						}
					}
					else if (commands.Length < 4)
					{
						if (!TryGetIDs(commands[1], out uint[] ids))
						{
							if (commands[1].ToLower() == "group" || commands[1].ToLower() == "owner")
							{
								if (commands[2] == "0")
								{
									UnturnedChat.Say(caller, "SteamID/GroupID cant be zero!");
									return;
								}
								GROUPOROWNER grouporowner = GROUPOROWNER.NONE;
								if (commands[1].ToLower() == "group")
								{
									grouporowner = GROUPOROWNER.GROUP;
								}
								else if (commands[1].ToLower() == "owner")
								{
									grouporowner = GROUPOROWNER.OWNER;
								}
								float radius = 9999999999;
								Clean(caller, radius, null, true, grouporowner, commands[2], confirm, CLEAN.BS, false);
								return;
							}
							Logger.Log("Id's must be split by using commas. Example \"1091,1092\"");
							return;
						}
						else
						{
							float radius = 9999999999;
							Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.BS, false);
						}
					}
					else if (commands.Length < 5)
					{
						if (!TryGetIDs(commands[1], out uint[] ids))
						{
							Logger.Log("Id's must be split by using commas. Example \"1091,1092\"");
							return;
						}
						else
						{
							float radius = 9999999999;
							Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.BS, false);
						}
					}
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.BS, false);
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
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.BS, false);
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
						if (commands[2].ToLower() == "group" || commands[2].ToLower() == "owner")
						{
							UnturnedChat.Say(caller, "Usage: /clean bs radius" + commands[2] + " steamid");
							return;
						}
						UnturnedChat.Say(caller, "Id's must be split by using commas. Example \"1091,1092\"");
						return;
					}
					else
					{
						Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.BS, false);
					}
					return;
				}
				else if (commands.Length < 5)
				{
					if (!float.TryParse(commands[1], out float radius))
					{
						UnturnedChat.Say(caller, "Wrong Usage. You have to use a number for the radius.");
						return;
					}
					if (!TryGetIDs(commands[2], out uint[] ids))
					{
						if (commands[2].ToLower() == "group" || commands[2].ToLower() == "owner")
						{
                            if (commands[3] == "0")
                            {
								UnturnedChat.Say(caller, "SteamID/GroupID cant be zero!");
								return;
                            }
							GROUPOROWNER grouporowner = GROUPOROWNER.NONE;
							if (commands[2].ToLower() == "group")
							{
								grouporowner = GROUPOROWNER.GROUP;
							}
							else if (commands[2].ToLower() == "owner")
							{
								grouporowner = GROUPOROWNER.OWNER;
							}
							Clean(caller, radius, null, true, grouporowner, commands[3], confirm, CLEAN.BS, false);
							return;
						}
						UnturnedChat.Say(caller, "Id's must be split by using commas. Example \"1091,1092\"");
						return;
					}
					else
					{
						Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.BS, false);
					}
					return;
				}
				else if (commands.Length < 6)
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
						Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.BS, false);
					}
					return;
				}
			}
			else if (req == "b")
			{
				if (caller is ConsolePlayer)
				{
					if (commands.Length < 2)
					{
						float radius = 9999999999;
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.B, false);
					}
					else if (commands.Length < 3)
					{
						if (!TryGetIDs(commands[1], out uint[] ids))
						{
							Logger.Log("Id's must be split by using commas. Example \"1091,1092\"");
							return;
						}
						else
						{
							float radius = 9999999999;
							Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.B, false);
						}
					}
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.B, false);
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
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.B, false);
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
						Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.B, false);
					}
					return;
				}
			}
			else if (req == "s")
			{
				if (caller is ConsolePlayer)
				{
					if (commands.Length < 2)
					{
						float radius = 9999999999;
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.S, false);
					}
					else if (commands.Length < 3)
					{
						if (!TryGetIDs(commands[1], out uint[] ids))
						{
							Logger.Log("Id's must be split by using commas. Example \"1091,1092\"");
							return;
						}
						else
						{
							float radius = 9999999999;
							Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.S, false);
						}
					}
					return;
				}
				if (commands.Length < 2)
				{
					float radius = Configuration.Instance.DefaultRadius;
					Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.S, false);
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
						Clean(caller, radius, null, false, GROUPOROWNER.NONE, null, confirm, CLEAN.S, false);
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
						Clean(caller, radius, ids, false, GROUPOROWNER.NONE, null, confirm, CLEAN.S, false);
					}
					return;
				}
			}
			else
			if (caller is ConsolePlayer)
			{
				Logger.Log("Wrong type. /clean <UncBS|UncB|UncS|BS|B|S|EV|V|UnlV|UnlEV|lV|lEV|I> (Radius) (ID)");
			}
			else
			{
				UnturnedChat.Say(caller, "Wrong type. /clean <UncBS|UncB|UncS|BS|B|S|EV|V|UnlV|UnlEV|lV|lEV|I> (Radius) (ID)");
				return;
			}

		}
		public override TranslationList DefaultTranslations
		{
			get
			{
				TranslationList translationList = new TranslationList();
				translationList.Add("FoundButClaimeduncbs", "\"{1}\" amount of Barricade&Structure found in \"{0}\" radius but was protected therefore not deleted.");
				translationList.Add("SuccessClaimeduncbs", "\"{1}\" amount of unprotected Barricade&Structure found in \"{0}\" radius and deleted.");
				translationList.Add("SuccessClaimeduncb", "\"{1}\" amount of unprotected Barricades found in \"{0}\" radius and deleted.");
				translationList.Add("FoundButClaimeduncb", "\"{1}\" amount of Barricades found in \"{0}\" radius but was protected therefore not deleted.");
				translationList.Add("SuccessClaimeduncs", "\"{1}\" amount of unprotected Structures found in \"{0}\" radius and deleted.");
				translationList.Add("FoundButClaimeduncs", "\"{1}\" amount of Structures found in \"{0}\" radius but was protected therefore not deleted.");
				translationList.Add("Successev", "\"{1}\" amount of Empty Vehicles found in \"{0}\" radius and deleted.");
				translationList.Add("Successv", "\"{1}\" amount of Vehicles found in \"{0}\" radius and deleted.");
				translationList.Add("Successulv", "\"{1}\" amount of Unlocked Vehicles found in \"{0}\" radius and deleted.");
				translationList.Add("Successulev", "\"{1}\" amount of Unlocked Empty Vehicles found in \"{0}\" radius and deleted.");
				translationList.Add("Successlv", "\"{1}\" amount of Locked Vehicles found in \"{0}\" radius and deleted.");
				translationList.Add("Successlev", "\"{1}\" amount of Locked Empty Vehicles found in \"{0}\" radius and deleted.");
				translationList.Add("Successbs", "\"{1}\" amount of Barricade&Structure found in \"{0}\" radius and deleted.");
				translationList.Add("Successbsgroup", "\"{1}\" amount of Barricade&Structure found in \"{0}\" radius with the specified group id and deleted.");
				translationList.Add("Successbsowner", "\"{1}\" amount of Barricade&Structure found in \"{0}\" radius with the specified owner id and deleted.");
				translationList.Add("Successb", "\"{1}\" amount of Barricades found in \"{0}\" radius and deleted.");
				translationList.Add("Successs", "\"{1}\" amount of Structures found in \"{0}\" radius and deleted.");
				translationList.Add("Successi", "Any dropped items found in \"{0}\" radius were deleted.");
				translationList.Add("Failuncbid", "There arent any unprotected Barricades with the specified id in \"{0}\" radius.");
				translationList.Add("Failuncs", "There arent any unprotected Structures in \"{0}\" radius.");
				translationList.Add("Failuncsid", "There arent any unprotected Structures with the specified id in \"{0}\" radius.");
				translationList.Add("Failuncb", "There arent any unprotected Barricades in \"{0}\" radius.");
				translationList.Add("Failuncbsid", "There arent any unprotected Barricades or Structures with the specified id in \"{0}\" radius.");
				translationList.Add("Failuncbs", "There arent any unprotected Barricades or Structures in \"{0}\" radius.");
				translationList.Add("Failid", "There arent any Barricades or Structures with the specified id in \"{0}\" radius.");
				translationList.Add("Fail", "There arent any Barricades or Structures in \"{0}\" radius.");
				translationList.Add("Failb", "There arent any Barricades in \"{0}\" radius.");
				translationList.Add("Failbid", "There arent any Barricades with the specified id in \"{0}\" radius.");
				translationList.Add("Fails", "There arent any Structures in \"{0}\" radius.");
				translationList.Add("Failsid", "There arent any Structures with the specified id in \"{0}\" radius.");
				translationList.Add("Failgroup", "There arent any Barricades or Structures in \"{0}\" radius with the specified group id.");
				translationList.Add("Failowner", "There arent any Barricades or Structures in \"{0}\" radius with the specified owner id.");
				translationList.Add("Failv", "There arent any Vehicles in \"{0}\" radius.");
				translationList.Add("Failev", "There arent any Empty Vehicles in \"{0}\" radius.");
				translationList.Add("Failulv", "There arent any Unlocked Vehicles in \"{0}\" radius.");
				translationList.Add("Failulev", "There arent any Unlocked Empty Vehicles in \"{0}\" radius.");
				translationList.Add("Faillv", "There arent any Locked Vehicles in \"{0}\" radius.");
				translationList.Add("Faillev", "There arent any Locked Empty Vehicles in \"{0}\" radius.");
				translationList.Add("FoundButClaimeduncbsid", "\"{1}\" amount of Barricade&Structure with specified id found in \"{0}\" radius but was protected therefore not deleted.");
				translationList.Add("SuccessClaimeduncbsid", "\"{1}\" amount of unprotected Barricade&Structure with specified id found in \"{0}\" radius and deleted.");
				translationList.Add("SuccessClaimeduncbid", "\"{1}\" amount of unprotected Barricades with specified id found in \"{0}\" radius and deleted.");
				translationList.Add("FoundButClaimeduncbid", "\"{1}\" amount of Barricades with specified id found in \"{0}\" radius but was protected therefore not deleted.");
				translationList.Add("SuccessClaimeduncsid", "\"{1}\" amount of unprotected Structures with specified id found in \"{0}\" radius and deleted.");
				translationList.Add("FoundButClaimeduncsid", "\"{1}\" amount of Structures with specified id found in \"{0}\" radius but was protected therefore not deleted.");
				translationList.Add("Successbsid", "\"{1}\" amount of Barricade&Structure with specified id found in \"{0}\" radius and deleted.");
				translationList.Add("Successbid", "\"{1}\" amount of Barricades with specified id found in \"{0}\" radius and deleted.");
				translationList.Add("Successsid", "\"{1}\" amount of Structures with specified id found in \"{0}\" radius and deleted.");
				translationList.Add("Successvid", "\"{1}\" amount of Vehicles with specified id found in \"{0}\" radius and deleted.");
				translationList.Add("Successevid", "\"{1}\" amount of Empty Vehicles with specified id found in \"{0}\" radius and deleted.");
				translationList.Add("Successulvid", "\"{1}\" amount of Unlocked Vehicles with specified id found in \"{0}\" radius and deleted.");
				translationList.Add("Successulevid", "\"{1}\" amount of Unocked Empty Vehicles with specified id found in \"{0}\" radius and deleted.");
				translationList.Add("Successlvid", "\"{1}\" amount of Locked Vehicles with specified id found in \"{0}\" radius and deleted.");
				translationList.Add("Successlevid", "\"{1}\" amount of Locked Empty Vehicles with specified id found in \"{0}\" radius and deleted.");
				translationList.Add("Failevid", "There arent any Empty Vehicles with specified id in \"{0}\" radius.");
				translationList.Add("Failvid", "There arent any Vehicles with specified id in \"{0}\" radius.");
				translationList.Add("Failulvid", "There arent any Unlocked Vehicles with specified id in \"{0}\" radius.");
				translationList.Add("Failulevid", "There arent any Unlocked Empty Vehicles with specified id in \"{0}\" radius.");
				translationList.Add("Faillvid", "There arent any Locked Vehicles with specified id in \"{0}\" radius.");
				translationList.Add("Faillevid", "There arent any Locked Empty Vehicles with specified id in \"{0}\" radius.");
				translationList.Add("Confirm", "\"{1}\" amount of objects found in \"{0}\" radius. Use \"/clean confirm/abort\" You will not be able to undo!");
				translationList.Add("Pending", "You have a pending confirmation for \"{0}\" amount of objects for cleaning \"{1}\". Use \"/clean confirm/abort\" You will not be able to undo!");
				translationList.Add("Aborted", "Aborted the operation.");
				return translationList;
			}
		}
	}
}
