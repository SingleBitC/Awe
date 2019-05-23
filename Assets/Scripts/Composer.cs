using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Effects/Deferred Renderer")]
public class Composer : MonoBehaviour
{
    public Shader _ShaderCullPass;
    public RenderTexture _RenderTexture_RESULT;
    public RenderTexture _RenderTexture_CULLER_A;
    public RenderTexture _RenderTexture_CULLER_B;

    // Materials

    private Material _MaterialCullPass;
    public Material MaterialCullPass {
        get {
            if (!_MaterialCullPass && _ShaderCullPass) {
                _MaterialCullPass = new Material(_ShaderCullPass);
                _MaterialCullPass.hideFlags = HideFlags.HideAndDontSave;
            }
            return _MaterialCullPass;
        }
    }

    // Cameras

    private Camera _CameraMain;
    public Camera CameraMain {
        get {
            if (!_CameraMain) {
                _CameraMain = GetComponent<Camera>();
            }
            return _CameraMain;
        }
    }
    private Camera _CameraFrontCuller;
    public Camera CameraFrontCuller {
        get {
            if (!_CameraFrontCuller) {
                _CameraFrontCuller = GameObject.Find("CameraFrontCuller").GetComponent<Camera>();
            }
            return _CameraFrontCuller;
        }
    }

    void Start()
    {
        RenderBuffer[] _RenderBuffersCuller = new RenderBuffer[2];
        _RenderBuffersCuller[0] = _RenderTexture_CULLER_A.colorBuffer;
        _RenderBuffersCuller[1] = _RenderTexture_CULLER_B.colorBuffer;
        CameraFrontCuller.SetTargetBuffers(_RenderBuffersCuller, _RenderTexture_CULLER_A.depthBuffer);
    }

    void OnPreRender() {
        CameraFrontCuller.Render(); 
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination) {
         
        /**
         * Culling pass
         */

        MaterialCullPass.SetTexture("_RenderTexture_CULLER_A", _RenderTexture_CULLER_A);
        MaterialCullPass.SetTexture("_RenderTexture_CULLER_B", _RenderTexture_CULLER_B);
        MaterialCullPass.SetMatrix("_FrustumCornersES", GetFrustumCorners(CameraFrontCuller));
        MaterialCullPass.SetMatrix("_CameraInvViewMatrix", CameraFrontCuller.cameraToWorldMatrix);
        MaterialCullPass.SetVector("_CameraWS", CameraFrontCuller.transform.position);
        CustomGraphicsBlit(null, destination, MaterialCullPass, 0);
    }

    void Update()
    {
        
    }

    private Matrix4x4 GetFrustumCorners(Camera cam) {
        float camFov = cam.fieldOfView;
        float camAspect = cam.aspect;
        float fovWHalf = camFov * 0.5f;
        float tan_fov = Mathf.Tan(fovWHalf * Mathf.Deg2Rad);
        Vector3 toRight = Vector3.right * tan_fov * camAspect;
        Vector3 toTop = Vector3.up * tan_fov;
        Vector3 topLeft = (-Vector3.forward - toRight + toTop);
        Vector3 topRight = (-Vector3.forward + toRight + toTop);
        Vector3 bottomRight = (-Vector3.forward + toRight - toTop);
        Vector3 bottomLeft = (-Vector3.forward - toRight - toTop);
        Matrix4x4 frustumCorners = Matrix4x4.identity;
        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, bottomRight);
        frustumCorners.SetRow(3, bottomLeft);
        return frustumCorners;
    }

    static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr) {
        RenderTexture.active = dest;
        if (source) {
            fxMaterial.SetTexture("_MainTex", source);
        }
        GL.PushMatrix();
        GL.LoadOrtho();
        fxMaterial.SetPass(passNr);
        GL.Begin(GL.QUADS);
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f);
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);
        GL.End();
        GL.PopMatrix();
    }
}
