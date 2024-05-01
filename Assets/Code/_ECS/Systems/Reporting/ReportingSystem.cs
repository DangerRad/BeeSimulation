using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(BeehivePopulationSystem))]
public partial struct ReportingSystem : ISystem
{
    public static event Action<string> PanelTextUpdated;

    public static event Action<float3> PanelPositionUpdated;
    // public static Action<int> PanelNumberChanged;
    static int _panelToDisplay;
    int _panelBeingDisplayed;

    public void OnDestroy(ref SystemState state)
    {
        BeehiveInfoDisplay.PanelNumberChanged -= PanelChanged;
    }

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeeSquad>();
        BeehiveInfoDisplay.PanelNumberChanged += PanelChanged;
    }

    public void PanelChanged(int panelNumber)
    {
        _panelToDisplay = panelNumber;
    }

    public void OnUpdate(ref SystemState state)
    {
        var hit = SystemAPI.GetSingleton<Hit>();
        var simulation = SystemAPI.GetSingleton<Simulation>();

        if (hit.HitChanged || simulation.MakeSimulationStep() || _panelBeingDisplayed != _panelToDisplay)
        {
            _panelBeingDisplayed = _panelToDisplay;
            Entity selectedEntity = hit.HitEntity;
            if (selectedEntity == Entity.Null)
                return;
            string infoToDisplay = "";
            switch (_panelToDisplay)
            {
                case 0:
                    infoToDisplay = HiveInfo(SystemAPI.GetComponent<Beehive>(selectedEntity));
                    break;
                case 1:
                    Queen queen = SystemAPI.GetComponent<Queen>(selectedEntity);
                    Lifespan lifespan = SystemAPI.GetComponent<Lifespan>(selectedEntity);
                    infoToDisplay = QueenInfo(queen, lifespan);
                    break;
                case 2:
                    FoodScarcity foodScarcity = SystemAPI.GetComponent<FoodScarcity>(selectedEntity);
                    Mites mites = SystemAPI.GetComponent<Mites>(selectedEntity);
                    infoToDisplay = OtherInfo(foodScarcity, mites);
                    break;
                default:
                    break;
            }

            if (hit.HitChanged)
            {
                PanelPositionUpdated?.Invoke(SystemAPI.GetComponent<LocalToWorld>(selectedEntity).Position);
            }

            PanelTextUpdated?.Invoke(infoToDisplay);
        }
    }

    public static string HiveInfo(Beehive beehive)
    {
        string hiveInfo = "Beehive Info";
        hiveInfo += "\nID: " + beehive.Id;
        hiveInfo += "\nPopulation: " + beehive.Population;
        hiveInfo += "\nFood Stored By Type: ";
        hiveInfo += "\n" + (FlowerSpecies)0 + ": " + beehive[0].ToString("0.0");
        hiveInfo += "\n" + (FlowerSpecies)1 + ": " + beehive[1].ToString("0.0");
        hiveInfo += "\n" + (FlowerSpecies)2 + ": " + beehive[2].ToString("0.0");
        hiveInfo += "\nTotal Food stored: " + beehive.TotalFood.ToString("0.0");
        return hiveInfo;
    }

    public static string QueenInfo(Queen queen, Lifespan lifespan)
    {
        string queenInfo = "Queen Info";
        queenInfo += "\nSpecies: " + queen.Species;
        queenInfo += "\nBees Birth Tick: " + queen.BeesBirthTick;
        queenInfo += "\nFertility: " + queen.Fertility.ToString("0.0");
        queenInfo += "\nMites Resistance: " + queen.MitesResistance.ToString("0.0");
        queenInfo += "\nTicks To Live: " + lifespan.TicksToLive;
        return queenInfo;
    }

    public static string OtherInfo(FoodScarcity foodScarcity, Mites mites)
    {
        string otherInfo = "Other Info";
        otherInfo += "\nFood Scarcity: " + foodScarcity.Value.ToString("0.0");
        otherInfo += "\nMites Infestation: " + mites.InfestationAmount.ToString("0.000");
        return otherInfo;
    }
}
