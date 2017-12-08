using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waterTrigger : MonoBehaviour {

    private Transform trans;

    private Transform playerTransform;

    private GameObject water;

    private SplitScreen splitScreen;

    private Transform mainCamera;

    private Transform waterCamera;

    private PlayerControl playerControler;

    private bool playerInWater;

    public float activationLvlScript = 0.6f;

    private bool changeScript = false;

    private bool changedScript;

    private AudioSource audioSource;
    public AudioClip soundUnderWater;
    public AudioClip soundWater;


    
	// Use this for initialization
	void Start () {
        trans = GetComponent<Transform>();
        water = trans.FindChild("Woda1").gameObject;
        mainCamera = Camera.main.GetComponent<Transform>();
        splitScreen = mainCamera.GetComponent<SplitScreen>();
        waterCamera = mainCamera.Find("waterCamera");

        audioSource = trans.GetComponent<AudioSource>();
        WaterSounds();
	}
	
	// Update is called once per frame
	void Update () {
		if(playerInWater) //jezeli gracz w wodzie
        {         
            //jezeli jest odpowiednia glebokosc i skrypt i nie zostal jeszcze przelaczony
            if(changeScript && !changedScript)
            {
                poruszanieWoda();  //zmiana na skrypt chodzenia w wodzie
                changedScript = true; //zmiana flagi aby ponownie tu nie wchodzic
                SoundsUnderWater();
            }
            else if(!changeScript && changedScript)
            {
                poruszanieLad();
                changedScript = false;
                WaterSounds();
            }
        }
	}

    //przelaczanie poruszania sie po ladzie na poruszanie sie w wodzie
    private void poruszanieWoda()
    {
        if(playerTransform !=null)
        {
            //wylaczanie skryptu odpowiedzialnego za poruszanie po ladzie
            playerControler = playerTransform.GetComponent<PlayerControl>();
            if (playerControler != null)
                playerControler.enabled = false;

            //aktywacja poruszania sie w wodzie
            PlayerWater playerWater = playerTransform.GetComponent<PlayerWater>();
            playerWater.enabled = true;

            //aktualny zwrot kamery - poruszanie sie w wodzie
            playerWater.myszGoraDol = playerControler.myszGoraDol;

            //restart skoku
            playerWater.aktualnaWysokoscSkoku = 0f;
        }
    }

    //przelaczanie poruszania sie w wodzie na poruszanie sie na ladzie
    private void poruszanieLad()
    {
        if (playerTransform != null)
        {
            //wylaczanie skryptu odpowiedzialnego za poruszanie po ladzie
            playerControler = playerTransform.GetComponent<PlayerControl>();
            if (playerControler != null)
                playerControler.enabled = true;

            //aktywacja poruszania sie w wodzie
            PlayerWater playerWater = playerTransform.GetComponent<PlayerWater>();
            playerWater.enabled = false;

            playerControler.aktualnaWysokoscSkoku = playerWater.aktualnaWysokoscSkoku;
            //aktualny zwrot kamery - poruszanie sie w wodzie
            playerWater.myszGoraDol = playerControler.myszGoraDol;
        }
    }

    //wejscie gracza do wody
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");
        Transform objectTrans = other.GetComponent<Transform>();

        //pobieranie komponentu PlayerControl
        playerControler = objectTrans.GetComponent<PlayerControl>();

        if(playerControler !=null) //nie null oznacza wejscie do wody
        {
            //pobieranie obiektu transform gracza
            playerTransform = other.GetComponent<Transform>();
            //ustawienie flagi ze gracz w wodzie
            playerInWater = true;
            //obiekt wody do skryptu podzialu
            splitScreen.setWoda(water);
            //aktywacja kamery dla wody
            waterCamera.gameObject.SetActive(true);
        }
    }

    //wyjscia gracza z wody
    void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit");
        Transform objectTrans = other.GetComponent<Transform>();

        //pobieranie komponentu PlayerControl
        playerControler = objectTrans.GetComponent<PlayerControl>();

        if (playerControler != null) //nie null oznacza wejscie do wody
        {
            //pobieranie obiektu transform gracza
            playerTransform = other.GetComponent<Transform>();
            //ustawienie flagi ze gracz poza woda
            playerInWater = false;
            //obiekt wody do skryptu podzialu
            splitScreen.setWoda(null);
            //deaktywacja kamery dla wody
            waterCamera.gameObject.SetActive(false);
        }
    }

    //wyjscia gracza z wody
    void OnTriggerStay(Collider other)
    {
        Debug.Log("OnTriggerStay");
        Transform playerTransform = other.GetComponent<Transform>();

        //pobieranie komponentu PlayerControl
        playerControler = playerTransform.GetComponent<PlayerControl>();

        if (playerControler != null) //nie null oznacza wejscie do wody
        {
            //sprawdzenie na jakiej wysokosci jest powierzchnia
            float powierzchnia = trans.position.y + trans.localScale.y;
            
            //czy gracz jest na wysokosci przelaczenia skryptow
            if(playerTransform.position.y > powierzchnia - activationLvlScript)
            {
                changeScript = false; //gracz za wysoko
            }
            else
            {
                changeScript = true; //gracz na poziomie przelaczania skryptow
            }
        }
        Debug.Log("OnTriggerStay");
    }

    //metoda odpowiedzialna za dzwieki pod woda
    private void SoundsUnderWater()
    {
        if (audioSource != null && soundUnderWater!= null)
        {
            audioSource.Stop();
            audioSource.spatialBlend = 0;
            audioSource.clip = soundUnderWater;
            audioSource.Play();
        }
    }

    //metoda odpowiedzialna za dzwieki pod woda
    private void WaterSounds()
    {
        if(audioSource !=null && soundWater !=null)
        {
            audioSource.Stop();
            audioSource.spatialBlend = 1;
            audioSource.clip = soundWater;
            audioSource.Play();
        }
    }
}
