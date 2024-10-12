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

using NodaTime;

using Spectre.Console;
using Spectre.Console.Cli;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

public static class AoC
{
    public static Task<int> RunAsync(string[] args)
        => RunAsync(null, null, null, null, null, null, args);

    internal static async Task<int> RunAsync(
        IAssemblyResolver? resolver,
        IInputOutputService? io,
        IClock? clock,
        IAoCDbContext? aocDbContext,
        IHttpClientWrapper? httpclient,
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
            httpclient,
            filesystem,
            string.IsNullOrEmpty(loglevel) ? LogLevel.Warning : Enum.Parse<LogLevel>(loglevel, true),
            debug
            );

        var registrar = new TypeRegistrar(services);

        var app = new CommandApp(registrar);

        app.Configure(config =>
        {
            AddCommand<Run>(config);
            AddCommand<Verify>(config);
            AddCommand<Init>(config);
            AddCommand<Sync>(config);
            AddCommand<Post>(config);
            AddCommand<Export>(config);
            AddCommand<Report>(config);
            AddCommand<Leaderboard>(config);
            AddCommand<Stats>(config);
            AddCommand<Test>(config);
            config.PropagateExceptions();
            if (args.Contains("--debug"))
            {
                config.ValidateExamples();
            };
            //config.SetExceptionHandler();
        });
        
        app.SetDefaultCommand<Run>();

        try
        {

            var returnValue = await app.RunAsync(args);



            return returnValue;
        }
        catch (UnauthorizedAccessException e)
        {
            AnsiConsole.MarkupInterpolated($"[red]{e.Message}[/]");
        }
        catch(AoCException e) 
        {
            AnsiConsole.WriteLine(e.Message);
            if (debug) throw;
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
            if (debug) throw;
        }
        return 99;
    }

    class MyInterceptor(IAoCDbContext db) : ICommandInterceptor
    {
        public void Intercept(CommandContext context, CommandSettings settings)
        {
            if (!Directory.Exists(".cache")) Directory.CreateDirectory(".cache");
            db.Migrate();
        }
        public void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
        {
            db.SaveChangesAsync().Wait();
        }

    }

    class TraceInterceptor : ICommandInterceptor
    {
        public void Intercept(CommandContext context, CommandSettings settings)
        {
            Trace.TraceInformation($"command name: {context.Name}");
            var info = from p in settings.GetType().GetProperties()
                       select $"{{ {p.Name}: {p.GetValue(settings)} }}";
            Trace.TraceInformation($"options: {{ {string.Join(",", info)} }}");
        }
    }

    static Task<ServiceCollection> InitializeServicesAsync(
        IAssemblyResolver? resolver,
        IInputOutputService? io,
        IClock? clock,
        IAoCDbContext? aocDbContext,
        IHttpClientWrapper? httpclient,
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
        services.AddSingleton<IAssemblyResolver>(resolver);
        if (httpclient is not null)
        {
            services.AddSingleton(httpclient);
        }
        else
        {
            services.AddTransient<IHttpClientWrapper, HttpClientWrapper>();
        }
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
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(ICommand))))
        {
            services.AddTransient(type);
        }
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(CommandSettings))))
        {
            services.AddTransient(type);
        }
        services.AddTransient<ICommandInterceptor, MyInterceptor>();
        services.AddTransient<ICommandInterceptor, TraceInterceptor>();

        if (level == LogLevel.Trace)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }
        return Task.FromResult(services);
    }

    static ICommandConfigurator AddCommand<T>(IConfigurator config) where T : class, ICommand
        => config.AddCommand<T>(typeof(T).Name.ToLower()).WithDescription(GetDescription(typeof(T)) ?? typeof(T).Name);

    static string? GetDescription(ICustomAttributeProvider provider)
        => provider.GetCustomAttributes(typeof(DescriptionAttribute), false).OfType<DescriptionAttribute>().SingleOrDefault()?.Description;
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