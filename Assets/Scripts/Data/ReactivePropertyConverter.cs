using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// ReactiveProperty<T> 클래스를 Newtonsoft.Json으로 직렬화/역직렬화하기 위한 사용자 정의 컨버터입니다.
/// </summary>
public class ReactivePropertyConverter : JsonConverter
{
    /// <summary>
    /// 이 컨버터가 특정 타입을 처리할 수 있는지 여부를 결정합니다.
    /// </summary>
    public override bool CanConvert(Type objectType)
    {
        // 제네릭 타입인지 확인하고, 그 제네릭 정의가 ReactiveProperty<>와 일치하는지 검사합니다.
        return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ReactiveProperty<>);
    }

    /// <summary>
    /// JSON에서 객체를 읽어 ReactiveProperty<T> 인스턴스를 생성합니다.
    /// </summary>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // JSON 토큰에서 값을 로드합니다. (예: 10, "some_string", true)
        JToken token = JToken.Load(reader);

        // ReactiveProperty<T>의 제네릭 타입 인자(예: int, string)를 가져옵니다.
        Type valueType = objectType.GetGenericArguments()[0];

        // 토큰의 값을 실제 타입(valueType)으로 변환합니다.
        object value = token.ToObject(valueType, serializer);

        // 변환된 값을 사용하여 ReactiveProperty<T>의 새 인스턴스를 생성하여 반환합니다.
        // Activator.CreateInstance를 사용하여 제네릭 객체를 동적으로 생성합니다.
        return Activator.CreateInstance(objectType, value);
    }

    /// <summary>
    /// ReactiveProperty<T> 객체를 JSON으로 씁니다.
    /// </summary>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        // 리플렉션을 사용하여 ReactiveProperty<T> 내부의 private _value 필드 값을 가져옵니다.
        // Why: 캡슐화를 유지하면서 직렬화에 필요한 최소한의 데이터에만 접근하기 위함입니다.
        var valueProperty = value.GetType().GetField("_value", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        object innerValue = valueProperty?.GetValue(value);

        // 내부 값(_value)만 JSON으로 직렬화합니다.
        serializer.Serialize(writer, innerValue);
    }
}