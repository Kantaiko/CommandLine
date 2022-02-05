using System;
using System.Collections.Generic;
using System.CommandLine.IO;
using Xunit;

namespace Kantaiko.CommandLine.Tests;

internal static class TestUtils
{
    public static (CommandLineProgram, TestConsole) CreateTestProgram(string[] args,
        IEnumerable<Type> lookupTypes)
    {
        var console = new TestConsole();

        var program = new CommandLineProgram(args)
        {
            ExecutableName = "test",
            Console = console,
            LookupTypes = lookupTypes
        };

        return (program, console);
    }

    public static (CommandLineProgram, TestConsole) CreateTestProgram<TTest>(string[] args)
    {
        return CreateTestProgram(args, typeof(TTest).GetNestedTypes());
    }

    public static (CommandLineProgram, TestConsole) CreateTestProgram<TTest>(string args)
    {
        return CreateTestProgram<TTest>(args.Split(" "));
    }

    public record RunResult(int ReturnCode, string? Output, string? Error);

    public static RunResult RunTestProgram<TTest>(string args)
    {
        var (program, console) = CreateTestProgram<TTest>(args);

        var returnCode = program.Run();

        return new RunResult(returnCode, console.Out.ToString(), console.Error.ToString());
    }

    public static void AssertRunResult(RunResult runResult, int? expectedCode = 0, string? expectedOutput = null,
        string? expectedError = null)
    {
        if (expectedCode.HasValue)
        {
            Assert.Equal(expectedCode, runResult.ReturnCode);
        }

        if (expectedOutput is not null)
        {
            Assert.Equal(expectedOutput, runResult.Output);
        }

        if (expectedError is not null)
        {
            Assert.Equal(expectedError, runResult.Error);
        }
    }
}
