using System.Reflection;
using Xunit;

namespace CareLink.Tests;

public static class InProcessTestRunner
{
    public static async Task<int> Main(string[] args)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var testTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .OrderBy(t => t.FullName)
            .ToList();

        int passed = 0;
        int failed = 0;
        var failures = new List<string>();

        Console.WriteLine("=================================================================");
        Console.WriteLine("CareLink Automated Test Suite (In-Process xUnit Runner)");
        Console.WriteLine("=================================================================");

        foreach (var type in testTypes)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<FactAttribute>() != null ||
                            m.GetCustomAttribute<TheoryAttribute>() != null)
                .ToList();

            if (methods.Count == 0)
                continue;

            foreach (var method in methods)
            {
                var inlineDataAttrs = method.GetCustomAttributes<InlineDataAttribute>().ToList();
                var paramCount = method.GetParameters().Length;
                var invocations = inlineDataAttrs.Count > 0
                    ? inlineDataAttrs.Select(a =>
                    {
                        var row = a.GetData(method).FirstOrDefault();
                        return row ?? new object?[paramCount];
                    }).ToList()
                    : [Array.Empty<object?>()];

                foreach (var parameters in invocations)
                {
                    var label = parameters != null && parameters.Length > 0
                        ? $"{type.Name}.{method.Name}({string.Join(", ", parameters.Select(p => p?.ToString() ?? "null"))})"
                        : $"{type.Name}.{method.Name}";

                    object? instance = null;
                    try
                    {
                        instance = Activator.CreateInstance(type);
                        var result = method.Invoke(instance, parameters);
                        if (result is Task task)
                        {
                            await task;
                        }

                        passed++;
                        Console.WriteLine($"  [PASS] {label}");
                    }
                    catch (Exception ex)
                    {
                        var actual = ex is TargetInvocationException tie && tie.InnerException != null
                            ? tie.InnerException
                            : ex;
                        failed++;
                        var failureMsg = $"  [FAIL] {label} -> {actual.GetType().Name}: {actual.Message}";
                        failures.Add(failureMsg);
                        Console.WriteLine(failureMsg);
                    }
                    finally
                    {
                        if (instance is IAsyncDisposable asyncDisposable)
                        {
                            await asyncDisposable.DisposeAsync();
                        }
                        else if (instance is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                }
            }
        }

        Console.WriteLine("=================================================================");
        Console.WriteLine($"Total: {passed + failed} | Passed: {passed} | Failed: {failed}");
        Console.WriteLine("=================================================================");

        if (failures.Count > 0)
        {
            foreach (var f in failures)
            {
                Console.Error.WriteLine(f);
            }
            return 1;
        }

        return 0;
    }
}
