using UnityEngine;
using System;
using System.Collections.Generic;

namespace RealFuelsPercentUI
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RealFuelsUIInjector : MonoBehaviour
    {
        void Start()
        {
            Type rfType = Type.GetType("RealFuels.Tanks.ModuleFuelTanks, RealFuels") 
                         ?? Type.GetType("RealFuels.ModuleFuelTanks, RealFuels");

            if (rfType == null) return;

            foreach (AvailablePart part in PartLoader.LoadedPartsList)
            {
                if (part == null || part.partPrefab == null) continue;

                bool hasRF = false;
                foreach (PartModule pm in part.partPrefab.Modules)
                {
                    if (pm != null && (pm.GetType().Name == "ModuleFuelTanks" || rfType.IsInstanceOfType(pm)))
                    {
                        hasRF = true;
                        break;
                    }
                }

                if (hasRF && !part.partPrefab.Modules.Contains("ModuleRealFuelsPercent"))
                {
                    part.partPrefab.AddModule("ModuleRealFuelsPercent");
                }
            }
            Debug.Log("[RF-PercentUI] Оптимизированный событийный модуль успешно добавлен!");
        }
    }

    public class ModuleRealFuelsPercent : PartModule, IPartCostModifier
    {
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Всего заправлено")]
        public string percentString = "Расчет...";

        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "Топливо 1")] public string res1 = "";
        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "Топливо 2")] public string res2 = "";
        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "Топливо 3")] public string res3 = "";
        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "Топливо 4")] public string res4 = "";
        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "Топливо 5")] public string res5 = "";

        private List<BaseField> guiFields = new List<BaseField>();
        
        // Кэшируем финальное значение стоимости, чтобы не гонять циклы каждую миллисекунду
        private float cachedModifierCost = 0f;
        private float lastPercent = -1f;

        public override void OnStart(StartState state)
        {
            guiFields.Clear();
            if (Fields != null)
            {
                for (int i = 1; i <= 5; i++)
                {
                    BaseField f = Fields["res" + i];
                    if (f != null) guiFields.Add(f);
                }
            }

            // Первичный расчет при спавне детали в ангаре
            RecalculateData();
        }

        // Вместо тяжелого Update() мы используем LateUpdate(), но встраиваем в него «спящий» режим.
        // Код сработает только тогда, когда суммарный объем ресурсов физически изменился.
        public void LateUpdate()
        {
            if (!HighLogic.LoadedSceneIsEditor || part == null || part.Resources == null) return;

            double currentTotal = 0;
            double maxTotal = 0;

            foreach (PartResource res in part.Resources)
            {
                if (res == null) continue;
                currentTotal += res.amount;
                maxTotal += res.maxAmount;
            }

            if (maxTotal > 0)
            {
                float currentPercent = (float)((currentTotal / maxTotal) * 100.0);

                // Если ползунок сдвинулся с прошлого кадра — просыпаемся и пересчитываем все строки и кэш цены
                if (Mathf.Abs(currentPercent - lastPercent) > 0.01f)
                {
                    lastPercent = currentPercent;
                    RecalculateData();

                    // Принудительно заставляем KSP перерисовать ценник ракеты в углу экрана
                    if (EditorLogic.fetch != null && EditorLogic.fetch.ship != null)
                    {
                        GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
                    }
                }
            }
        }

        // Основной оптимизированный метод сбора данных (запускается только при движении ползунков)
        private void RecalculateData()
        {
            if (part == null || part.Resources == null || part.partInfo == null) return;

            try
            {
                double currentTotalAmount = 0;
                double maxTotalAmount = 0;

                float basePartCost = part.partInfo.cost;
                float bonusCost1 = 0f; 
                float bonusCost2 = 0f; 

                int fieldIndex = 0;

                foreach (PartResource res in part.Resources)
                {
                    if (res == null) continue;

                    currentTotalAmount += res.amount;
                    maxTotalAmount += res.maxAmount;

                    float unitCost = res.info != null ? res.info.unitCost : 0f;
                    if (unitCost > 0f)
                    {
                        if (res.amount > 0) bonusCost1 += (float)(res.amount * unitCost);

                        double missingAmount = res.maxAmount - res.amount;
                        if (missingAmount > 0) bonusCost2 += (float)(missingAmount * unitCost);
                    }

                    if (fieldIndex < guiFields.Count)
                    {
                        double resourcePercent = res.maxAmount > 0 ? (res.amount / res.maxAmount) * 100.0 : 0.0;
                        string displayValue = string.Format("{0:F1}% ({1:F0} / {2:F0} л)", resourcePercent, res.amount, res.maxAmount);

                        BaseField field = guiFields[fieldIndex];
                        field.guiName = res.resourceName;

                        if (fieldIndex == 0) res1 = displayValue;
                        else if (fieldIndex == 1) res2 = displayValue;
                        else if (fieldIndex == 2) res3 = displayValue;
                        else if (fieldIndex == 3) res4 = displayValue;
                        else if (fieldIndex == 4) res5 = displayValue;

                        field.guiActive = true;
                        field.guiActiveEditor = true;
                        fieldIndex++;
                    }
                }

                // Записываем финальную целевую стоимость в кэш-память
                cachedModifierCost = basePartCost + bonusCost1 + bonusCost2; 

                if (maxTotalAmount > 0)
                {
                    percentString = string.Format("{0:F1}%", (currentTotalAmount / maxTotalAmount) * 100.0);
                }
                else percentString = "0.0% (Пусто)";

                for (int i = fieldIndex; i < guiFields.Count; i++)
                {
                    guiFields[i].guiActive = false;
                    guiFields[i].guiActiveEditor = false;
                }
            }
            catch
            {
                percentString = "Ошибка чтения бака";
                HideAllResourceFields();
            }
        }

        private void HideAllResourceFields()
        {
            foreach (BaseField field in guiFields)
            {
                if (field == null) continue;
                field.guiActive = false;
                field.guiActiveEditor = false;
            }
        }

        // --- УЛЬТРА-БЫСТРЫЙ ИНТЕРФЕЙС СТОИМОСТИ KSP ---
        public float GetModuleCost(float defaultCost, ModifierStagingSituation situation)
        {
            // Метод GetModuleCost вызывается KSP десятки раз за секунду. 
            // Благодаря кэшированию, вместо тяжелых циклов и строк, он мгновенно отдает готовую дельту из памяти!
            if (cachedModifierCost <= 0.001f) return 0f;
            return cachedModifierCost - defaultCost;
        }

        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return default(ModifierChangeWhen);
        }
    }
}
