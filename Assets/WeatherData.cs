using System;

/// <summary>
/// 전체 날씨 데이터 구조 (OpenWeatherMap API 응답에 맞춤)
/// </summary>
[Serializable]
public class WeatherData
{
    public Coord coord;           // 위도, 경도 정보
    public Weather[] weather;    // 날씨 상태 배열 (보통 하나)
    public string station;       // base 필드 이름 변경 (reserved keyword 문제로)
    public Main main;            // 주요 기상 정보 (온도, 습도 등)
    public int visibility;       // 가시거리 (미터 단위)
    public Wind wind;            // 바람 정보
    public Clouds clouds;        // 구름 양 정보
    public int dt;               // 데이터 계산 시각 (유닉스 타임)
    public Sys sys;              // 시스템 관련 정보 (국가, 일출/일몰 시간 등)
    public int id;               // 도시 ID
    public string name;          // 도시명
    public int cod;              // 응답 코드
}

/// <summary>
/// 날씨 아이콘 종류 (WeatherUI 등에서 사용)
/// </summary>
[Serializable]
public enum WeatherIconType
{
    Sun,            // 맑은 날, 낮
    Moon,           // 맑은 날, 밤
    Cloud,          // 구름 낀 상태
    CloudLightning, // 천둥번개
    CloudRain,      // 비 또는 이슬비
    CloudSnow,      // 눈
    Squall,         // 바람 또는 안개/흐림

    Smoke,          // 연기
    Haze,           // 연무
    Dust,           // 먼지
    Fog,            // 안개
    Tornado         // 토네이도
}

/// <summary>
/// 위치 좌표 (위도, 경도)
/// </summary>
[Serializable]
public class Coord
{
    public float lon;   // 경도
    public float lat;   // 위도
}

/// <summary>
/// 개별 날씨 상태 정보
/// </summary>
[Serializable]
public class Weather
{
    public int id;              // 날씨 상태 ID
    public string main;         // 날씨 그룹 이름 (예: Rain, Snow)
    public string description;  // 상세 설명
    public string icon;         // 아이콘 코드
}

/// <summary>
/// 바람 정보
/// </summary>
[Serializable]
public class Wind
{
    public float speed; // 풍속 (m/s)
    public float deg;   // 풍향 (도)
}

/// <summary>
/// 주요 기상 정보
/// </summary>
[Serializable]
public class Main
{
    public float temp;       // 현재 온도 (섭씨)
    public int pressure;     // 기압 (hPa)
    public int humidity;     // 습도 (%)
    public float temp_min;   // 최저 온도
    public float temp_max;   // 최고 온도
}

/// <summary>
/// 구름 양 정보
/// </summary>
[Serializable]
public class Clouds
{
    public int all; // 구름 양 (%)
}

/// <summary>
/// 시스템 정보 (국가 코드, 일출/일몰 시간 등)
/// </summary>
[Serializable]
public class Sys
{
    public int type;      // 시스템 타입
    public int id;        // 시스템 ID
    public float message; // 메시지 (추가 정보)
    public string country;// 국가 코드 (예: CA)
    public int sunrise;   // 일출 시각 (유닉스 타임)
    public int sunset;    // 일몰 시각 (유닉스 타임)
}
