using UnityEngine;

public class AbilityParticleSystem : MonoBehaviour
{
    public bool extraEmitOnSelf;
    public int emitCount;
    public ParticleSystem ps;
    private ParticleSystem.MainModule psMain;
    private ParticleSystem.ShapeModule psShape;
    private ParticleSystem.MinMaxCurve baseMinMax;
    private ParticleSystem.VelocityOverLifetimeModule psVelOverLife;
    public float waitUntilNextEmit;
    public bool scaleStartSize;
    public bool scaleShapeSize;
    public bool rotateVelocity;
    public bool rotateParticles;
    private float baseConstant;
    private float baseShapeSize;
    private Vector3 baseShapePosition;
    private Vector3 baseShapeRotation;

    private void Awake()
    {
        psMain = ps.main;
        psShape = ps.shape;
        psVelOverLife = ps.velocityOverLifetime;
        baseConstant = ps.main.startSize.constant;
        baseMinMax = ps.main.startSize;
        baseShapeSize = ps.shape.radius;
        baseShapePosition = ps.shape.position;
        baseShapeRotation = ps.shape.rotation;
    }

    public void OnParticleSystemStopped()
    {
        GameObject.Destroy(this.gameObject);
    }

    public void Emit(ParticleSystem.EmitParams emitParams, int emitCount, float scaling)
    {
        if (scaleStartSize)
        {
            ScaleStartSize(scaling);
        }
        if (scaleShapeSize)
        {
            ScaleShapeSize(scaling);
        }


        ps.Emit(emitParams, emitCount);
    }

    public void Emit(ParticleSystem.EmitParams emitParams, int emitCount, float scaling, float rotationAngle, Transform parent)
    {
        if (scaleStartSize)
        {
            ScaleStartSize(scaling);
        }
        if (scaleShapeSize)
        {
            ScaleShapeSize(scaling);
        }

        Vector3 rotationVector = new Vector3(0, 0, rotationAngle);

        if (rotateVelocity)
        {
            Vector3 resultVelocity = Quaternion.Euler(rotationVector) * new Vector3(psVelOverLife.x.constant, psVelOverLife.y.constant, psVelOverLife.z.constant);
            emitParams.velocity = resultVelocity;
        }

        if (rotateParticles)
        {
            emitParams.rotation = -rotationAngle;
        }

        Vector3 offsetPosition = parent.position - transform.position;
        psVelOverLife.orbitalOffsetX = offsetPosition.x;
        psVelOverLife.orbitalOffsetY = offsetPosition.y;
        
        psShape.position = Quaternion.Euler(rotationVector) * psShape.position;
        psShape.rotation = rotationVector;



        ps.Emit(emitParams, emitCount);
    }

    private void ScaleStartSize(float scaling)
    {
        baseMinMax.constant = baseConstant * scaling;
        psMain.startSize = baseMinMax;
    }

    private void ScaleShapeSize(float scaling)
    {
        psShape.radius = baseShapeSize * scaling;
        psShape.position = baseShapePosition * scaling;
    }
}