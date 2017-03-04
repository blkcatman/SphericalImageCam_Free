//SphericalImageCam.cs
//
//Copyright (c) 2015 Tatsuro Matsubara
//SphericalImageCam_Free is licensed under a Creative Commons Attribution-ShareAlike 4.0 International License.
//See also http://creativecommons.org/licenses/by-sa/4.0/
//
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SphericalImageCam_Free : MonoBehaviour {
	private float[] rots = { -90f, 0f, 90f, 179.99f, 0f, 0f, 0f, 120f, 0f, -120f };
	private float[] ds = { -0.5f, -0.5f, 0f, 0f, 0f };
	private string[] metas = { "_VC", "_VC", "_VS", "_VS", "_VS" };
	private Vector4[] rects = {
		new Vector4(0, 0.4355f, 1, 0.568f),
		new Vector4(0,-0.4355f, 1, 0.568f),
		new Vector4(0, 0, 0.33356f, 0.6666667f),
		new Vector4( 0.6666667f, 0, 0.33356f, 0.6666667f),
		new Vector4(-0.6666667f, 0, 0.33356f, 0.6666667f)
	};
	private Vector4[] flips = {
		new Vector4(0, 1, 1,-1),
		new Vector4(1, 0,-1, 1),
		new Vector4(0, 0, 1, 1),
		new Vector4(0, 0, 1, 1),
		new Vector4(0, 0, 1, 1)
	};

	private bool canDraw = false;
	protected bool supportHDRTextures = true;
	protected bool supportDX11 = false;

	public RenderTexture target;
	[HideInInspector]
	public Shader shader;

	void Start() {
		if (target == null) {
			target = new RenderTexture(1280, 720, 0, RenderTextureFormat.ARGB32);
			target.name = "temp";
		}
		Camera main = gameObject.GetComponent<Camera>();

		if (!CheckSupport(main.hdr)) {
			return;
		}
		if (!shader) {
			Debug.LogWarning("Missing the shader \"Hidden/SphericalShader\"!");
			return;
		}
		if (!shader.isSupported) {
			Debug.LogWarning("The shader \"Hidden/SphericalShader\" is not supported on this platform!");
			return;
		}

		for (int i = 0; i < 5; i++) {
			GameObject dummy = new GameObject();
			Vector3 rot = new Vector3(rots[i * 2], rots[i * 2 + 1], 0);
			dummy.transform.SetParent(gameObject.transform);
			dummy.transform.localRotation = Quaternion.Euler(rot);
			dummy.transform.localPosition = Vector3.zero;
			dummy.hideFlags = HideFlags.HideInHierarchy;

			Camera cam = dummy.AddComponent<Camera>();
			System.Reflection.FieldInfo[] camFields = main.GetType().GetFields();
			foreach (System.Reflection.FieldInfo f in camFields) {
				f.SetValue(cam, f.GetValue(main));
			}
			cam.aspect = 1f;
			cam.nearClipPlane = 0.001f;
			cam.fieldOfView = 120f;
			cam.rect = new Rect(0f, 0f, 1f, 1f);
			cam.depth = ds[i];
			cam.targetTexture = target;

			Material mat = new Material(shader);
			mat.EnableKeyword(metas[i]);
			mat.SetVector("_rt", rects[i]);
			mat.SetVector("_fl", flips[i]);

			RenderEvent ev = dummy.AddComponent<RenderEvent>();
			ev.material = mat;
		}

		main.nearClipPlane = 0.001f;
		main.farClipPlane = 0.002f;
		canDraw = true;
	}

	bool CheckSupport(bool needHdr) {
		supportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
		supportDX11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;

		if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures) {
			Debug.LogWarning("ImageEffects or RenderTexture is not supported on this platform!");
			return false;
		}
		if (needHdr && !supportHDRTextures) {
			Debug.LogWarning("HDR Rendering is not supported on this platform!");
			return false;
		}
		return true;
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination) {
		if (canDraw) {
			Graphics.Blit(target, destination);
		}
	}

	void OnDestroy() {
		for (int i = 0; i < transform.childCount; i++) {
			GameObject obj = transform.GetChild(i).gameObject;
			DestroyImmediate(obj);
		}
	}

	class RenderEvent : MonoBehaviour {
		public Material material;

		void OnRenderImage(RenderTexture source, RenderTexture destination) {
			if (material == null) {
				Graphics.Blit(source, destination);
			}
			Graphics.Blit(source, destination, material);
		}

		void OnDestroy() {
			DestroyImmediate(material);
		}
	}
}