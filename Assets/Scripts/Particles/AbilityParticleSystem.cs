using UnityEngine;

public class AbilityParticleSystem : MonoBehaviour
{
    public int emitCount;
    public ParticleSystem ps;
    private ParticleSystem.MainModule psMain;
    private ParticleSystem.ShapeModule psShape;
    public float waitUntilNextEmit;
    public bool scaleStartSize;
    public bool scaleShapeSize;

    private ParticleSystem.MinMaxCurve baseStartSize;
    private float baseShapeSize;

    private void Start()
    {
        psMain = ps.main;
        psShape = ps.shape;
        baseStartSize = ps.main.startSize;
        baseShapeSize = ps.shape.radius;
    }

    public void Emit(ParticleSystem.EmitParams emitParams, int emitCount, float scaling)
    {
        if (scaleStartSize)
        {
            psMain.startSize = new ParticleSystem.MinMaxCurve(baseStartSize.constant * scaling);
        }
        if (scaleShapeSize)
        {
            psShape.radius = baseShapeSize * scaling;
        }

        ps.Emit(emitParams, emitCount);
    }
}