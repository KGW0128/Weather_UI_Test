using System;

/// <summary>
/// ��ü ���� ������ ���� (OpenWeatherMap API ���信 ����)
/// </summary>
[Serializable]
public class WeatherData
{
    public Coord coord;           // ����, �浵 ����
    public Weather[] weather;    // ���� ���� �迭 (���� �ϳ�)
    public string station;       // base �ʵ� �̸� ���� (reserved keyword ������)
    public Main main;            // �ֿ� ��� ���� (�µ�, ���� ��)
    public int visibility;       // ���ðŸ� (���� ����)
    public Wind wind;            // �ٶ� ����
    public Clouds clouds;        // ���� �� ����
    public int dt;               // ������ ��� �ð� (���н� Ÿ��)
    public Sys sys;              // �ý��� ���� ���� (����, ����/�ϸ� �ð� ��)
    public int id;               // ���� ID
    public string name;          // ���ø�
    public int cod;              // ���� �ڵ�
}

/// <summary>
/// ���� ������ ���� (WeatherUI ��� ���)
/// </summary>
[Serializable]
public enum WeatherIconType
{
    Sun,            // ���� ��, ��
    Moon,           // ���� ��, ��
    Cloud,          // ���� �� ����
    CloudLightning, // õ�չ���
    CloudRain,      // �� �Ǵ� �̽���
    CloudSnow,      // ��
    Squall,         // �ٶ� �Ǵ� �Ȱ�/�帲

    Smoke,          // ����
    Haze,           // ����
    Dust,           // ����
    Fog,            // �Ȱ�
    Tornado         // ����̵�
}

/// <summary>
/// ��ġ ��ǥ (����, �浵)
/// </summary>
[Serializable]
public class Coord
{
    public float lon;   // �浵
    public float lat;   // ����
}

/// <summary>
/// ���� ���� ���� ����
/// </summary>
[Serializable]
public class Weather
{
    public int id;              // ���� ���� ID
    public string main;         // ���� �׷� �̸� (��: Rain, Snow)
    public string description;  // �� ����
    public string icon;         // ������ �ڵ�
}

/// <summary>
/// �ٶ� ����
/// </summary>
[Serializable]
public class Wind
{
    public float speed; // ǳ�� (m/s)
    public float deg;   // ǳ�� (��)
}

/// <summary>
/// �ֿ� ��� ����
/// </summary>
[Serializable]
public class Main
{
    public float temp;       // ���� �µ� (����)
    public int pressure;     // ��� (hPa)
    public int humidity;     // ���� (%)
    public float temp_min;   // ���� �µ�
    public float temp_max;   // �ְ� �µ�
}

/// <summary>
/// ���� �� ����
/// </summary>
[Serializable]
public class Clouds
{
    public int all; // ���� �� (%)
}

/// <summary>
/// �ý��� ���� (���� �ڵ�, ����/�ϸ� �ð� ��)
/// </summary>
[Serializable]
public class Sys
{
    public int type;      // �ý��� Ÿ��
    public int id;        // �ý��� ID
    public float message; // �޽��� (�߰� ����)
    public string country;// ���� �ڵ� (��: CA)
    public int sunrise;   // ���� �ð� (���н� Ÿ��)
    public int sunset;    // �ϸ� �ð� (���н� Ÿ��)
}
