
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Net.Code.AdventOfCode.Toolkit.Commands;

public interface IInputOutputService
{
    T Prompt<T>(IPrompt<T> prompt);
    void WriteLine(string message);
    void MarkupLine(string markup);
    void Write(IRenderable renderable);
}

public class InputOutputService : IInputOutputService
{
    public T Prompt<T>(IPrompt<T> prompt)
    {
        return AnsiConsole.Prompt(prompt);
    }

    public void Write(IRenderable renderable)
    {
        AnsiConsole.Write(renderable);
    }

    public void WriteLine(string message)
    {
        AnsiConsole.WriteLine(message);
    }

    public void MarkupLine(string markup)
    {
        AnsiConsole.MarkupLine(markup);
    }
}
