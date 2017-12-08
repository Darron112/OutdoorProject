using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waterEffect : MonoBehaviour {

    private Transform trans;

    //nowe ustawienia mgly
    public bool fog;
    public Color kolorMgly;
    public float gestoscMgly;
    public FogMode trybMgly;
    public float startMgly;
    public float koniecMgly;

    //pierwotne ustawienia mgly
    private bool TMPfog;
    private Color TMPkolorMgly;
    private float TMPgestoscMgly;
    private FogMode TMPtrybMgly;
    private float TMPstartMgly;
    private float TMPkoniecMgly;

    public float wysokosc = 3f;

    // Use this for initialization
    void Start () {
        trans = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
		if(trans.position.y < wysokosc)
        {
            //zapamietywanie domyslnych ustawien mgly
            TMPfog = RenderSettings.fog;
            TMPkolorMgly = RenderSettings.fogColor;
            TMPgestoscMgly = RenderSettings.fogDensity;
            TMPtrybMgly = RenderSettings.fogMode;
            TMPstartMgly = RenderSettings.fogStartDistance;
            TMPkoniecMgly = RenderSettings.fogEndDistance;

            //nadanie nowych ustawien mgly
            RenderSettings.fog = fog;
            RenderSettings.fogColor = kolorMgly;
            RenderSettings.fogDensity = gestoscMgly;
            RenderSettings.fogMode = trybMgly;
            RenderSettings.fogStartDistance = startMgly;
            RenderSettings.fogEndDistance = koniecMgly;
        }
        else
        {
            //przywracanie domyslnych ustawien mgly
            RenderSettings.fog = TMPfog;
            RenderSettings.fogColor = TMPkolorMgly;
            RenderSettings.fogDensity = TMPgestoscMgly;
            RenderSettings.fogMode = TMPtrybMgly;
            RenderSettings.fogStartDistance = TMPstartMgly;
            RenderSettings.fogEndDistance = TMPkoniecMgly;
        }
	}
}
