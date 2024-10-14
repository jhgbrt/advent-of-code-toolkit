using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Net.Code.AdventOfCode.Toolkit.UnitTests")]
[assembly: InternalsVisibleTo("Net.Code.AdventOfCode.Toolkit.IntegrationTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Net.Code.AdventOfCode.Toolkit;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Net.Code.AdventOfCode.Toolkit.Commands;
using Net.Code.AdventOfCode.Toolkit.Core;
using Net.Code.AdventOfCode.Toolkit.Data;
using Net.Code.AdventOfCode.Toolkit.Infrastructure;
using Net.Code.AdventOfCode.Toolkit.Logic;
using Net.Code.AdventOfCode.Toolkit.Web;

using Newtonsoft.Json.Linq;

using NodaTime;

using Spectre.Console;
using Spectre.Console.Cli;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;

public static class AoC
{
    public static Task<int> RunAsync(string[] args)
        => RunAsync(null, null, null, null, null, null, args);

    internal static async Task<int> RunAsync(
        IAssemblyResolver? resolver,
        IInputOutputService? io,
        IClock? clock,
        IAoCDbContext? aocDbContext,
        HttpMessageHandler? handler,
        IFileSystem? filesystem,
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

        var debug = args.Contains("--debug");

        var services = await InitializeServicesAsync(
            resolver,
            io,
            clock,
            aocDbContext,
            handler,
            filesystem,
            string.IsNullOrEmpty(loglevel) ? LogLevel.Warning : Enum.Parse<LogLevel>(loglevel, true),
            debug
            );

        var registrar = new TypeRegistrar(services);

        var app = new CommandApp(registrar);

        app.Configure(config =>
        {
            CommandRegistrar.AddCommands(config);
            config.PropagateExceptions();
            if (args.Contains("--debug"))
            {
                config.ValidateExamples();
            };
            config.SetExceptionHandler(
                (exception, resolver) =>
                {
                    if (exception is AoCException)
                        AnsiConsole.MarkupInterpolated($"[red]{exception.Message}[/]");
                    else
                        AnsiConsole.WriteException(exception, ExceptionFormats.ShortenEverything);
                    return 99;
                }
                );
        });
        
        app.SetDefaultCommand<Run>();

        return await app.RunAsync(args);
    }

    class AoCCommandInterceptor(IAoCDbContext db, ILogger<AoCCommandInterceptor> logger) : ICommandInterceptor
    {
        public void Intercept(CommandContext context, CommandSettings settings)
        {
            var sb = new StringBuilder("command: {command}, options: ");
            var info = from p in settings.GetType().GetProperties()
                       select (p.Name, Value: p.GetValue(settings));
            sb.Append(string.Join(", ", info.Select(item => $"{item.Name}: {{{item.Name}}}")));

            var values = new[] { context.Name }.Concat(info.Select(p => p.Value)).ToArray();

            logger.LogTrace(sb.ToString(), values);
            if (!Directory.Exists(".cache")) Directory.CreateDirectory(".cache");
            db.Migrate();
        }
        public void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
        {
            db.SaveChangesAsync().Wait();
        }

    }

    static Task<ServiceCollection> InitializeServicesAsync(
        IAssemblyResolver? resolver,
        IInputOutputService? io,
        IClock? clock,
        IAoCDbContext? aocDbContext,
        HttpMessageHandler? handler,
        IFileSystem? filesystem,
        LogLevel level,
        bool debug)
    {
        resolver = resolver ?? AssemblyResolver.Instance;
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
            .AddConsole()
            .SetMinimumLevel(level));
        services.AddSingleton(configuration);
        services.AddTransient<IAoCClient, AoCClient>();
        services.AddTransient<IFileSystemFactory, FileSystemFactory>();
        services.AddTransient<IPuzzleManager, PuzzleManager>();
        services.AddTransient<IAoCRunner, AoCRunner>();
        services.AddTransient<ICodeManager, CodeManager>();
        services.AddTransient<ILeaderboardManager, LeaderboardManager>();
        if (filesystem is not null)
        {
            services.AddSingleton(filesystem);
        }
        else
        {
            services.AddTransient<IFileSystem, FileSystem>();
        }
        services.AddSingleton(resolver);

        services.AddTransient<AoCLogic>();
        services.AddSingleton(clock ?? SystemClock.Instance);
        services.AddSingleton(io ?? new InputOutputService());

        if (aocDbContext is not null)
        {
            services.AddSingleton(aocDbContext);
        }
        else
        {

            services.AddDbContext<IAoCDbContext, AoCDbContext>(
                options =>
                {
                    if (debug)
                    {
                        options.EnableDetailedErrors();
                        options.EnableSensitiveDataLogging();
                    }
                    options.UseSqlite(
                        new SqliteConnectionStringBuilder() { DataSource = @".cache\aoc.db" }.ToString()
                        );

                }, contextLifetime: ServiceLifetime.Scoped
                );
        }

            services.AddHttpClient<IAoCClient, AoCClient>(client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                if (string.IsNullOrEmpty(configuration.SessionCookie))
                {
                    throw new NotAuthenticatedException("This command requires logging in, but the AOC_SESSION cookie is not set. " +
                        "Log in to adventofcode.com, and find the 'session' cookie value in your browser devtools. " +
                        "Copy this value and set it as an environment variable in your shell, or as a dotnet user-secret for your project.");
                }
            }).ConfigurePrimaryHttpMessageHandler(s =>
            {
                if (handler is not null)
                {
                    return handler;
                }
                else
                {
                    var sessionCookie = configuration.SessionCookie;
                    var cookieContainer = new CookieContainer();
                    cookieContainer.Add(new Uri(baseAddress), new Cookie("session", sessionCookie));
                    var handler = new HttpClientHandler { CookieContainer = cookieContainer };
                    return handler;
                }
            });

        CommandRegistrar.RegisterCommands(services);

        var typesToRegister = 
            from type in Assembly.GetExecutingAssembly().GetTypes()
            where !type.IsAbstract && (
                type.IsAssignableTo(typeof(ICommand)) || type.IsAssignableTo(typeof(CommandSettings))
            )
            select type;

        foreach (var type in typesToRegister)
        {
            services.AddTransient(type);
        }
        services.AddTransient<ICommandInterceptor, AoCCommandInterceptor>();

        return Task.FromResult(services);
    }

}

sealed class TypeRegistrar(IServiceCollection builder) : ITypeRegistrar
{
    public IServiceProvider? ServiceProvider => serviceProvider;

    private IServiceProvider? serviceProvider;
    private IServiceScope? Scope { get; set; }

    public ITypeResolver Build()
    {
        serviceProvider = builder.BuildServiceProvider();
        Scope = serviceProvider.CreateScope();
        return new TypeResolver(serviceProvider);
    }

    public void Register(Type service, Type implementation) => builder.AddTransient(service, implementation);

    public void RegisterInstance(Type service, object implementation) => builder.AddTransient(service, _ => implementation);

    public void RegisterLazy(Type service, Func<object> factory) => builder.AddTransient(service, _ => factory());
}

sealed class TypeResolver(IServiceProvider _provider) : ITypeResolver
{
    public object Resolve(Type? type)
        => _provider.GetRequiredService(type ?? throw new ArgumentNullException(nameof(type)));
}