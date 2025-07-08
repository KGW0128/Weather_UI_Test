using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Globalization;

/// <summary>
/// Calgary 시간 기준 1시간마다 날씨 갱신 및 UI 표시
/// </summary>
public class WeatherUI : MonoBehaviour
{
    [Header("날씨 데이터 컨트롤러")]
    public WeatherController weatherController;

    [Header("UI Elements")]
    public Image weatherIconImage;
    public TextMeshProUGUI temperatureText;
    public TextMeshProUGUI timeText;

    private TimeZoneInfo calgaryTimeZone;
    private Coroutine updateCoroutine = null;  // 코루틴 중복 방지용 핸들

    private void Start()
    {
        // Calgary 시간대 정보 초기화
        calgaryTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");

        // 날씨 및 시간 갱신 사이클 시작
        StartWeatherUpdateCycle();
    }

    // ================================================ 시간 관련 메서드 ================================================

    /// <summary>
    /// 1시간 단위 정시에 맞춰 날씨 및 시간 갱신 사이클 시작
    /// </summary>
    private void StartWeatherUpdateCycle()
    {
        // 코루틴 중복 방지
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }

        // 시간 UI 먼저 갱신
        UpdateTimeUI();

        // 날씨 데이터 캐시 무효화 후 강제 API 호출로 최신 데이터 요청
        weatherController.ClearCache();
        weatherController.GetWeather(OnWeatherReceived, forceRefresh: true);

        // 다음 정시까지 남은 시간 계산 후 대기 및 재귀 호출 시작
        float waitSeconds = CalculateSecondsUntilNextUpdate();
        updateCoroutine = StartCoroutine(WaitAndRepeatUpdate(waitSeconds));
    }

    /// <summary>
    /// Calgary 현지 시간으로 현재 시간 UI 갱신 (12시간제 AM/PM)
    /// </summary>
    private void UpdateTimeUI()
    {
        DateTime utcNow = DateTime.UtcNow;
        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, calgaryTimeZone);

        // 12시간제 + AM/PM 표시 형식으로 변경
        timeText.text = localTime.ToString("yyyy-MM-dd hh:mm:ss tt", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 다음 1시간 정시까지 남은 시간(초) 계산
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

        // 만약 현재 시간이 다음 정시와 같거나 지났으면 다음 시간으로 한 시간 더 추가
        if (nextUpdateLocal <= localTime)
        {
            nextUpdateLocal = nextUpdateLocal.AddHours(1);
        }

        // 다시 UTC로 변환하여 남은 시간 계산
        DateTime nextUpdateUtc = TimeZoneInfo.ConvertTimeToUtc(nextUpdateLocal, calgaryTimeZone);
        double secondsUntilNextUpdate = (nextUpdateUtc - utcNow).TotalSeconds;

        Debug.Log($"[WeatherUI] 다음 갱신까지 남은 시간(초): {secondsUntilNextUpdate}");

        return (float)secondsUntilNextUpdate;
    }

    /// <summary>
    /// 남은 시간 동안 1초마다 시간 UI 갱신, 이후 갱신 사이클 재시작
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

        // 대기 후 갱신 사이클 다시 시작
        StartWeatherUpdateCycle();
    }

    // ================================================ 날씨 관련 메서드 ================================================

    /// <summary>
    /// 날씨 데이터 수신 콜백 처리 및 UI 업데이트
    /// </summary>
    private void OnWeatherReceived(WeatherData data)
    {
        string report =
            $"도시: {data.name}\n" +
            $"위치: {data.coord.lat}, {data.coord.lon}\n" +
            $"날씨: {data.weather[0].main} ({data.weather[0].description})\n" +
            $"온도: {data.main.temp}°C (최소 {data.main.temp_min}°, 최대 {data.main.temp_max}°)\n" +
            $"습도: {data.main.humidity}%\n" +
            $"풍속: {data.wind.speed}m/s\n" +
            $"구름: {data.clouds.all}%";

        Debug.Log(report);

        temperatureText.text = $"{data.main.temp:F1}°C";

        Debug.Log($"받은 날씨 정보: Main = {data.weather[0].main}, Description = {data.weather[0].description}");

        // 소문자 변환 후 아이콘 타입 매핑
        WeatherIconType iconType = MapToWeatherIcon(data.weather[0].main.ToLower(), data.weather[0].description.ToLower());

        Debug.Log($"매핑된 아이콘 타입: {iconType}");

        SetWeatherIcon(iconType);
    }

    /// <summary>
    /// 날씨 상태 문자열(mainWeather)과 세부 설명(description)을 기반으로
    /// 알맞은 WeatherIconType을 반환합니다.
    /// switch문으로 mainWeather 값을 분기 처리하며,
    /// "clear"의 경우 description에 "night" 포함 여부로 낮/밤 아이콘 구분.
    /// </summary>
    private WeatherIconType MapToWeatherIcon(string mainWeather, string description)
    {
        switch (mainWeather)
        {
            case "clear": // 맑음
                {
                    // "clear"인 경우 밤인지 낮인지 description으로 판단
                    return description.Contains("night") ? WeatherIconType.Moon : WeatherIconType.Sun;
                }

            case "clouds": // 구름많음
                {
                    return WeatherIconType.Cloud;
                }

            case "thunderstorm": // 천둥번개
                {
                    return WeatherIconType.CloudLightning;
                }

            case "rain":    // 비
            case "drizzle": // 이슬비
                {
                    return WeatherIconType.CloudRain;
                }

            case "snow":    // 눈
                {
                    return WeatherIconType.CloudSnow;
                }

            case "mist":   // 안개
            case "smoke":  // 연기
            case "haze":   // 연무
            case "dust":   // 먼지
            case "fog":    // 안개
                {
                    return WeatherIconType.Squall;
                }

            case "squall": // 돌풍
                {
                    return WeatherIconType.Squall;
                }

            case "tornado": // 토네이도
                {
                    return WeatherIconType.Tornado; // 토네이도 아이콘이 없으니 천둥번개로 대체
                }

            default:
                {
                    // 명시된 케이스에 없으면 기본으로 구름 아이콘 사용
                    return WeatherIconType.Cloud;
                }
        }
    }

    /// <summary>
    /// 아이콘 타입에 맞는 스프라이트를 Resources에서 불러와 설정
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
            Debug.LogWarning($"아이콘 '{iconName}' 을(를) Resources/WeatherIcon 폴더에서 찾을 수 없습니다.");
        }
    }
}
