using Spectre.Console;
using Spectre.Console.Rendering;

namespace Net.Code.AdventOfCode.Toolkit.Infrastructure;

public interface IInputOutputService
{
    T Prompt<T>(IPrompt<T> prompt);
    void WriteLine(string message);
    void MarkupLine(string markup);
    void Write(IRenderable renderable);
}

public class InputOutputService : IInputOutputService
{
    public T Prompt<T>(IPrompt<T> prompt) => AnsiConsole.Prompt(prompt);

    public void Write(IRenderable renderable) => AnsiConsole.Write(renderable);

    public void WriteLine(string message) => AnsiConsole.WriteLine(message);

    public void MarkupLine(string markup) => AnsiConsole.MarkupLine(markup);
}

class DelegatingIOService : IInputOutputService
{
    Action<string> Log;

    public DelegatingIOService(Action<string> log) => Log = log;

    public void MarkupLine(string markup) => Log(markup);

    public T Prompt<T>(IPrompt<T> prompt) => default!;

    public void Write(IRenderable renderable) => Log(renderable?.ToString() ?? string.Empty);

    public void WriteLine(string message) => Log(message);
}
