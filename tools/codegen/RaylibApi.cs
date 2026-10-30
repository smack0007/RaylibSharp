using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RaylibBindingGenerator;

sealed class RaylibApi
{
  [JsonPropertyName("defines")]
  public List<RaylibDefine> Defines { get; set; } = [];

  [JsonPropertyName("structs")]
  public List<RaylibStruct> Structs { get; set; } = [];

  [JsonPropertyName("aliases")]
  public List<RaylibAlias> Aliases { get; set; } = [];

  [JsonPropertyName("enums")]
  public List<RaylibEnum> Enums { get; set; } = [];

  [JsonPropertyName("callbacks")]
  public List<RaylibFunction> Callbacks { get; set; } = [];

  [JsonPropertyName("functions")]
  public List<RaylibFunction> Functions { get; set; } = [];
}

sealed class RaylibDefine
{
  [JsonPropertyName("name")]
  public string Name { get; set; } = "";

  [JsonPropertyName("type")]
  public string Type { get; set; } = "";

  [JsonPropertyName("value")]
  public JsonElement Value { get; set; }

  [JsonPropertyName("description")]
  public string Description { get; set; } = "";
}

sealed class RaylibStruct
{
  [JsonPropertyName("name")]
  public string Name { get; set; } = "";

  [JsonPropertyName("description")]
  public string Description { get; set; } = "";

  [JsonPropertyName("fields")]
  public List<RaylibField> Fields { get; set; } = [];
}

sealed class RaylibField
{
  [JsonPropertyName("type")]
  public string Type { get; set; } = "";

  [JsonPropertyName("name")]
  public string Name { get; set; } = "";

  [JsonPropertyName("description")]
  public string Description { get; set; } = "";
}

sealed class RaylibAlias
{
  [JsonPropertyName("type")]
  public string Type { get; set; } = "";

  [JsonPropertyName("name")]
  public string Name { get; set; } = "";

  [JsonPropertyName("description")]
  public string Description { get; set; } = "";
}

sealed class RaylibEnum
{
  [JsonPropertyName("name")]
  public string Name { get; set; } = "";

  [JsonPropertyName("description")]
  public string Description { get; set; } = "";

  [JsonPropertyName("values")]
  public List<RaylibEnumValue> Values { get; set; } = [];
}

sealed class RaylibEnumValue
{
  [JsonPropertyName("name")]
  public string Name { get; set; } = "";

  [JsonPropertyName("value")]
  public int Value { get; set; }

  [JsonPropertyName("description")]
  public string Description { get; set; } = "";
}

sealed class RaylibFunction
{
  [JsonPropertyName("name")]
  public string Name { get; set; } = "";

  [JsonPropertyName("description")]
  public string Description { get; set; } = "";

  [JsonPropertyName("returnType")]
  public string ReturnType { get; set; } = "";

  [JsonPropertyName("params")]
  public List<RaylibParameter> Parameters { get; set; } = [];
}

sealed class RaylibParameter
{
  [JsonPropertyName("type")]
  public string Type { get; set; } = "";

  [JsonPropertyName("name")]
  public string Name { get; set; } = "";

  public bool IsNormalized { get; set; }
}