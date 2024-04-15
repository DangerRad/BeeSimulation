using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class BeehiveAuthoring : MonoBehaviour
{
    public AnimationCurve birthRateOverYearCurve;
    public int AnimationCurveAccuracy;

    class Baker : Baker<BeehiveAuthoring>
    {
        public override void Bake(BeehiveAuthoring authoring)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref BirthRateOverYearData BirthRateData = ref builder.ConstructRoot<BirthRateOverYearData>();
            BlobBuilderArray<float> arrayBuilder = builder.Allocate(
                ref BirthRateData.Ticks,
                authoring.AnimationCurveAccuracy
            );

            for (int i = 0; i < authoring.AnimationCurveAccuracy; i++)
            {
                arrayBuilder[i] = RasterizeCurve(
                    i, authoring.AnimationCurveAccuracy, authoring.birthRateOverYearCurve);
            }

            var blobReference = builder.CreateBlobAssetReference<BirthRateOverYearData>(Allocator.Persistent);
            builder.Dispose();

            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new Beehive
            {
                Ventilation = 0f,
                TotalFood = 15000.0f,
                BirthRateData = blobReference,
            });
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
