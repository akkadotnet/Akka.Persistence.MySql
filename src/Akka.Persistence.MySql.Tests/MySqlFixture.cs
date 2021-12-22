using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Akka.Util;
using Docker.DotNet;
using Docker.DotNet.Models;
using Xunit;
using Xunit.Sdk;

namespace Akka.Persistence.MySql.Tests
{
    [CollectionDefinition("MySqlSpec")]
    public sealed class MySqlSpecsFixture : ICollectionFixture<MySqlFixture>
    {
    }

    /// <summary>
    ///     Fixture used to run SQL Server
    /// </summary>
    public class MySqlFixture : IAsyncLifetime
    {
        public const string DatabaseName = "akka_persistence_tests";
        
        protected readonly string SqlContainerName = $"mysql-{Guid.NewGuid():N}";
        protected DockerClient Client;

        public MySqlFixture()
        {
            DockerClientConfiguration config;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                config = new DockerClientConfiguration(new Uri("unix://var/run/docker.sock"));
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                config = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"));
            else
                throw new NotSupportedException($"Unsupported OS [{RuntimeInformation.OSDescription}]");

            Client = config.CreateClient();
        }

        protected string ImageName => "mysql";
        protected string Tag => "8";

        protected string SqlServerImageName => $"{ImageName}:{Tag}";

        public string ConnectionString { get; private set; }

        public async Task InitializeAsync()
        {
            var sysInfo = await Client.System.GetSystemInfoAsync();
            if (sysInfo.OSType.ToLowerInvariant() != "linux")
                throw new TestClassException("MySQL docker image only available for linux containers");
            
            var images = await Client.Images.ListImagesAsync(new ImagesListParameters
            {
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    {
                        "reference",
                        new Dictionary<string, bool>
                        {
                            {SqlServerImageName, true}
                        }
                    }
                }
            }); 

            if (images.Count == 0)
                await Client.Images.CreateImageAsync(
                    new ImagesCreateParameters {FromImage = ImageName, Tag = Tag}, null,
                    new Progress<JSONMessage>(message =>
                    {
                        Console.WriteLine(!string.IsNullOrEmpty(message.ErrorMessage)
                            ? message.ErrorMessage
                            : $"{message.ID} {message.Status} {message.ProgressMessage}");
                    }));

            var mySqlHostPort = ThreadLocalRandom.Current.Next(9000, 10000);

            // create the container
            await Client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = SqlServerImageName,
                Name = SqlContainerName,
                Tty = true,
                ExposedPorts = new Dictionary<string, EmptyStruct>
                {
                    {"3306/tcp", new EmptyStruct()}
                },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {
                            "3306/tcp",
                            new List<PortBinding>
                            {
                                new PortBinding
                                {
                                    HostPort = $"{mySqlHostPort}"
                                }
                            }
                        }
                    }
                },
                Env = new[]
                {
                    "MYSQL_ROOT_PASSWORD=Password12!", 
                    $"MYSQL_DATABASE={DatabaseName}",
                }
            });

            // start the container
            await Client.Containers.StartContainerAsync(SqlContainerName, new ContainerStartParameters());

            // Wait until MySQL is completely ready
            var logStream = await Client.Containers.GetContainerLogsAsync(SqlContainerName, new ContainerLogsParameters
            {
                Follow = true,
                ShowStdout = true,
                ShowStderr = true
            });

            string line = null;
            var timeoutInMilis = 60000;
            using (var reader = new StreamReader(logStream))
            {
                var stopwatch = Stopwatch.StartNew();
                while (stopwatch.ElapsedMilliseconds < timeoutInMilis && (line = await reader.ReadLineAsync()) != null)
                {
                    if(!string.IsNullOrWhiteSpace(line))
                        Console.WriteLine(line);
                    if (line.Contains("ready for connections."))
                    {
                        break;
                    }
                }
                stopwatch.Stop();
            }
#if NETFRAMEWORK
            logStream.Dispose();
#else
            await logStream.DisposeAsync();
#endif
            if (!line?.Contains("ready for connections.") ?? false)
                throw new Exception("MySQL docker image failed to run.");

            var connectionString = new DbConnectionStringBuilder
            {
                ["Server"] = $"localhost",
                ["Port"] = mySqlHostPort.ToString(),
                ["Database"] = "akka_persistence_tests",
                ["User Id"] = "root",
                ["Password"] = "Password12!"
            };

            ConnectionString = connectionString.ToString();
            Console.WriteLine($"Connection string: [{ConnectionString}]");

            await Task.Delay(10000);
        }

        public async Task DisposeAsync()
        {
            if (Client != null)
            {
                try
                {
                    await Client.Containers.StopContainerAsync(SqlContainerName, new ContainerStopParameters());
                    await Client.Containers.RemoveContainerAsync(SqlContainerName,
                        new ContainerRemoveParameters {Force = true});
                }
                catch (DockerContainerNotFoundException)
                {
                    // no-op
                }
                finally
                {
                    Client.Dispose();
                }
            }
        }
    }}