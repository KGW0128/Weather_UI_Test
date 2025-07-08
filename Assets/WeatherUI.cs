using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Globalization;

/// <summary>
/// Calgary �ð� ���� 1�ð����� ���� ���� �� UI ǥ��
/// </summary>
public class WeatherUI : MonoBehaviour
{
    [Header("���� ������ ��Ʈ�ѷ�")]
    public WeatherController weatherController;

    [Header("UI Elements")]
    public Image weatherIconImage;
    public TextMeshProUGUI temperatureText;
    public TextMeshProUGUI timeText;

    private TimeZoneInfo calgaryTimeZone;
    private Coroutine updateCoroutine = null;  // �ڷ�ƾ �ߺ� ������ �ڵ�

    private void Start()
    {
        // Calgary �ð��� ���� �ʱ�ȭ
        calgaryTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");

        // ���� �� �ð� ���� ����Ŭ ����
        StartWeatherUpdateCycle();
    }

    // ================================================ �ð� ���� �޼��� ================================================

    /// <summary>
    /// 1�ð� ���� ���ÿ� ���� ���� �� �ð� ���� ����Ŭ ����
    /// </summary>
    private void StartWeatherUpdateCycle()
    {
        // �ڷ�ƾ �ߺ� ����
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }

        // �ð� UI ���� ����
        UpdateTimeUI();

        // ���� ������ ĳ�� ��ȿȭ �� ���� API ȣ��� �ֽ� ������ ��û
        weatherController.ClearCache();
        weatherController.GetWeather(OnWeatherReceived, forceRefresh: true);

        // ���� ���ñ��� ���� �ð� ��� �� ��� �� ��� ȣ�� ����
        float waitSeconds = CalculateSecondsUntilNextUpdate();
        updateCoroutine = StartCoroutine(WaitAndRepeatUpdate(waitSeconds));
    }

    /// <summary>
    /// Calgary ���� �ð����� ���� �ð� UI ���� (12�ð��� AM/PM)
    /// </summary>
    private void UpdateTimeUI()
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, calgaryTimeZone);

        // 12�ð��� + AM/PM ǥ�� �������� ����
        timeText.text = localTime.ToString("yyyy-MM-dd hh:mm:ss tt", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// ���� 1�ð� ���ñ��� ���� �ð�(��) ���
    /// </summary>
    private float CalculateSecondsUntilNextUpdate()
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, calgaryTimeZone);

        int nextHour = (localTime.Hour + 1) % 24;

        DateTime nextUpdateLocal = new DateTime(
            localTime.Year,
            localTime.Month,
            localTime.Day,
            nextHour,
            0,
            0
        );

        // ���� ���� �ð��� ���� ���ÿ� ���ų� �������� ���� �ð����� �� �ð� �� �߰�
        if (nextUpdateLocal <= localTime)
        {
            nextUpdateLocal = nextUpdateLocal.AddHours(1);
        }

        // �ٽ� UTC�� ��ȯ�Ͽ� ���� �ð� ���
        DateTime nextUpdateUtc = TimeZoneInfo.ConvertTimeToUtc(nextUpdateLocal, calgaryTimeZone);
        double secondsUntilNextUpdate = (nextUpdateUtc - utcNow).TotalSeconds;

        Debug.Log($"[WeatherUI] ���� ���ű��� ���� �ð�(��): {secondsUntilNextUpdate}");

        return (float)secondsUntilNextUpdate;
    }

    /// <summary>
    /// ���� �ð� ���� 1�ʸ��� �ð� UI ����, ���� ���� ����Ŭ �����
    /// </summary>
    private IEnumerator WaitAndRepeatUpdate(float waitSeconds)
    {
        float elapsed = 0f;

        while (elapsed < waitSeconds)
        {
            UpdateTimeUI();

            yield return new WaitForSeconds(1f);

            elapsed += 1f;
        }

        // ��� �� ���� ����Ŭ �ٽ� ����
        StartWeatherUpdateCycle();
    }

    // ================================================ ���� ���� �޼��� ================================================

    /// <summary>
    /// ���� ������ ���� �ݹ� ó�� �� UI ������Ʈ
    /// </summary>
    private void OnWeatherReceived(WeatherData data)
    {
        string report =
            $"����: {data.name}\n" +
            $"��ġ: {data.coord.lat}, {data.coord.lon}\n" +
            $"����: {data.weather[0].main} ({data.weather[0].description})\n" +
            $"�µ�: {data.main.temp}��C (�ּ� {data.main.temp_min}��, �ִ� {data.main.temp_max}��)\n" +
            $"����: {data.main.humidity}%\n" +
            $"ǳ��: {data.wind.speed}m/s\n" +
            $"����: {data.clouds.all}%";

        Debug.Log(report);

        temperatureText.text = $"{data.main.temp:F1}��C";

        Debug.Log($"���� ���� ����: Main = {data.weather[0].main}, Description = {data.weather[0].description}");

        // �ҹ��� ��ȯ �� ������ Ÿ�� ����
        WeatherIconType iconType = MapToWeatherIcon(data.weather[0].main.ToLower(), data.weather[0].description.ToLower());

        Debug.Log($"���ε� ������ Ÿ��: {iconType}");

        SetWeatherIcon(iconType);
    }

    /// <summary>
    /// ���� ���� ���ڿ�(mainWeather)�� ���� ����(description)�� �������
    /// �˸��� WeatherIconType�� ��ȯ�մϴ�.
    /// switch������ mainWeather ���� �б� ó���ϸ�,
    /// "clear"�� ��� description�� "night" ���� ���η� ��/�� ������ ����.
    /// </summary>
    private WeatherIconType MapToWeatherIcon(string mainWeather, string description)
    {
        switch (mainWeather)
        {
            case "clear": // ����
                {
                    // "clear"�� ��� ������ ������ description���� �Ǵ�
                    return description.Contains("night") ? WeatherIconType.Moon : WeatherIconType.Sun;
                }

            case "clouds": // ��������
                {
                    return WeatherIconType.Cloud;
                }

            case "thunderstorm": // õ�չ���
                {
                    return WeatherIconType.CloudLightning;
                }

            case "rain":    // ��
            case "drizzle": // �̽���
                {
                    return WeatherIconType.CloudRain;
                }

            case "snow":    // ��
                {
                    return WeatherIconType.CloudSnow;
                }

            case "mist":   // �Ȱ�
            case "smoke":  // ����
            case "haze":   // ����
            case "dust":   // ����
            case "fog":    // �Ȱ�
                {
                    return WeatherIconType.Squall;
                }

            case "squall": // ��ǳ
                {
                    return WeatherIconType.Squall;
                }

            case "tornado": // ����̵�
                {
                    return WeatherIconType.Tornado; // ����̵� �������� ������ õ�չ����� ��ü
                }

            default:
                {
                    // ��õ� ���̽��� ������ �⺻���� ���� ������ ���
                    return WeatherIconType.Cloud;
                }
        }
    }

    /// <summary>
    /// ������ Ÿ�Կ� �´� ��������Ʈ�� Resources���� �ҷ��� ����
    /// </summary>
    private void SetWeatherIcon(WeatherIconType type)
    {
        string iconName = type.ToString();

        Sprite iconSprite = Resources.Load<Sprite>($"WeatherIcon/{iconName}");

        if (iconSprite != null)
        {
            weatherIconImage.sprite = iconSprite;
        }
        else
        {
            Debug.LogWarning($"������ '{iconName}' ��(��) Resources/WeatherIcon �������� ã�� �� �����ϴ�.");
        }
    }
}
