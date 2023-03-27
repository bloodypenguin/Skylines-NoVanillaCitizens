using System;
using System.Reflection;
using ICities;

namespace NoVanillaCitizens
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame || mode == LoadMode.NewGameFromScenario)
            {
                SimulationManager.instance.AddAction(FilterAssets);
            }
        }

        private static void FilterAssets()
        {
            try
            {
                UnityEngine.Debug.Log("No Vanilla Citizens: filtering citizens...");
                FilterCitizens();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("No Vanilla Citizens: an error happened while filtering citizens");
                UnityEngine.Debug.LogException(e);
            }

            try
            {
                UnityEngine.Debug.Log("No Vanilla Citizens: filtering animals...");
                FilterAnimals();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("No Vanilla Citizens: an error happened while filtering animals");
                UnityEngine.Debug.LogException(e);
            }
        }

        private static void FilterCitizens()
        {
            var groupCitizensField =
                typeof(CitizenManager).GetField("m_groupCitizens", BindingFlags.Instance | BindingFlags.NonPublic);
            var groupCitizens = (FastList<ushort>[]) groupCitizensField.GetValue(CitizenManager.instance);
            var newGroupCitizens = new FastList<ushort>[groupCitizens.Length];
            for (var i = 0; i < newGroupCitizens.Length; i++)
            {
                if (groupCitizens[i] is null || groupCitizens[i].m_size == 0)
                {
                    newGroupCitizens[i] = groupCitizens[i];
                    continue;
                }

                var customCitizens = new FastList<ushort>();
                for (var index = 0; index < groupCitizens[i].m_size; index++)
                {
                    var value = groupCitizens[i].m_buffer[index];
                    var citizen = PrefabCollection<CitizenInfo>.GetPrefab(value);
                    if (citizen == null)
                    {
                        continue;
                    }

                    if (!IsWorkshopAsset(citizen))
                    {
                        continue;
                    }
                    customCitizens.Add(value);
                }

                var service = PrefabCollection<CitizenInfo>.GetPrefab(groupCitizens[i].m_buffer[0])?.m_class?.m_service;
                var gender = PrefabCollection<CitizenInfo>.GetPrefab(groupCitizens[i].m_buffer[0])?.m_gender;
                var agePhase = PrefabCollection<CitizenInfo>.GetPrefab(groupCitizens[i].m_buffer[0])?.m_agePhase;
                var subCulture = PrefabCollection<CitizenInfo>.GetPrefab(groupCitizens[i].m_buffer[0])?.m_subCulture;
                if (customCitizens.m_size == 0)
                {
                    newGroupCitizens[i] = groupCitizens[i];
                    UnityEngine.Debug.LogWarning(
                        $"No Vanilla Citizens: Vanilla citizens will be used for service: {service}, gender: {gender}, agePhase: {agePhase}, subCulture: {subCulture}. No custom prefabs found!");
                    continue;
                }
                UnityEngine.Debug.Log(
                $"No Vanilla Citizens: preventing vanilla citizens from spawning for service: {service}, gender: {gender}, agePhase: {agePhase}, subCulture: {subCulture}");
                newGroupCitizens[i] = customCitizens;
            }
            groupCitizensField.SetValue( CitizenManager.instance, newGroupCitizens);
        }
        
        private static void FilterAnimals()
        {
            var groupAnimalsField =
                typeof(CitizenManager).GetField("m_groupAnimals", BindingFlags.Instance | BindingFlags.NonPublic);
            var groupAnimals = (FastList<ushort>[]) groupAnimalsField.GetValue(CitizenManager.instance);
            var newGroupAnimals = new FastList<ushort>[groupAnimals.Length];
            for (var i = 0; i < newGroupAnimals.Length; i++)
            {
                if (groupAnimals[i] is null || groupAnimals[i].m_size == 0)
                {
                    newGroupAnimals[i] = groupAnimals[i];
                    continue;
                }

                var customAnimals = new FastList<ushort>();
                for (var index = 0; index < groupAnimals[i].m_size; index++)
                {
                    var value = groupAnimals[i].m_buffer[index];
                    var animal = PrefabCollection<CitizenInfo>.GetPrefab(value);
                    if (animal == null)
                    {
                        continue;
                    }

                    if (!IsWorkshopAsset(animal))
                    {
                        continue;
                    }
                    customAnimals.Add(value);
                }

                var service = PrefabCollection<CitizenInfo>.GetPrefab(groupAnimals[i].m_buffer[0])?.m_class?.m_service;
                var subCulture = PrefabCollection<CitizenInfo>.GetPrefab(groupAnimals[i].m_buffer[0])?.m_subCulture;
                if (customAnimals.m_size == 0)
                {
                    newGroupAnimals[i] = groupAnimals[i];
                    UnityEngine.Debug.LogWarning(
                        $"No Vanilla Citizens: Vanilla animals will be used for service: {service}, subCulture: {subCulture}. No custom prefabs found!");
                    continue;
                }
                UnityEngine.Debug.Log(
                $"No Vanilla Citizens: preventing vanilla animals from spawning for service: {service}, subCulture: {subCulture}");
                newGroupAnimals[i] = customAnimals;
            }
            groupAnimalsField.SetValue( CitizenManager.instance, newGroupAnimals);
        }

        private static bool IsWorkshopAsset(CitizenInfo citizenInfo)
        {
            return citizenInfo.m_isCustomContent || citizenInfo.name.Contains(".");
        }
    }
}