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
                SimulationManager.instance.AddAction(FilterCitizens);
            }
        }

        private void FilterCitizens()
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

        private static bool IsWorkshopAsset(CitizenInfo citizenInfo)
        {
            return citizenInfo.m_isCustomContent || citizenInfo.name.Contains(".");
        }
    }
}