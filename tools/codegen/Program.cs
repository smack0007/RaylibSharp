using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RaylibBindingGenerator;

public static class Program
{
  static string LIBRARY_NAMESPACE = "RaylibSharp";

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
  }

  static class Generator
  {
    private static readonly HashSet<string> CSharpKeywords = new() {
      "abstract", "as", "base", "bool", "break", "byte",
      "case", "catch", "char", "checked", "class", "const",
      "continue", "decimal", "default", "delegate", "do",
      "double", "else", "enum", "event", "explicit",
      "extern", "false", "finally", "fixed", "float",
      "for", "foreach", "goto", "if", "implicit", "in",
      "int", "interface", "internal", "is", "lock", "long",
      "namespace", "new", "null", "object", "operator",
      "out", "override", "params", "private", "protected",
      "public", "readonly", "ref", "return", "sbyte",
      "sealed", "short", "sizeof", "stackalloc", "static",
      "string", "struct", "switch", "this", "throw", "true",
      "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
      "ushort", "using", "virtual", "void", "volatile",
      "while", "add", "alias", "ascending", "async",
      "await", "by", "descending", "dynamic", "equals",
      "from", "get", "global", "group", "init", "into",
      "join", "let", "nameof", "notnull", "on", "or",
      "orderby", "partial", "remove", "select", "set",
      "unmanaged", "value", "var", "when", "where", "with",
      "yield"
  };

    private static RaylibApi _api = null!;

    private static Dictionary<string, string> _aliases = new();

    public static void Generate(
      RaylibApi api,
      string outputPath,
      string namespaceName)
    {
      _api = api;

      _aliases = api.Aliases
          .ToDictionary(
              x => x.Name,
              x => x.Type,
              StringComparer.Ordinal);

      var sb = new StringBuilder();

      EmitHeader(sb, namespaceName);

      EmitEnums(sb);
      EmitStructs(sb);
      EmitDefines(sb);
      // EmitDelegates(sb);
      EmitNativeFunctions(sb);

      sb.AppendLine("}");
      sb.AppendLine();

      File.WriteAllText(outputPath, sb.ToString());
    }

    private static void EmitHeader(
      StringBuilder sb,
      string namespaceName)
    {
      sb.AppendLine("// <auto-generated />");
      sb.AppendLine("// Do not edit this file manually.");
      sb.AppendLine();

      sb.AppendLine("using System;");
      sb.AppendLine("using System.Runtime.InteropServices;");
      sb.AppendLine();

      sb.AppendLine($"namespace {namespaceName};");
      sb.AppendLine();

      sb.AppendLine("public static partial class Raylib");
      sb.AppendLine("{");
    }

    private static void EmitEnums(StringBuilder sb)
    {
      foreach (RaylibEnum e in _api.Enums)
      {
        sb.AppendLine(
            $"    /// <summary>{EscapeXml(e.Description)}</summary>");

        sb.AppendLine(
            $"    public enum {SafeIdentifier(e.Name)} : int");

        sb.AppendLine("    {");

        foreach (RaylibEnumValue value in e.Values)
        {
          if (!string.IsNullOrWhiteSpace(value.Description))
          {
            sb.AppendLine(
                $"        /// <summary>{EscapeXml(value.Description)}</summary>");
          }

          sb.AppendLine(
              $"        {SafeIdentifier(value.Name)} = {value.Value},");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
      }
    }

    private static void EmitStructs(StringBuilder sb)
    {
      var t = "    ";

      var structNames = new string[] {
      "Color",
    };

      foreach (RaylibStruct structure in _api.Structs.Where(x => structNames.Contains(x.Name)))
      {
        sb.AppendLine($"{t}/// <summary>{EscapeXml(structure.Description)}</summary>");

        sb.AppendLine($"{t}[StructLayout(LayoutKind.Sequential)]");

        sb.AppendLine($"{t}public partial struct {SafeIdentifier(structure.Name)}");

        sb.AppendLine($"{t}{{");

        foreach (RaylibField field in structure.Fields)
        {
          EmitField(sb, field);
        }

        sb.AppendLine($"{t}}}");
        sb.AppendLine();
      }
    }

    private static void EmitDefines(StringBuilder sb)
    {
      var t = "    ";

      foreach (RaylibDefine define in _api.Defines)
      {
        switch (define.Type)
        {
          case "COLOR":
            var colorParts = new string[] { "r", "g", "b", "a" };
            var value = "new Color() { " + string.Join(", ", define.Value.ToString()
              .Replace("CLITERAL(Color){", "")
              .Replace("}", "")
              .Trim()
              .Split(",")
              .Select((x, i) => $"{colorParts[i]} = {x}")
            ) + " }";

            sb.AppendLine($"{t}/// <summary>{EscapeXml(define.Description)}</summary>");
            sb.AppendLine($"{t}public static Color {define.Name} = {value};");
            sb.AppendLine();
            break;
        }
      }
    }

    private static void EmitField(
        StringBuilder sb,
        RaylibField field)
    {
      var t = "        ";

      string fieldName = SafeIdentifier(field.Name);

      string type = ResolveType(field.Type);

      string comment =
          !string.IsNullOrWhiteSpace(field.Description)
              ? $" // {field.Description.Replace("\r", "").Replace("\n", " ")}"
              : "";

      // Inline array: e.g. float[4], int[4], Matrix[2]
      if (TryParseArrayType(
          type,
          out string elementType,
          out int arraySize))
      {
        EmitInlineArrayField(
            sb,
            fieldName,
            elementType,
            arraySize,
            comment);

        return;
      }

      // bool must explicitly marshal as C bool.
      if (type == "bool")
      {
        sb.AppendLine($"{t}[MarshalAs(UnmanagedType.I1)]");
        sb.AppendLine($"{t}public bool {fieldName};{comment}");
        return;
      }

      sb.AppendLine($"{t}public {type} {fieldName};{comment}");
    }

    private static void EmitInlineArrayField(
        StringBuilder sb,
        string fieldName,
        string elementType,
        int size,
        string comment)
    {
      var t = "        ";

      if (IsFixedBufferType(elementType))
      {
        sb.AppendLine($"{t}public fixed {elementType} {fieldName}[{size}];{comment}");
        return;
      }

      // Struct arrays cannot be represented by a C# fixed buffer.
      // ByValArray gives the desired native inline-array layout.
      string? marshalType = GetMarshalArraySubType(elementType);

      sb.AppendLine($"{t}[MarshalAs(UnmanagedType.ByValArray, SizeConst = {size}" +
          (marshalType != null
              ? $", ArraySubType = UnmanagedType.{marshalType}"
              : "") +
          ")]");

      sb.AppendLine($"{t}public {elementType}[] {fieldName};{comment}");
    }

    private static void EmitDelegates(StringBuilder sb)
    {
      foreach (RaylibFunction callback in _api.Callbacks)
      {
        string returnType =
            ResolveType(callback.ReturnType);

        sb.AppendLine(
            $"    /// <summary>{EscapeXml(callback.Description)}</summary>");

        sb.AppendLine(
            "    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]");

        sb.AppendLine(
            $"    public delegate {returnType} " +
            $"{SafeIdentifier(callback.Name)}(");

        EmitParameterList(
            sb,
            callback.Parameters,
            indent: "        ");

        sb.AppendLine("    );");
        sb.AppendLine();
      }
    }

    private static void EmitNativeFunctions(StringBuilder sb)
    {
      var t = "    ";

      var functionNames = new string[] {
      "BeginDrawing",
      "ClearBackground",
      "CloseWindow",
      "DrawText",
      "EndDrawing",
      "InitWindow",
      "SetTargetFPS",
      "WindowShouldClose",
    };

      foreach (RaylibFunction function in _api.Functions.Where(x => functionNames.Contains(x.Name)))
      {
        string returnType =
            ResolveType(function.ReturnType);

        sb.AppendLine($"{t}/// <summary>{EscapeXml(function.Description)}</summary>");

        if (returnType == "bool")
        {
          sb.AppendLine($"{t}[return: MarshalAs(UnmanagedType.I1)]");
        }

        sb.AppendLine($"{t}[DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]");

        sb.AppendLine($"{t}public static extern {returnType} {SafeIdentifier(function.Name)}(");

        EmitParameterList(
            sb,
            function.Parameters,
            indent: "        ");

        sb.AppendLine("    );");
        sb.AppendLine();
      }
    }

    private static void EmitParameterList(
        StringBuilder sb,
        List<RaylibParameter> parameters,
        string indent)
    {
      for (int i = 0; i < parameters.Count; i++)
      {
        RaylibParameter parameter = parameters[i];

        string type = ResolveType(parameter.Type);

        string name = SafeIdentifier(parameter.Name);

        string marshal = "";

        if (type == "bool")
        {
          marshal =
              "[MarshalAs(UnmanagedType.I1)] ";
        }

        sb.Append(indent);

        sb.Append(marshal);
        sb.Append(type);
        sb.Append(' ');
        sb.Append(name);

        if (i < parameters.Count - 1)
          sb.Append(',');

        sb.AppendLine();
      }
    }

    private static string ResolveType(string type)
    {
      type = NormalizeType(type);

      // Resolve typedef aliases while preserving pointers.
      //
      // Example:
      // ModelAnimPose * -> Transform **
      //
      // Actually ModelAnimPose itself is Transform *,
      // therefore ModelAnimPose * becomes Transform **.

      int pointerCount = 0;

      while (type.EndsWith('*'))
      {
        pointerCount++;
        type = type[..^1].TrimEnd();
      }

      // Resolve aliases recursively.
      HashSet<string> visited = [];

      while (_aliases.TryGetValue(type, out string? alias))
      {
        if (!visited.Add(type))
          break;

        type = NormalizeType(alias);

        while (type.EndsWith('*'))
        {
          pointerCount++;

          type = type[..^1].TrimEnd();
        }
      }

      string baseType = MapBaseType(type);

      if (pointerCount == 0)
        return baseType;

      // void* is best represented as void*.
      if (baseType == "void")
        return "void" + new string('*', pointerCount);

      if (baseType == "const_char")
      {
        if (pointerCount == 1)
        {
          return "[MarshalAs(UnmanagedType.LPStr)] string";
        }
      }

      return baseType + new string('*', pointerCount);
    }

    private static string MapBaseType(string type)
    {
      return type switch
      {
        "void" => "void",

        "bool" => "bool",

        "char" => "sbyte",
        "signed char" => "sbyte",
        "unsigned char" => "byte",

        "short" => "short",
        "unsigned short" => "ushort",

        "int" => "int",
        "unsigned int" => "uint",

        "long" => "nint",
        "unsigned long" => "nuint",

        "long long" => "long",
        "unsigned long long" => "ulong",

        "float" => "float",
        "double" => "double",

        // C's size_t.
        "size_t" => "nuint",

        // C's va_list is platform-specific.
        // We intentionally expose it as an opaque pointer.
        "va_list" => "nint",

        _ => SafeIdentifier(type)
      };
    }

    private static string NormalizeType(string type)
    {
      type = type.Trim();

      type = type
          .Replace("  ", " ")
          .Replace(" *", "*")
          .Replace("* ", "*");

      return type;
    }

    private static bool TryParseArrayType(
        string type,
        out string elementType,
        out int size)
    {
      elementType = "";
      size = 0;

      int open = type.LastIndexOf('[');

      if (open < 0 || !type.EndsWith(']'))
        return false;

      string sizeText =
          type[(open + 1)..^1];

      if (!int.TryParse(sizeText, out size))
        return false;

      elementType =
          type[..open].Trim();

      return true;
    }

    private static bool IsFixedBufferType(string type)
    {
      return type is
          "bool" or
          "byte" or
          "sbyte" or
          "short" or
          "ushort" or
          "int" or
          "uint" or
          "long" or
          "ulong" or
          "float" or
          "double" or
          "char";
    }

    private static string? GetMarshalArraySubType(
        string type)
    {
      return type switch
      {
        "bool" => "I1",
        "byte" => "U1",
        "sbyte" => "I1",
        "short" => "I2",
        "ushort" => "U2",
        "int" => "I4",
        "uint" => "U4",
        "long" => "I8",
        "ulong" => "U8",
        "float" => "R4",
        "double" => "R8",
        _ => null
      };
    }

    private static string SafeIdentifier(string name)
    {
      if (string.IsNullOrWhiteSpace(name))
        return "_";

      // Handle names that aren't valid C# identifiers.
      var sb = new StringBuilder();

      for (int i = 0; i < name.Length; i++)
      {
        char c = name[i];

        if (i == 0)
        {
          if (char.IsLetter(c) || c == '_')
            sb.Append(c);
          else
            sb.Append('_');
        }
        else
        {
          if (char.IsLetterOrDigit(c) || c == '_')
            sb.Append(c);
          else
            sb.Append('_');
        }
      }

      string result = sb.ToString();

      if (CSharpKeywords.Contains(result))
        result = "@" + result;

      return result;
    }

    private static string EscapeXml(string? value)
    {
      if (string.IsNullOrEmpty(value))
        return "";

      return value
          .Replace("&", "&amp;")
          .Replace("<", "&lt;")
          .Replace(">", "&gt;");
    }
  }

  public static int Main(string[] args)
  {
    string input = "raylib_api.json";
    string output = "../../src/Raylib.Generated.cs";

    if (!File.Exists(input))
    {
      Console.Error.WriteLine($"Input file does not exist: {input}");
      return 1;
    }

    try
    {
      string json = File.ReadAllText(input);

      var options = new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      };

      RaylibApi api = JsonSerializer.Deserialize<RaylibApi>(
        json,
        options
      ) ?? throw new InvalidOperationException("Could not deserialize raylib_api.json.");

      Generator.Generate(api, output, LIBRARY_NAMESPACE);

      return 0;
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine(ex);
      return 1;
    }
  }
}