﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore.Tests.Pomelo.EntityFrameworkCore.MySql
{
    [TestClass]
    public class Mysql_v8_0_15
    {
        [TestMethod]
        public async Task Test()
        {
            var containerName = GetType().FullName;

            var containerPort = 20006;

            var container = new TestUtil.DockerContainer(
                containerName,
                "mysql:8.0.15",
                new[]
                {
                    $"-p {containerPort}:3306",
                    "-e MYSQL_ROOT_PASSWORD=my-secret-pw"
                }
            );

            using (container)
            {
                container.Start();

                await TestUtil.WaitForDockerLogOutput(
                    containerName,
                    "/usr/sbin/mysqld: ready for connections. Version: '8.0.15'  socket: '/var/run/mysqld/mysqld.sock'  port: 3306  MySQL Community Server - GPL.",
                    DateTime.UtcNow.AddMinutes(2)
                );

                await TestUtil.WaitForTcpSocket(
                    "localhost",
                    containerPort,
                    DateTime.UtcNow.AddMinutes(1)
                );

                var dbContextConnString =
                    $"Server=localhost;" +
                    $"Port={containerPort};" +
                    $"Database=orleans_ef_test;" +
                    $"Uid=root;" +
                    $"Pwd=my-secret-pw";

                void Callback(IServiceCollection services)
                {
                    services.AddEntityFrameworkMySql();
                    services.AddDbContext<OrleansEFContext>(options =>
                        options.UseMySql(dbContextConnString)
                    );
                }

                await TestSuite.Run(20000, Callback);
            }
        }
    }
}