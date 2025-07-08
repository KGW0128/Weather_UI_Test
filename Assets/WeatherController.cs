using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherController : MonoBehaviour
{
    // Tyler ���� ���
    // API �ּ� (https ���)
    private const string API_ADDRESS = @"https://api.openweathermap.org/data/2.5/weather?q=Calgary&appid=bd2b3cf96ff93eb638d95712dabb1784&units=metric";

    // �ִ� ��õ� Ƚ�� �� ��õ� ���� (��)
    private const int MaxRetryCount = 3;
    private const float RetryDelaySeconds = 2f;

    // ���� ������ �ݹ� Ÿ�� (���� ��)
    public delegate void WeatherDataCallback(WeatherData weatherData);

    // ���� �߻� �� �ݹ� Ÿ��
    public delegate void WeatherErrorCallback(string errorMessage);

    // ĳ�̿� ���� ������
    private WeatherData _weatherData;

    /// <summary>
    /// ���� ������ ��û
    /// </summary>
    /// <param name="onSuccess">������ ���� �ݹ�</param>
    /// <param name="onError">���� �߻� �ݹ� (�ɼ�)</param>
    /// <param name="forceRefresh">true�� ĳ�� ����, API ��ȣ��</param>
    public void GetWeather(
        WeatherDataCallback onSuccess,
        WeatherErrorCallback onError = null,
        bool forceRefresh = false)
    {
        if (_weatherData == null || forceRefresh)
        {
            StartCoroutine(CoGetWeather(onSuccess, onError));
        }
        else
        {
            onSuccess?.Invoke(_weatherData);
        }
    }

    /// <summary>
    /// ĳ�� ��ȿȭ
    /// </summary>
    public void ClearCache()
    {
        _weatherData = null;
    }

    /// <summary>
    /// API�κ��� ���� ������ �޾ƿ��� �ڷ�ƾ (��õ� ����)
    /// </summary>
    private IEnumerator CoGetWeather(
        WeatherDataCallback onSuccess,
        WeatherErrorCallback onError)
    {
        int attempt = 0;

        while (attempt < MaxRetryCount)
        {
            attempt++;
            Debug.Log($"���� ���� �ٿ�ε� �õ� {attempt}ȸ");

            using (UnityWebRequest webRequest = UnityWebRequest.Get(API_ADDRESS))
            {
                webRequest.timeout = 10; // 10�� Ÿ�Ӿƿ� ����
                yield return webRequest.SendWebRequest();

                if (IsNetworkError(webRequest))
                {
                    if (HandleRetryOrFail(attempt, onError, webRequest.error, "��Ʈ��ũ ����"))
                        yield break;
                    yield return new WaitForSeconds(RetryDelaySeconds);
                    continue;
                }

                if (webRequest.responseCode != 200)
                {
                    if (HandleRetryOrFail(attempt, onError, $"HTTP ���� �ڵ�: {webRequest.responseCode}", "HTTP ����"))
                        yield break;
                    yield return new WaitForSeconds(RetryDelaySeconds);
                    continue;
                }

                string downloadedTxt = webRequest.downloadHandler.text;
                Debug.Log("���� ������ �ٿ�ε� �Ǿ����ϴ�! : " + downloadedTxt);

                // "base" �ʵ尡 ������ "station"���� �ӽ� ġȯ �� �Ľ�
                string weatherStr = downloadedTxt.Replace("\"base\"", "\"station\"");

                WeatherData parsedData = JsonUtility.FromJson<WeatherData>(weatherStr);

                if (parsedData == null || parsedData.weather == null || parsedData.weather.Length == 0)
                {
                    Debug.LogError("�Ľ̵� ���� �����Ͱ� ��ȿ���� �ʽ��ϴ�.");
                    onError?.Invoke("�Ľ̵� ���� �����Ͱ� ��ȿ���� �ʽ��ϴ�.");
                    yield break;
                }

                _weatherData = parsedData;
                onSuccess?.Invoke(_weatherData);
                yield break;
            }
        }
    }

    // ��Ʈ��ũ �Ǵ� �������� ���� ���� �Ǵ�
    private bool IsNetworkError(UnityWebRequest request)
    {
        return request.result == UnityWebRequest.Result.ConnectionError ||
               request.result == UnityWebRequest.Result.ProtocolError;
    }

    // ��õ� �����ϸ� ��õ�, �ʰ��� ���� �ݹ� �� ���� ��ȣ ��ȯ
    private bool HandleRetryOrFail(int attempt, WeatherErrorCallback onError, string errorMsg, string logPrefix)
    {
        Debug.LogWarning($"{logPrefix} �߻�: {errorMsg}");

        if (attempt >= MaxRetryCount)
        {
            Debug.LogError("�ִ� ��õ� Ƚ�� �ʰ�. �ٿ�ε� ����.");
            onError?.Invoke(errorMsg);
            return true; // �ߴ� ��ȣ
        }

        return false; // ��õ� ����
    }
}
