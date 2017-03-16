//SphericalImageCam_Stereo_Pseudo.cs
//
//Copyright (c) 2015 Tatsuro Matsubara
//SphericalImageCam_Free is licensed under a Creative Commons Attribution-ShareAlike 4.0 International License.
//See also http://creativecommons.org/licenses/by-sa/4.0/
//
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SphericalImageCam_Stereo_Pseudo : MonoBehaviour {

	private GameObject l;
	private GameObject r;

	private bool canDraw = false;

	public RenderTexture target;
	[HideInInspector]
	public Shader shader;

	[SerializeField]
	private float _ipd = 0.06f;
	public float IPD {
		set {
			_ipd = value;
			l.transform.localPosition = l.transform.right * _ipd * -0.5f;
			r.transform.localPosition = r.transform.right * _ipd * 0.5f;
		}
		get {
			return _ipd;
		}
	}

	void Awake() {
		if(shader == null) {
			shader = Resources.Load<Shader>("RectDraw");
		}
		l = GameObject.Instantiate(new GameObject("left"));
		r = GameObject.Instantiate(new GameObject("right"));
	}

	// Use this for initialization
	void Start () {
		if (target == null) {
			target = new RenderTexture(1280, 720, 0, RenderTextureFormat.ARGB32);
			target.name = "temp";
		}
		Camera main = gameObject.GetComponent<Camera>();
		int width = target.width;
		int height = target.height;

		l.transform.SetParent(gameObject.transform, false);
		r.transform.SetParent(gameObject.transform, false);
		l.transform.localPosition = Vector3.right * _ipd * -0.5f;
		r.transform.localPosition = Vector3.right * _ipd * 0.5f;

		SphericalImageCam_Free ls = l.AddComponent<SphericalImageCam_Free>();
		SphericalImageCam_Free rs = r.AddComponent<SphericalImageCam_Free>();
		ls.drawOnOffscreen = true;
		rs.drawOnOffscreen = true;
		ls.target = target;
		rs.target = target;
		ls.SetGraphicRect(new Vector4(0.0f, 0.5f, 1.0f, 0.5f));
		rs.SetGraphicRect(new Vector4(0.0f,-0.5f, 1.0f, 0.5f));

		main.nearClipPlane = 0.001f;
		main.farClipPlane = 0.002f;
		main.depth = 0f;
		canDraw = true;
	}

	void OnValidate() {
		if (l != null && r != null) {
			l.transform.localPosition = l.transform.right * _ipd * -0.5f;
			r.transform.localPosition = r.transform.right * _ipd * 0.5f;
		}
	}

	void OnDestroy() {
		DestroyImmediate(l);
		DestroyImmediate(r);
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination) {
		if (canDraw) {
			Graphics.Blit(target, destination);
		}
	}
}
