using System.Collections.Generic;
using CsvHelper.Configuration.Attributes; // [Name] 속성 사용 위해 추가

// struct 대신 class로 변경하고 CsvHelper 매핑 속성 추가
public class CarData // 클래스 이름 변경 (선택적이지만 권장)
{
    // CSV 헤더와 이름이 동일한 경우 [Name] 불필요
    public int Num { get; set; } // Num 열 추가 (CSV에 있으므로)
    public string Name { get; set; }
    public string fileName { get; set; }
    public float Mass { get; set; } // CSV는 정수(Mass)지만 float으로 받기 가능

    [Name("dragCoefficient")] // 명확성을 위해 추가 (선택적)
    public float dragCoefficient { get; set; }

    [Name("baseEngineAcceleration")]
    public float baseEngineAcceleration { get; set; }

    public float maxEngineRPM { get; set; }
    public float minEngineRPM { get; set; }
    public short lastGear { get; set; }

    // gearRatio와 gearSpeedLimit은 CSV에 리스트 형태가 아니므로
    // 직접 매핑하기 어렵습니다. 이전 CarSpecRecord처럼 개별 속성으로 받거나,
    // 로드 후 별도로 리스트를 구성해야 합니다.
    // 여기서는 CarSpecRecord와 유사하게 개별 속성으로 받겠습니다.
    // 만약 Car_data 구조체의 List<float> 형태를 유지하고 싶다면,
    // LoadCarData에서 CarSpecRecord로 읽은 후 Car_data 리스트로 변환해야 합니다.

    [Name("gearRatio.eGEAR_REVERSE")]
    public float gearRatio_eGEAR_REVERSE { get; set; }

    [Name("gearRatio.eGEAR_FIRST")]
    public float gearRatio_eGEAR_FIRST { get; set; }

    [Name("gearRatio.eGEAR_SECOND")]
    public float gearRatio_eGEAR_SECOND { get; set; }

    [Name("gearRatio.eGEAR_THIRD")]
    public float gearRatio_eGEAR_THIRD { get; set; }

    [Name("gearRatio.eGEAR_FOURTH")]
    public float gearRatio_eGEAR_FOURTH { get; set; }

    [Name("gearRatio.eGEAR_FIFTH")]
    public float gearRatio_eGEAR_FIFTH { get; set; }

    [Name("gearRatio.eGEAR_SIXTH")]
    public float gearRatio_eGEAR_SIXTH { get; set; }

    public float finalDriveRatio { get; set; }

    [Name("gearSpeedLimit.eGEAR_REVERSE")]
    public float gearSpeedLimit_eGEAR_REVERSE { get; set; }

    // 'gearSpeedLimite.GEAR_FIRST' 오타 주의! CSV 파일 실제 헤더 확인 필요
    [Name("gearSpeedLimite.GEAR_FIRST")]
    public float gearSpeedLimite_GEAR_FIRST { get; set; }

    [Name("gearSpeedLimit.eGEAR_SECOND")]
    public float gearSpeedLimit_eGEAR_SECOND { get; set; }

    [Name("gearSpeedLimit.eGEAR_THIRD")]
    public float gearSpeedLimit_eGEAR_THIRD { get; set; }

    [Name("gearSpeedLimit.eGEAR_FOURTH")]
    public float gearSpeedLimit_eGEAR_FOURTH { get; set; }

    [Name("gearSpeedLimit.eGEAR_FIFTH")]
    public float gearSpeedLimit_eGEAR_FIFTH { get; set; }

    [Name("gearSpeedLimit.eGEAR_SIXTH")]
    public float gearSpeedLimit_eGEAR_SIXTH { get; set; }

    // 만약 기존 Car_data 구조체의 List<float> 형태를 꼭 사용해야 한다면,
    // 이 클래스를 사용하지 않고, 이전 답변의 CarSpecRecord로 읽은 후
    // CarData_Manager에서 Car_data 리스트로 변환하는 로직을 추가해야 합니다.
    // 예:
    // public List<float> gearRatio { get; set; }
    // public List<float> gearSpeedLimit { get; set; }
    // 이 필드들은 CsvHelper가 자동으로 채울 수 없으므로 [Ignore] 속성 필요
}