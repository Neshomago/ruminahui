// Minimal offline runner for Assets/Tests/EditMode (reflection over the NUnit attribute stubs).
// Only meaningful for pure-logic tests — there is no real engine underneath.
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

public static class Program
{
    public static int Main()
    {
        int pass = 0, fail = 0;
        var types = typeof(Program).Assembly.GetTypes().Where(t => t.Namespace == "Ruminahui.Tests").OrderBy(t => t.Name);
        foreach (var type in types)
        {
            var methods = type.GetMethods();
            var setUp = methods.FirstOrDefault(m => m.GetCustomAttribute<SetUpAttribute>() != null);
            var tearDown = methods.FirstOrDefault(m => m.GetCustomAttribute<TearDownAttribute>() != null);
            foreach (var m in methods)
            {
                var cases = m.GetCustomAttributes<TestCaseAttribute>().Select(c => c.Arguments).ToList();
                if (m.GetCustomAttribute<TestAttribute>() != null) cases.Add(new object[0]);
                foreach (var args in cases)
                {
                    var inst = Activator.CreateInstance(type);
                    string label = $"{type.Name}.{m.Name}({string.Join(", ", args)})";
                    try
                    {
                        setUp?.Invoke(inst, null);
                        var ps = m.GetParameters();
                        var converted = args.Select((a, i) => a is IConvertible && ps[i].ParameterType.IsPrimitive ? Convert.ChangeType(a, ps[i].ParameterType) : a).ToArray();
                        m.Invoke(inst, converted);
                        tearDown?.Invoke(inst, null);
                        pass++;
                        Console.WriteLine("  PASS " + label);
                    }
                    catch (TargetInvocationException e)
                    {
                        fail++;
                        Console.WriteLine("  FAIL " + label + " — " + e.InnerException?.Message);
                    }
                }
            }
        }
        Console.WriteLine($"\n{pass} passed, {fail} failed");
        return fail == 0 ? 0 : 1;
    }
}
