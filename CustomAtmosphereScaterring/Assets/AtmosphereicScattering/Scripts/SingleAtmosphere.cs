using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class SingleAtmosphere : MonoBehaviour
{
    protected Shader AtmosphereShader;
    public Material AtmosphereMat;

    // public Light sun;

    [Range(0, 10f)]
    public float RayleighScatterCoef = 1;
    [Range(0, 10f)]
    public float RayleighExtinctionCoef = 1;
    [Range(0, 10f)]
    public float MieScatterCoef = 1;
    [Range(0, 10f)]
    public float MieExtinctionCoef = 1;
    [Range(0f, 0.999f)]
    public float MieG = 0.76f;

    [Min(0f)]
    public float DistanceScale = 1f;

    [Min(0f)]
    public float OriginHeight = 100f;

    public CustomSkyboxRF skyboxRF;

    private const float AtmosphereHeight = 1000000f;
    private const float PlanetRadius = 6357000f;
    private Vector4 DensityScale = new Vector4(7994f, 1200f, 0, 0);
    private Vector4 RayleighScatter = new Vector4(5.8f, 13.5f, 33.1f, 0f) * 0.000001f;
    private Vector4 MieScatter = new Vector4(2f, 2f, 2f, 0f) * 0.00001f;

    private static int AtmosphereHeightId = Shader.PropertyToID("_AtmosphereHeight");
    private static int PlanetRadiusId = Shader.PropertyToID("_PlanetRadius");
    private static int DensityScaleHeightId = Shader.PropertyToID("_DensityScaleHeight");
    private static int ExtinctionRId = Shader.PropertyToID("_ExtinctionR");
    private static int ExtinctionMId = Shader.PropertyToID("_ExtinctionM");
    private static int MieGId = Shader.PropertyToID("_MieG");
    private static int ScatteringRId = Shader.PropertyToID("_ScatteringR");
    private static int ScatteringMId = Shader.PropertyToID("_ScatteringM");
    private static int OriginHeightId = Shader.PropertyToID("_OriginHeight");

    SingleAtmosphere() {
        Debug.Log("SingleAtmosphere Constructor");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // AtmosphereShader = Shader.Find("Skybox/SingleAtmosphereScattering");
        // AtmosphereMat = new Material(AtmosphereShader) {hideFlags = HideFlags.HideAndDontSave};
    }

    // Update is called once per frame
    void Update()
    {
        if (AtmosphereMat) {
            AtmosphereMat.SetFloat(AtmosphereHeightId, AtmosphereHeight);
            AtmosphereMat.SetFloat(PlanetRadiusId, PlanetRadius);
            AtmosphereMat.SetVector(DensityScaleHeightId, DensityScale);
            AtmosphereMat.SetVector(ExtinctionRId, RayleighExtinctionCoef*RayleighScatter);
            AtmosphereMat.SetVector(ExtinctionMId, MieExtinctionCoef * MieScatter);
            AtmosphereMat.SetFloat(MieGId, MieG);
            AtmosphereMat.SetVector(ScatteringRId, RayleighScatterCoef * RayleighScatter);
            AtmosphereMat.SetVector(ScatteringMId, MieScatterCoef * MieScatter);
            AtmosphereMat.SetFloat(OriginHeightId, OriginHeight);
        }
    }
}
