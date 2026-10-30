using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: DisableRuntimeMarshalling]

namespace RaylibSharp;

public static unsafe partial class Raylib
{
  private const string LibraryName = "libraylib.6.2.0.dylib";

#pragma warning disable CA2255
  [ModuleInitializer]
#pragma warning restore CA2255
  internal static void Initialize()
  {
    NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), ResolveLibrary);
  }

  private static IntPtr ResolveLibrary(
    string libraryName,
    Assembly assembly,
    DllImportSearchPath? searchPath)
  {
    var path = FindInPath(libraryName);
    if (!string.IsNullOrEmpty(path))
    {
      return NativeLibrary.Load(path);
    }

    return IntPtr.Zero;
  }

  private static string? FindInPath(string libraryName)
  {
    var pathEnv = Environment.GetEnvironmentVariable("PATH");
    var paths = pathEnv?.Split(Path.PathSeparator);

    foreach (var path in paths ?? Array.Empty<string>())
    {
      string fullPath = Path.Combine(path, libraryName);
      if (File.Exists(fullPath))
        return fullPath;
    }

    return null;
  }
}