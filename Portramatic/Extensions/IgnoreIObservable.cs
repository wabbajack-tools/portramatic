using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Portramatic.Extensions;

public class IgnoreIObservable<T> : JsonConverter<IObservable<T>>
{
    public override IObservable<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
    public override void Write(Utf8JsonWriter writer, IObservable<T> value, JsonSerializerOptions options)
    {
        return;
    }
}