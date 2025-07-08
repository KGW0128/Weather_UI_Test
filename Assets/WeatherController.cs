using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherController : MonoBehaviour
{
    // Tyler 계정 사용
    // API 주소 (https 사용)
    private const string API_ADDRESS = @"https://api.openweathermap.org/data/2.5/weather?q=Calgary&appid=bd2b3cf96ff93eb638d95712dabb1784&units=metric";

    // 최대 재시도 횟수 및 재시도 간격 (초)
    private const int MaxRetryCount = 3;
    private const float RetryDelaySeconds = 2f;

    // 날씨 데이터 콜백 타입 (성공 시)
    public delegate void WeatherDataCallback(WeatherData weatherData);

    // 에러 발생 시 콜백 타입
    public delegate void WeatherErrorCallback(string errorMessage);

    // 캐싱용 날씨 데이터
    private WeatherData _weatherData;

    /// <summary>
    /// 날씨 데이터 요청
    /// </summary>
    /// <param name="onSuccess">데이터 수신 콜백</param>
    /// <param name="onError">오류 발생 콜백 (옵션)</param>
    /// <param name="forceRefresh">true면 캐시 무시, API 재호출</param>
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
    /// 캐시 무효화
    /// </summary>
    public void ClearCache()
    {
        _weatherData = null;
    }

    /// <summary>
    /// API로부터 날씨 데이터 받아오기 코루틴 (재시도 포함)
    /// </summary>
    private IEnumerator CoGetWeather(
        WeatherDataCallback onSuccess,
        WeatherErrorCallback onError)
    {
        int attempt = 0;

        while (attempt < MaxRetryCount)
        {
            attempt++;
            Debug.Log($"날씨 정보 다운로드 시도 {attempt}회");

            using (UnityWebRequest webRequest = UnityWebRequest.Get(API_ADDRESS))
            {
                webRequest.timeout = 10; // 10초 타임아웃 설정
                yield return webRequest.SendWebRequest();

                if (IsNetworkError(webRequest))
                {
                    if (HandleRetryOrFail(attempt, onError, webRequest.error, "네트워크 오류"))
                        yield break;
                    yield return new WaitForSeconds(RetryDelaySeconds);
                    continue;
                }

                if (webRequest.responseCode != 200)
                {
                    if (HandleRetryOrFail(attempt, onError, $"HTTP 오류 코드: {webRequest.responseCode}", "HTTP 오류"))
                        yield break;
                    yield return new WaitForSeconds(RetryDelaySeconds);
                    continue;
                }

                string downloadedTxt = webRequest.downloadHandler.text;
                Debug.Log("날씨 정보가 다운로드 되었습니다! : " + downloadedTxt);

                // "base" 필드가 예약어라 "station"으로 임시 치환 후 파싱
                string weatherStr = downloadedTxt.Replace("\"base\"", "\"station\"");

                WeatherData parsedData = JsonUtility.FromJson<WeatherData>(weatherStr);

                if (parsedData == null || parsedData.weather == null || parsedData.weather.Length == 0)
                {
                    Debug.LogError("파싱된 날씨 데이터가 유효하지 않습니다.");
                    onError?.Invoke("파싱된 날씨 데이터가 유효하지 않습니다.");
                    yield break;
                }

                _weatherData = parsedData;
                onSuccess?.Invoke(_weatherData);
                yield break;
            }
        }
    }

    // 네트워크 또는 프로토콜 오류 여부 판단
    private bool IsNetworkError(UnityWebRequest request)
    {
        return request.result == UnityWebRequest.Result.ConnectionError ||
               request.result == UnityWebRequest.Result.ProtocolError;
    }

    // 재시도 가능하면 재시도, 초과시 에러 콜백 후 종료 신호 반환
    private bool HandleRetryOrFail(int attempt, WeatherErrorCallback onError, string errorMsg, string logPrefix)
    {
        Debug.LogWarning($"{logPrefix} 발생: {errorMsg}");

        if (attempt >= MaxRetryCount)
        {
            Debug.LogError("최대 재시도 횟수 초과. 다운로드 실패.");
            onError?.Invoke(errorMsg);
            return true; // 중단 신호
        }

        return false; // 재시도 가능
    }
}
