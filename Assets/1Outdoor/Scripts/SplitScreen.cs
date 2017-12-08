using UnityEngine;
using System.Collections;


public class SplitScreen : MonoBehaviour {

    private Transform trans;

    /** Kamera g³ówna.*/
    private Camera kameraGlowna;
    /** Kamera dla wody.*/
    private Camera kameraWWodzie;
    /** Transform dla wody.*/
    private Transform kamWWodzieTrans;
    /** Obiekt wody potrzebny do pobrania pozycji.*/
    private GameObject woda;

    /** Efekty przebywania pod wod¹ dla kamery podwodnej.*/
    private waterEffect efektyPodwodne;

    /** Stopien podzia³u obrazu (skala od 0 do 1).*/
    public float poziomPodzialuEkranu;

    void Start() {
        trans = GetComponent<Transform>();
        kameraGlowna = GetComponent<Camera>();

        //Pobieram obiekt Transform kamery podwodnej.
        kamWWodzieTrans = trans.FindChild("KameraWWodzie");
        //Pobieram komponent kamery z obiektu kamery podwodnej.
        kameraWWodzie = kamWWodzieTrans.GetComponent<Camera>();

        //Domyœlnie kamera pod wod¹ wy³aczona.
        kamWWodzieTrans.gameObject.SetActive(false);
        //Pobieram komponent z kamery podwodnej odpowiedzialny za efekty podwodne. 
        efektyPodwodne = kamWWodzieTrans.GetComponent<waterEffect>();
    }

    void Update() {
        //Ustawiam podzia³ ekranu.
        ustawPodzial();
        //W³aczam efekty przebywania pod wod¹.
        wlaczEfektWody();

        /*Sprawdzam czy gracz znajduje siê w wodzie oraz czy kamera jest czêœciowo zanurzona.
          Je¿eli kamera zanurzona to mo¿na dzieliæ ekran.*/
        if (getWodaTrans() != null && czyUstawicPodzialuEkranu()) {

            if (poziomPodzialuEkranu > 0 && poziomPodzialuEkranu < 1) {
                ustawPodzialEkranu();
            }
        } else { // Gracz wyszed³ z wody. Przywrócenie ustawieñ domyœlnych.
            resetKameryGlownej();
            poziomPodzialuEkranu = 0;
        }
    }

    /**
     * Metoda odpowiedzialna za podzia³ ekranu w zale¿noœci od stopnia zanurzenia kamery.
     */
    void ustawPodzialEkranu() {
        Camera zanurzonaKamera = kameraWWodzie;

        float polowaWysokosci = Mathf.Tan(zanurzonaKamera.fieldOfView * Mathf.Deg2Rad * .5f) * zanurzonaKamera.nearClipPlane;
        float gornaCzescGory = polowaWysokosci;
        float dolnaczescGory = (poziomPodzialuEkranu - .5f) * polowaWysokosci * 2f;
        float gornaCzescDolu = dolnaczescGory;
        float dolnaCzescDolu = -polowaWysokosci;

        Matrix4x4 macierzKameryGlownej = kameraGlowna.projectionMatrix;
        macierzKameryGlownej[1, 1] = (2f * zanurzonaKamera.nearClipPlane) / (gornaCzescGory - dolnaczescGory);
        macierzKameryGlownej[1, 2] = (gornaCzescGory + dolnaczescGory) / (gornaCzescGory - dolnaczescGory);

        Matrix4x4 macierzKameryDolnej = zanurzonaKamera.projectionMatrix;
        macierzKameryDolnej[1, 1] = (2f * zanurzonaKamera.nearClipPlane) / (gornaCzescDolu - dolnaCzescDolu);
        macierzKameryDolnej[1, 2] = (gornaCzescDolu + dolnaCzescDolu) / (gornaCzescDolu - dolnaCzescDolu);

        kameraGlowna.projectionMatrix = macierzKameryGlownej;
        zanurzonaKamera.projectionMatrix = macierzKameryDolnej;

        Rect rectDlaKamDolnej = zanurzonaKamera.rect;
        rectDlaKamDolnej.height = poziomPodzialuEkranu;
        zanurzonaKamera.rect = rectDlaKamDolnej;

        Rect rectDlaKamGornej = kameraGlowna.rect;
        rectDlaKamGornej.height = 1f - poziomPodzialuEkranu;
        rectDlaKamGornej.y = poziomPodzialuEkranu;
        kameraGlowna.rect = rectDlaKamGornej;
    }

    /**
     * Metoda wylicza miejsce podzia³u ekranu na zanurzon¹ i nad wodn¹.
     */
    private void ustawPodzial() {

        if (getWodaTrans() != null) { //Gracz w wodzie.

            if (czyUstawicPodzialuEkranu()) { //Kamera czêœciowo zanurzona.

                //Pobieram rozmiar ekranu na podstawie górnej i dolnej krawêdzi.
                float rozmiarEkranu = Vector3.Distance(getDolnaCzescKamery(), getGornaCzescKamery());

                //Obliczam kierunek dla promienia wykrywaj¹cego wodê.
                Vector3 kierunekPromienia = getDolnaCzescKamery() - getGornaCzescKamery();

                //Tworzê promien wykrywaj¹cy stopieñ zanurzenia w wodzie.
                Ray ray = new Ray(getGornaCzescKamery(), kierunekPromienia);

                //Zwróci informacjê o stopniu zanurzenia.
                RaycastHit hitInfo;

                /*Kamera znajduje siê w obiekcie gracza a zatem nie mo¿na dopuœciæ, aby promieñ wchodzi³ w kolizjê
                z graczem. Tworze zmienn¹ z informacja o ignorowaniu warstwy gracza.*/
                int warstwaDoIgnorowania = ~(1 << 8);

                //Sprawdzam czy promieñ natrafil na wodê.
                if (Physics.Raycast(ray, out hitInfo, rozmiarEkranu, warstwaDoIgnorowania)) {

                    //Pobranie informacji o trafionym obiekcie.
                    Vector3 hitPoint = hitInfo.point;
                    Debug.DrawRay(getGornaCzescKamery(), hitPoint - getGornaCzescKamery(), Color.red);

                    /*Odleg³oœæ od punktu trafienia promienia w wodê do dolnej krawêdzi kamery jest 
                      wielkoœci¹ zanurzenia kamery w wodzie.*/
                    float zanurzenie = Vector3.Distance(getDolnaCzescKamery(), hitPoint);

                    //Debug.Log("hitPoint["+ hitPoint + "], X ["+dis+"], A["+ getDolnaCzescKamery() + "], C["+ getGornaCzescKamery() + "], AC="+ rozmiarEkranu + "");

                    //Procêtowe zanurzenie kamery w wodzie (od 0 do 1).
                    poziomPodzialuEkranu = zanurzenie / rozmiarEkranu;
                }

            } else if (getDolnaCzescKamery().y > getWodaTrans().position.y) { //Gracz dalej w wodzie ale kamera nie zanurzona.
                //Przywracam domyœlne ustawienia obydwu kamer.
                resetKameryGlownej();
                resetKameryWody();
                poziomPodzialuEkranu = 0;
            } else if (getGornaCzescKamery().y < getWodaTrans().position.y) { //Ca³kowite zanurzenie kamery.
                //Przywracam domyœlne ustawienia obydwu kamer.
                resetKameryGlownej();
                resetKameryWody();
                poziomPodzialuEkranu = 1;
            }
        }
    }

    /**
     * Funkcja dostarcza informacjê o tym czy ma nast¹piæ ustawianie podzia³u ekranu czy kamera jest czêœciowo zanurzona.
     */
    private bool czyUstawicPodzialuEkranu() {
        return getDolnaCzescKamery().y < getWodaTrans().position.y && getGornaCzescKamery().y > getWodaTrans().position.y;
    }

    /**
     * Funkcja zwraca polozenie dolnej krawedzi kamery(plan bliski) w œwiecie;
     * Funkcja zwraca dolny lewy róg.
     */
    private Vector3 getDolnaCzescKamery() {
        Vector3 p = kameraGlowna.ScreenToWorldPoint(new Vector3(0, 0, kameraGlowna.nearClipPlane));
        return p;
    }

    /**
     * Funkcja zwraca polozenie górnej krawedzi kamery(plan bliski) w œwiecie;
     * Funkcja zwraca lewy górny róg.
     */
    private Vector3 getGornaCzescKamery() {
        Vector3 p = kameraGlowna.ScreenToWorldPoint(new Vector3(0, Screen.height - 1, kameraGlowna.nearClipPlane));
        return p;
    }

    /**
     * Przywrócenie domyœlnych/wyjœciowych ustawieñ kamyry g³ównej.
     */
    private void resetKameryGlownej() {
        Rect rect = new Rect(0f, 0f, 1f, 1f);
        Camera.main.rect = rect;
        kameraGlowna.ResetProjectionMatrix();
    }

    /**
     * Przywrócenie domyœlnych/wyjœciowych ustawieñ kamyry zanurzonej w wodzie.
     */
    private void resetKameryWody() {
        Rect rect = new Rect(0f, 0f, 1f, 1f);
        kameraWWodzie.rect = rect;
        kameraWWodzie.ResetProjectionMatrix();
    }

    private void wlaczEfektWody() {
        if (getWodaTrans() != null) {
            if (getDolnaCzescKamery().y < getWodaTrans().position.y) {
                efektyPodwodne.fog = true;
            } else {
                efektyPodwodne.fog = false;
            }
        }
    }

    

    /**
     * Dostarcza obiekt Transform wody.
     */
    private Transform getWodaTrans() {
        if (woda != null) {
            return woda.GetComponent<Transform>();
        }
        return null;
    }

    /**
     * Pozwala na przekazanie do skryptu obiektu wody, w której znalaz³ siê gracz.
     */
    public void setWoda(GameObject woda) {
        this.woda = woda;
    }
}
