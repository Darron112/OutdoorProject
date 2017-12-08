using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWater : MonoBehaviour
{

    //tworzenie obiektu odpowiedzialnego za poruszanie
    public CharacterController characterController;

    //klawiatura - zmienne zawierajace wartosci poruszania
    public float predkoscPoruszania = 9.0f;
    public float wysokoscSkoku = 7.0f;
    public float aktualnaWysokoscSkoku = 0f;
    public float predkoscBiegania = 7.0f;

    //mysz - zmienne dotczace poruszania mysza
    public float czuloscMyszy = 3.0f;
    public float myszGoraDol = 0.0f;
    public float zakresMyszyGoraDol = 90.0f;

    public float predkoscWynurzania = 0.03f;
    public float predkoscZanurzania = 0.01f;

    public bool plynDoGory;

    // Use this for initialization
    void Start()
    {

        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {

        klawiatura();
        mysz();
    }

    private void klawiatura()
    {
        //pobieranie predkosci poruszania przod (wartosc dodatnia) / tyl (wartosc ujemna)
        float ruchPrzodTyl = Input.GetAxis("Vertical") * predkoscPoruszania;

        //pobieranie predkosci poruszania prawo (dodatnia) / lewo (ujemna)
        float ruchLewoPrawo = Input.GetAxis("Horizontal") * predkoscPoruszania;

        //zachowanie gracza w wodzie
        if(Input.GetButton("Jump"))
        {
            if(!plynDoGory)
            {
                aktualnaWysokoscSkoku = 0;
                plynDoGory = true;
            }
            aktualnaWysokoscSkoku += predkoscWynurzania;
        }
        else if(!characterController.isGrounded)
        {
            aktualnaWysokoscSkoku -= predkoscZanurzania;
            plynDoGory = false;
        }
        else if(characterController.isGrounded)
        {
            plynDoGory = false;
        }

        //skakanie - jezeli jestesmy na ziemi i naciskamy spacje (skok)
        if (characterController.isGrounded && Input.GetButton("Jump"))
        {
            aktualnaWysokoscSkoku = wysokoscSkoku;
        }
        else if (!characterController.isGrounded)
        {
            //fizyka odpowiedzialna za grawitacje (os y). Deltatime = dostosowanie predkosci klatek na sekunde
            aktualnaWysokoscSkoku += Physics.gravity.y * Time.deltaTime;
        }

        Debug.Log(Physics.gravity.y);

        //bieganie - przycisk wcisniety / puszczony
        if (Input.GetKeyDown("left shift"))
        {
            predkoscPoruszania += predkoscBiegania;
        }
        else if (Input.GetKeyUp("left shift"))
        {
            predkoscPoruszania -= predkoscBiegania;
        }

        //utworzenie wektora odpowiedzialnego za ruch, dopisujemy do niego nasze zmienne
        Vector3 ruch = new Vector3(ruchLewoPrawo, aktualnaWysokoscSkoku, ruchPrzodTyl);

        ruch = transform.rotation * ruch;

        characterController.Move(ruch * Time.deltaTime);
    }

    private void mysz()
    {
        //pobieranie ruchu myszy lewo (wartosci ujemne) / prawo (wartosci dodatnie)
        float myszLewoPrawo = Input.GetAxis("Mouse X") * czuloscMyszy;
        transform.Rotate(0, myszLewoPrawo, 0);

        //pobieranie ruchu myszy gora (dodatnie) / dol (ujemne)
        myszGoraDol -= Input.GetAxis("Mouse Y") * czuloscMyszy;

        //funkcja nie pozwalajaca przekroczyc danego zakresu
        myszGoraDol = Mathf.Clamp(myszGoraDol, -zakresMyszyGoraDol, zakresMyszyGoraDol);

        //nie obracamy calej postaci gora / dol tylko kamere
        Camera.main.transform.localRotation = Quaternion.Euler(myszGoraDol, 0, 0);
    }
}
