using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Net.Code.AdventOfCode.Toolkit.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Net.Code.AdventOfCode.Toolkit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

using Net.Code.AdventOfCode.Toolkit.Commands;
using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Logic;

using NodaTime;

using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

using System.ComponentModel;
using System.Reflection;

public static class AoC
{
    public static Task<int> RunAsync(string[] args)
        => RunAsync(
            AssemblyResolver.Instance,
            new InputOutputService(),
            SystemClock.Instance,
            args);

    public static async Task<IEngine> GetEngine(
        Assembly assembly,
        Action<string> log
        )
    {
        var services = await InitializeServicesAsync(new FixedAssemblyResolver(assembly), new DelegatingIOService(log), SystemClock.Instance, LogLevel.Trace, false);
        var engine = services.BuildServiceProvider().GetService<IEngine>() ?? throw new Exception("could not resolve Engine");
        return engine;
    }

    public static async Task<int> RunAsync(
        IAssemblyResolver resolver,
        IInputOutputService io,
        IClock clock,
        string[] args
        )
    {
        string? loglevel = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("--loglevel="))
            {
                loglevel = args[i].Split('=')[1];
                break;
            }
            else if (args[i] == "--loglevel" && i < args.Length - 1)
            {
                loglevel = args[i + 1];
                break;
            }
        }
        var services = await InitializeServicesAsync(resolver, io, clock, string.IsNullOrEmpty(loglevel) ? LogLevel.Warning : Enum.Parse<LogLevel>(loglevel, true), args.Contains("--debug"));

        var registrar = new TypeRegistrar(services);

        var app = new CommandApp(registrar);

        app.Configure(config =>
        {
            AddCommand<Run>(config);
            AddCommand<Verify>(config);
            AddCommand<Init>(config);
            AddCommand<Sync>(config);
            AddCommand<Show>(config);
            AddCommand<Post>(config);
            AddCommand<Export>(config);
            AddCommand<Report>(config);
            AddCommand<Leaderboard>(config);
            AddCommand<Stats>(config);
            if (args.Contains("--debug"))
            {
                config.PropagateExceptions();
                config.ValidateExamples();
            }
        });

        return await app.RunAsync(args);
    }

    static Task<ServiceCollection> InitializeServicesAsync(
        IAssemblyResolver resolver,
        IInputOutputService io,
        IClock clock,
        LogLevel level,
        bool debug)
    {
        var assembly = resolver.GetEntryAssembly() ?? throw new NullReferenceException("GetEntryAssembly returned null"); 

        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(assembly)
            .Build();

        var cookieValue = config["AOC_SESSION"] ?? throw new NullReferenceException("The AOC_SESSION variable is not set. Go to https://adventofcode.com, log in and copy the value of the session cookie. You need to set this value as a user secret or environment variable called AOC_SESSION.");
        
        const string baseAddress = "https://adventofcode.com";
        var configuration = new Configuration(baseAddress, cookieValue);
        var services = new ServiceCollection();
        services.AddLogging(builder => builder
            .AddInlineSpectreConsole(c => c.LogLevel = LogLevel.Trace).SetMinimumLevel(level));
        services.AddSingleton(configuration);
        services.AddTransient<IAoCClient, AoCClient>();
        services.AddTransient<IPuzzleManager, PuzzleManager>();
        services.AddTransient<IAoCRunner, AoCRunner>();
        services.AddTransient<ICodeManager, CodeManager>();
        services.AddTransient<IReportManager, ReportManager>();
        services.AddTransient<ICache, Cache>();
        services.AddTransient<IFileSystem, FileSystem>();
        services.AddSingleton<IAssemblyResolver>(resolver);
        services.AddTransient<IHttpClientWrapper, HttpClientWrapper>();
        services.AddTransient<AoCLogic>();
        services.AddTransient<IEngine, Engine>();
        services.AddSingleton(clock);
        services.AddSingleton(io);
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(ICommand))))
        {
            services.AddTransient(type);
        }
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(CommandSettings))))
        {
            services.AddTransient(type);
        }
        return Task.FromResult(services);
    }

    static ICommandConfigurator AddCommand<T>(IConfigurator config) where T : class, ICommand
        => config.AddCommand<T>(typeof(T).Name.ToLower()).WithDescription(GetDescription(typeof(T)) ?? typeof(T).Name);

    static string? GetDescription(ICustomAttributeProvider provider)
        => provider.GetCustomAttributes(typeof(DescriptionAttribute), false).OfType<DescriptionAttribute>().SingleOrDefault()?.Description;
}

sealed class TypeRegistrar : ITypeRegistrar
{
    public IServiceProvider ServiceProvider => serviceProvider ?? throw new NullReferenceException("service provider not available");
    private readonly IServiceCollection _builder;
    private IServiceProvider? serviceProvider;
    public TypeRegistrar(IServiceCollection builder)
    {
        _builder = builder;
    }

    public ITypeResolver Build()
    {
        serviceProvider = _builder.BuildServiceProvider();
        return new TypeResolver(serviceProvider);
    }

    public void Register(Type service, Type implementation) => _builder.AddTransient(service, implementation);

    public void RegisterInstance(Type service, object implementation) => _builder.AddTransient(service, _ => implementation);

    public void RegisterLazy(Type service, Func<object> factory) => _builder.AddTransient(service, _ => factory());
}

sealed class TypeResolver : ITypeResolver
{
    private readonly IServiceProvider _provider;

    public TypeResolver(IServiceProvider provider)
        => _provider = provider ?? throw new ArgumentNullException(nameof(provider));

    public object Resolve(Type? type)
        => _provider.GetRequiredService(type ?? throw new ArgumentNullException(nameof(type)));
}