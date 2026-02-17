using System.Collections.Concurrent;
using DeviceManagementAPI.Data;
using DeviceManagementAPI.Models;

namespace DeviceManagementAPI.Services
{
    public class AuditLogBackgroundService : BackgroundService
    {
        private readonly ConcurrentQueue<AuditLog> _queue;
        private readonly IServiceProvider _serviceProvider;

        public AuditLogBackgroundService(ConcurrentQueue<AuditLog> queue, IServiceProvider serviceProvider)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessLogs();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                await Task.Delay(10000, stoppingToken);
            }

            if (!_queue.IsEmpty)
            {
                await ProcessLogs();
            }
        }

        private async Task ProcessLogs()
        {
            if (_queue.IsEmpty) return;

            var logs = new List<AuditLog>();

            while (_queue.TryDequeue(out var log))
            {
                logs.Add(log);
            }

            if (logs.Count > 0)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DeviceDbContext>();
                
                await context.AuditLogs.InsertManyAsync(logs);
                Console.WriteLine($"Logged {logs.Count} items to MongoDB.");
            }
        }
    }
}