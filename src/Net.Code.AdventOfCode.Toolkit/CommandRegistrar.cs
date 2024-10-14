
namespace Net.Code.AdventOfCode.Toolkit;
using Microsoft.Extensions.DependencyInjection;
using Net.Code.AdventOfCode.Toolkit.Commands;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Reflection;

/*
*/

static class CommandRegistrar
{

	public static void RegisterCommands(IServiceCollection services)
	{
		services.AddTransient<Export>();
		services.AddTransient<Init>();
		services.AddTransient<Leaderboard>();
		services.AddTransient<Post>();
		services.AddTransient<Report>();
		services.AddTransient<Run>();
		services.AddTransient<Stats>();
		services.AddTransient<Sync>();
		services.AddTransient<Test>();
		services.AddTransient<Verify>();
	}

	public static void AddCommands(IConfigurator config)
	{
		AddCommand<Export>(config);
		AddCommand<Init>(config);
		AddCommand<Leaderboard>(config);
		AddCommand<Post>(config);
		AddCommand<Report>(config);
		AddCommand<Run>(config);
		AddCommand<Stats>(config);
		AddCommand<Sync>(config);
		AddCommand<Test>(config);
		AddCommand<Verify>(config);
	}

    static ICommandConfigurator AddCommand<T>(IConfigurator config) where T : class, ICommand
        => config.AddCommand<T>(typeof(T).Name.ToLower()).WithDescription(GetDescription(typeof(T)) ?? typeof(T).Name);
    static string? GetDescription(ICustomAttributeProvider provider)
        => provider.GetCustomAttributes(typeof(DescriptionAttribute), false).OfType<DescriptionAttribute>().SingleOrDefault()?.Description;
}