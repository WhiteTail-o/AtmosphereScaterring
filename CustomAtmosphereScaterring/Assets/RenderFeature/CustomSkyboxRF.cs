using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomSkyboxRF : ScriptableRendererFeature
{
    class SkyboxSettings
    {
        public const float AtmosphereHeight = 1000000f;
        public const float PlanetRadius = 6357000f;
        public Vector4 DensityScaleHeight = new Vector4(7994f, 1200f, 0, 0);
        public Vector4 ExtinctionR = new Vector4(5.8f, 13.5f, 33.1f, 0f) * 0.000001f;
        public Vector4 ExtinctionM = new Vector4(2f, 2f, 2f, 0f) * 0.00001f;
        public float MieG = 0.76f;
        public Vector4 ScatteringR = new Vector4(5.8f, 13.5f, 33.1f, 0f) * 0.000001f;
        public Vector4 ScatteringM = new Vector4(2f, 2f, 2f, 0f) * 0.00001f;
        public float OriginHeight = 100f;

    }

    class CustomRenderPass : ScriptableRenderPass
    {
        private ComputeShader computeShader;
        private Material material;
        private SkyboxSettings settings;
        int kernelId;

        private RenderTexture rt;

        public CustomRenderPass(ComputeShader cs, Material mat, SkyboxSettings skyboxsettings) {
            computeShader = cs;
            settings = skyboxsettings;
            material = mat;
        }
        
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor renderTextureDescriptor = new RenderTextureDescriptor(Camera.main.pixelWidth, Camera.main.pixelHeight, RenderTextureFormat.DefaultHDR);
            renderTextureDescriptor.enableRandomWrite = true;
            // rt = new RenderTexture(renderTextureDescriptor);
            // rt.enableRandomWrite = true;
            // rt.Create();
            cmd.GetTemporaryRT(ShaderID.SkyboxTextureId, renderTextureDescriptor);
            
            ConfigureTarget(ShaderID.SkyboxTextureId);
            ConfigureClear(ClearFlag.Color, Color.black);
            kernelId = computeShader.FindKernel("AtmosphereScaterring");
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // material.SetTexture(ShaderID.SkyboxTextureId, rt);
            // computeShader.SetTexture(kernelId, ShaderID.SkyboxTextureId, rt);
            // computeShader.SetFloat(AtmosphereHeightId, SkyboxSettings.AtmosphereHeight);
            // computeShader.SetFloat(PlanetRadiusId, SkyboxSettings.PlanetRadius);
            // computeShader.SetVector(DensityScaleHeightId, settings.DensityScaleHeight);
            // computeShader.SetVector(ExtinctionRId, settings.ExtinctionR);
            // computeShader.SetVector(ExtinctionMId, settings.ExtinctionM);
            // computeShader.SetFloat(MieGId, settings.MieG);
            // computeShader.SetVector(ScatteringRId, settings.ScatteringR);
            // computeShader.SetVector(ScatteringMId, settings.ScatteringM);
            // computeShader.SetFloat(OriginHeightId, settings.OriginHeight);
            // computeShader.Dispatch(kernelId, Camera.main.pixelWidth / 8, Camera.main.pixelHeight/8, 1);

            CommandBuffer cmd = CommandBufferPool.Get("CustomSkybox_CS");
            cmd.SetGlobalTexture(ShaderID.SkyboxTextureId, ShaderID.SkyboxTextureId);
            cmd.SetComputeTextureParam(computeShader, kernelId, ShaderID.SkyboxTextureId, ShaderID.SkyboxTextureId);
            cmd.SetComputeFloatParam(computeShader, AtmosphereHeightId, SkyboxSettings.AtmosphereHeight);
            cmd.SetComputeFloatParam(computeShader, PlanetRadiusId, SkyboxSettings.PlanetRadius);
            cmd.SetComputeVectorParam(computeShader, DensityScaleHeightId, settings.DensityScaleHeight);
            cmd.SetComputeVectorParam(computeShader, ExtinctionRId, settings.ExtinctionR);
            cmd.SetComputeVectorParam(computeShader, ExtinctionMId, settings.ExtinctionM);
            cmd.SetComputeFloatParam(computeShader, MieGId, settings.MieG);
            cmd.SetComputeVectorParam(computeShader, ScatteringRId, settings.ScatteringR);
            cmd.SetComputeVectorParam(computeShader, ScatteringMId, settings.ScatteringM);
            cmd.SetComputeFloatParam(computeShader, OriginHeightId, settings.OriginHeight);
            cmd.DispatchCompute(computeShader, kernelId, Camera.main.pixelWidth / 8, Camera.main.pixelHeight/8, 1);

            context.ExecuteCommandBuffer(cmd);

            cmd.Clear();
            cmd.Release();
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // rt.Release();
            cmd.ReleaseTemporaryRT(ShaderID.SkyboxTextureId);
        }
    }

    CustomRenderPass m_ScriptablePass;
    public ComputeShader m_ComputeShader;
    public Material m_SkyboxMaterial;
    SkyboxSettings m_SkyboxSettings;

    // Parameter of the skybox
    // ==================================================================================================
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

    // private const float AtmosphereHeight = 1000000f;
    // private const float PlanetRadius = 6357000f;
    // private Vector4 DensityScale = new Vector4(7994f, 1200f, 0, 0);
    // private Vector4 RayleighScatter = new Vector4(5.8f, 13.5f, 33.1f, 0f) * 0.000001f;
    // private Vector4 MieScatter = new Vector4(2f, 2f, 2f, 0f) * 0.00001f;

    private static int AtmosphereHeightId = Shader.PropertyToID("_AtmosphereHeight");
    private static int PlanetRadiusId = Shader.PropertyToID("_PlanetRadius");
    private static int DensityScaleHeightId = Shader.PropertyToID("_DensityScaleHeight");
    private static int ExtinctionRId = Shader.PropertyToID("_ExtinctionR");
    private static int ExtinctionMId = Shader.PropertyToID("_ExtinctionM");
    private static int MieGId = Shader.PropertyToID("_MieG");
    private static int ScatteringRId = Shader.PropertyToID("_ScatteringR");
    private static int ScatteringMId = Shader.PropertyToID("_ScatteringM");
    private static int OriginHeightId = Shader.PropertyToID("_OriginHeight");
    // ==================================================================================================


    public override void Create()
    {
        m_SkyboxSettings = new SkyboxSettings();
        m_SkyboxSettings.ExtinctionR *= RayleighExtinctionCoef;
        m_SkyboxSettings.ExtinctionM *= MieExtinctionCoef;
        m_SkyboxSettings.ScatteringR *= RayleighScatterCoef;
        m_SkyboxSettings.ScatteringM *= MieScatterCoef;
        m_SkyboxSettings.MieG = MieG;
        m_SkyboxSettings.OriginHeight = OriginHeight;

        m_ScriptablePass = new CustomRenderPass(m_ComputeShader, m_SkyboxMaterial, m_SkyboxSettings);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


