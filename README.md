# 🌤️ Weather_UI_Test

Unity를 활용한 날씨 UI 테스트 프로젝트입니다.  
OpenWeatherMap API를 통해 실시간 날씨 정보를 받아와 Unity UI에 표시하는 기능을 구현합니다.

---

## 🧩 프로젝트 정보

- **프로젝트명**: `Weather_UI_Test`
- **Unity 버전**: `2020.3.50f1` (2D 기반)
- **API 서비스**: [OpenWeatherMap 무료 버전](https://openweathermap.org/current)
- **API 키 제공 계정**: Tyler의 Google 계정
- **주요 기능**:
  - 도시별 실시간 날씨 정보 호출
  - 온도, 습도, 날씨 상태 등의 정보 UI 표시
  - UnityWebRequest를 통한 비동기 API 통신 구현
- **추가 정보**:
  - 캐나다 Calgary
  - 썹씨기준 데이터 받아옴

---

## 🔗 주요 참조

- Velog 튜토리얼  
  👉 [Unity 날씨 API 연동하면서 UnityWebRequest 알아보기](https://velog.io/@szun8/Unity%EB%82%A0%EC%94%A8-API-%EC%97%B0%EB%8F%99%ED%95%98%EB%A9%B4%EC%84%9C-UnityWebRequest-%EC%95%8C%EC%95%84%EB%B3%B4%EA%B8%B0)

---


## 📝 기타 참고 사항

- 무료 API는 초당 호출 제한이 있으므로 테스트 중에는 주기적 호출 간격을 두어야 합니다.(1시간)
- API 키는 보안상 외부 저장 파일이나 환경 변수에서 불러오는 방식 권장.
