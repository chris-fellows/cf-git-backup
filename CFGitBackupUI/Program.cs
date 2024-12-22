using CFGitBackup.Interfaces;
using CFGitBackup.Models;
using CFGitBackup.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace CFGitBackupUI
{
    internal static class Program
    {        
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var host = CreateHostBuilder().Build();
            ServiceProvider = host.Services;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(ServiceProvider.GetRequiredService<MainForm>());
        }

        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Create a host builder to build the service provider
        /// </summary>
        /// <returns></returns>
        static IHostBuilder CreateHostBuilder()
        {            
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => 
                {
                    // Register data services
                    var dataFolder = System.Configuration.ConfigurationManager.AppSettings.Get("DataFolder")
                                .Replace("{process-folder}", Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));

                    Directory.CreateDirectory(dataFolder);
                    services.AddTransient<IGitConfigService>((scope) =>
                    {
                        return new XmlGitConfigService(Path.Combine(dataFolder, "GitConfig"));                        
                    });
                    services.AddTransient<IGitRepoBackupConfigService>((scope) =>
                    {
                        return new XmlGitRepoBackupConfigService(Path.Combine(dataFolder, "GitRepoBackupConfig"));
                    });

                    // Register other services
                    services.AddTransient<IGitRepoBackupService, GitRepoBackupService>();
                    services.RegisterAllTypes<IGitRepoService>(new[] { typeof(GitConfig).Assembly });

                    // Register forms
                    services.AddTransient<MainForm>();
                });
        }

        /// <summary>
        /// Registers all types implementing interface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <param name="lifetime"></param>
        private static void RegisterAllTypes<T>(this IServiceCollection services, IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var typesFromAssemblies = assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T))));
            foreach (var type in typesFromAssemblies)
            {
                services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
            }
        }
    }
}