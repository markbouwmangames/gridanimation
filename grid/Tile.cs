using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Tile : MonoBehaviour {
    public bool IsEnabled = true;
    public int X;
    public int Y;

    public Element OrbElement { get; private set; }
    
    private Vector3 startPos;

    private float currentBrightness = 0.8f;
    private float tempBrightness;
    private float offBrightness = 0.8f;

    private float minBrightnessOffset = -0.15f;
    private float maxBrightnessOffset = 0.25f;

    private float brightnessUpdateTime = 0;
    private float minBrightnessUpdateTime = 0.5f;
    private float maxBrightnessUpdateTime = 2f;

    private int numGhosts = 2;
    private float ghostUpdateTime = 0.0125f;

    public Vector3 GetScreenPos() {
        return Camera.main.WorldToScreenPoint(startPos);
    }

    public void On(Element element, float brightness) {
        IsEnabled = true;

        OrbElement = element;
        currentBrightness = brightness;
        tempBrightness = currentBrightness;
        SetRenderer();
    }

    public void Off() {
        IsEnabled = false;

        OrbElement = Element.NONE;
        currentBrightness = offBrightness;
        tempBrightness = currentBrightness;
        SetRenderer();
    }

    public void Ping(Element element) {
        StartCoroutine(PingRoutine(element));
        IsEnabled = false;
    }

    private void Awake() {
        startPos = transform.position;
        Off();
        brightnessUpdateTime = Random.Range(maxBrightnessUpdateTime, maxBrightnessUpdateTime);
    }

    private void Update() {
        if (brightnessUpdateTime > 0) {
            brightnessUpdateTime -= Time.deltaTime;

            if (brightnessUpdateTime < 0) {
                brightnessUpdateTime = Random.Range(minBrightnessUpdateTime, maxBrightnessUpdateTime);
                currentBrightness = tempBrightness + Random.Range(minBrightnessOffset, maxBrightnessOffset);
                SetTileBrightness();
            }
        }
    }

    private IEnumerator PingRoutine(Element element) {
        On(element, 0.8f);

        float maxBrightness = currentBrightness;
        float step = maxBrightness / (numGhosts + 1);

        while (currentBrightness > 0) {
            yield return new WaitForSeconds(ghostUpdateTime);
            currentBrightness -= step;
            tempBrightness = currentBrightness;
            SetTileBrightness();
        }

        Off();
    }
    
    private void SetRenderer() {
        Color color = new Color(currentBrightness, currentBrightness, currentBrightness, 1f);
        Color elementColor = ElementColorUtil.GetElementColor(OrbElement).Color;
        float gridIntensity = 0;

        if (OrbElement == Element.NONE) {
            gridIntensity = 0.9f;
        } else {
            gridIntensity = 0.1f;
        }

        Set(color, elementColor, gridIntensity);
    }

    private void Set(Color color, Color fresnel, float gridIntensity) {
        GetComponent<Renderer>().material.SetColor("_Color", color);
        GetComponent<Renderer>().material.SetColor("_Fresnel", fresnel);
        GetComponent<Renderer>().material.SetFloat("_GridIntensity", gridIntensity);
    }

    private void SetTileBrightness() {
        Color color = new Color(currentBrightness, currentBrightness, currentBrightness, 1f);
        GetComponent<Renderer>().material.SetColor("_Color", color);
    }
}
