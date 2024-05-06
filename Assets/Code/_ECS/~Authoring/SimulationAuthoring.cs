using Unity.Collections;
using UnityEngine;
using Unity.Entities;

public class SimulationAuthoring : MonoBehaviour
{
    [Header("Simulation")] public float TickLength = 0.5f;
    public int TicksInYear = 1;
    public int TicksInDay = 1;
    public int TicksInNight = 1;
    public AnimationCurve birthRateOverYearCurve;
    public GameObject BeePrefab;

    class Baker : Baker<SimulationAuthoring>
    {
        public override void Bake(SimulationAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            var blobReference = CreateBirthRateOverYearBlob(authoring);

            AddComponent(entity, new Simulation
            {
                TickLength = authoring.TickLength,
                TicksInDay = authoring.TicksInDay,
                TicksInYear = authoring.TicksInYear,
                TicksInNight = authoring.TicksInNight,
                BeePrefab = GetEntity(authoring.BeePrefab, TransformUsageFlags.Dynamic),
                BirthRateData = blobReference,
                TicksInSeason = authoring.TicksInYear / 4,
            });
            AddComponent<Hit>(entity);
            AddComponent<BeehvieSpawnInfo>(entity);
            AddComponent<InputState>(entity);
        }

        BlobAssetReference<BirthRateOverYearData> CreateBirthRateOverYearBlob(SimulationAuthoring authoring)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref BirthRateOverYearData BirthRateData = ref builder.ConstructRoot<BirthRateOverYearData>();
            BlobBuilderArray<float> arrayBuilder = builder.Allocate(
                ref BirthRateData.Ticks,
                authoring.TicksInYear
            );

            for (int i = 0; i < authoring.TicksInYear; i++)
            {
                arrayBuilder[i] = RasterizeCurve(i, authoring.TicksInYear, authoring.birthRateOverYearCurve);
            }

            var blobReference = builder.CreateBlobAssetReference<BirthRateOverYearData>(Allocator.Persistent);
            builder.Dispose();
            return blobReference;
        }

        float RasterizeCurve(int currentPoint, int totalPoints, AnimationCurve curve)
        {
            float totalTime = curve[curve.length - 1].time;
            float timeOnCurve = 1.0f * currentPoint / (totalPoints - 1) * totalTime;
            float valueOnCurve = curve.Evaluate(timeOnCurve);
            return valueOnCurve;
        }
    }
}
