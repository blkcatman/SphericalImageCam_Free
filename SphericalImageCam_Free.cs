//SphericalImageCam.cs
//
//Copyright (c) 2015 Tatsuro Matsubara
//SphericalImageCam_Free is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
//See also http://creativecommons.org/licenses/by-nc-sa/4.0/
//
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class SphericalImageCam_Free : MonoBehaviour {
	private float[] rots = {-90f, 0f, 90f, 179.99f, 0f, 0f, 0f, 120f, 0f, -120f};
	private float[] ds = {-0.5f, -0.5f, 0f, 0f, 0f};
	private string[] metas = {"_VT", "_VC", "_VB", "_VC", "_VF", "_VS", "_VR", "_VS", "_VL", "_VS"};
	private bool canDraw = false;
	protected bool supportHDRTextures = true;
	protected bool supportDX11 = false;
	
	public RenderTexture target;

	void Start () {
		if (target == null) {
			target = new RenderTexture (1280, 720, 0, RenderTextureFormat.ARGB32);
			target.name = "temp";
		}
		Camera main = gameObject.GetComponent<Camera>();

		if(!CheckSupport(false, main.hdr)) {
			return;
		}
		Shader shader = Shader.Find("Hidden/SphericalShader");
		if(!shader) {
			Debug.LogWarning("Missing the shader \"Hidden/SphericalShader\"!");
			return;
		}
		if(!shader.isSupported) {
			Debug.LogWarning("The shader \"Hidden/SphericalShader\" is not supported on this platform!");
			return;
		}
		Component[] cs = gameObject.GetComponents<MonoBehaviour>();

		for (int i = 0; i < 5; i++) {
			GameObject dummy = new GameObject();
			Vector3 rot = new Vector3(rots[i*2], rots[i*2+1], 0);
			dummy.transform.rotation = Quaternion.Euler(rot);
			dummy.transform.SetParent(gameObject.transform);
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
			mat.EnableKeyword(metas[i*2]);
			mat.EnableKeyword(metas[i*2+1]);

			for(int j = 0; j<cs.Length; j++) {
				System.Type type = cs[j].GetType();
				if(CheckComponentTypes(type)) {
					Component copy = dummy.AddComponent(type);
					System.Reflection.FieldInfo[] fields = type.GetFields(); 
					foreach (System.Reflection.FieldInfo field in fields) {
						field.SetValue(copy, field.GetValue(cs[j]));
					}
				}
			}

			RenderEvent ev = dummy.AddComponent<RenderEvent>();
			ev.material = mat;
		}

		for(int j = 0; j<cs.Length; j++) {
			System.Type type = cs[j].GetType();
			if(CheckComponentTypes(type)) {
				DestroyImmediate(cs[j]);
			}

		}

		main.nearClipPlane = 0.001f;
		main.farClipPlane = 0.002f;
		canDraw = true;
	}

	bool CheckComponentTypes(System.Type type) {
		if(type.Equals(typeof(SphericalImageCam_Free))) return false;
		//if(type.Equals(typeof(AudioListener))) return false;
		//if(type.Equals(typeof(AudioReverbZone))) return false;
		//if(type.Equals(typeof(AudioClip))) return false;
		//if(type.Equals(typeof(AudioSource))) return false;
		if(type.Equals(typeof(Camera))) return false;
		if(type.Equals(typeof(Transform))) return false;
		return true;
	}

	bool CheckSupport (bool needDepth, bool needHdr){
		supportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
		supportDX11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;
		
		if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures) {
			Debug.LogWarning("ImageEffects or RenderTexture is not supported on this platform!");
			return false;
		}
		if(needHdr && !supportHDRTextures) {
			Debug.LogWarning("HDR Rendering is not supported on this platform!");
			return false;		
		}
		if(needDepth && !SystemInfo.SupportsRenderTextureFormat (RenderTextureFormat.Depth)) {
			Debug.LogWarning("RenderTextureFormat.Depth is not supported on this platform!");
			return false;
		}
		if(needDepth) {
			gameObject.GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		}
		
		return true;
	}

	void OnRenderImage (RenderTexture source, RenderTexture destination) {
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
		
		void OnRenderImage (RenderTexture source, RenderTexture destination) {
			if(material == null) {
				Graphics.Blit (source, destination);
			}
			Graphics.Blit (source, destination, material);
		}
		
		void OnDestroy() {
			DestroyImmediate(material);
		}
	}
}
