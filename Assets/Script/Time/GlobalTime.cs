using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering.Universal;

public class GlobalTime : MonoBehaviour
{

    public static GlobalTime Instance { get; private set; }
    public GameObject shadow;
    public int minutes;
    public int hours;
    public int days;
    public float tempSecond;
    public bool isDay = false;
    public bool isMorning = false;
    public bool isNight = false;
    public bool isEvening = false;
    public Light2D spotLightPlayer;
    public Light2D spotLight;
    public TextMeshProUGUI time;

    void Start()
    {
        Instance = this;
    }
    void Update()
    {
        tempSecond += Time.deltaTime;
        if (tempSecond >= 1)
        {
            minutes += 1;
            tempSecond = 0;
        }
        _Time();
        time.text = $"{hours:00}:{minutes:00}";
        if (isNight == false)
        {
            spotLight.enabled = false;
            spotLightPlayer.enabled = false;
            
            shadow.SetActive(true);
        }
        else
        {
            spotLight.enabled = true;
            spotLightPlayer.enabled = true;
            shadow.SetActive(false);
        }

    }

    public void _Time()
    {
        if (minutes >= 60)
        {
            hours++;
            minutes = 0;
        }
        if (hours >= 24)
        {
            hours = 0;
            days++;
        }
        isMorning = hours >= 6 && hours <= 11;
        isDay = hours >= 12 && hours <= 17;
        isEvening = hours >= 18 && hours <= 21;
        isNight = hours >= 22 || hours <= 5;
    }

    public bool IsDay()
    {
        return isDay;

    }

    public bool IsMorning()
    {
        return isMorning;
    }

    public bool IsNight()
    {
        return isNight;
    }

    public bool IsEvening()
    {
        return isEvening;
    }
}
