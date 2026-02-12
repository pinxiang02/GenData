using System.Collections.Concurrent;
using DataGen_v1.Data;
using DataGen_v1.Models;
using Microsoft.EntityFrameworkCore;

namespace DataGen_v1.Services
{
    public class GenerationManager
    {
        // EVENT 1: Updates the "JSON Preview" box
        public event Action<int, string>? OnGenerated;

        // EVENT 2: Updates the "API Response" box (NEW)
        public event Action<int, string>? OnApiResponse;

        private readonly ConcurrentDictionary<int, CancellationTokenSource> _runningTasks = new();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly SystemLogService _logger;

        public GenerationManager(IServiceScopeFactory scopeFactory, SystemLogService logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public bool IsRunning(int genId) => _runningTasks.ContainsKey(genId);

        public void StartGeneration(int genId)
        {
            if (_runningTasks.ContainsKey(genId)) return;
            var cts = new CancellationTokenSource();
            _runningTasks.TryAdd(genId, cts);
            Task.Run(() => ExecuteLoopAsync(genId, cts.Token));
        }

        public void StopGeneration(int genId)
        {
            if (_runningTasks.TryRemove(genId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }

        private async Task ExecuteLoopAsync(int genId, CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            var genService = scope.ServiceProvider.GetRequiredService<GeneratorService>();
            var httpSender = scope.ServiceProvider.GetRequiredService<HttpSenderService>();
            var dbInserter = scope.ServiceProvider.GetRequiredService<DbInsertService>();

            using var dbContext = dbFactory.CreateDbContext();

            // 1. Load Configuration
            var generator = await dbContext.Generators
                .Include(g => g.ApiConfig)
                .Include(g => g.DbConnectionConfig)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == genId, token);

            if (generator == null) return;

            var nodes = await dbContext.NodeConfigs
                .Where(n => n.GeneratorId == genId)
                .AsNoTracking()
                .ToListAsync(token);

            _logger.AddLog(generator.Name, "System", "Started", $"Interval: {generator.IntervalMs}ms");

            // 2. The Loop
            while (!token.IsCancellationRequested)
            {
                var startTime = DateTime.UtcNow;

                try
                {
                    // A. Generate Data
                    var json = genService.GeneratePayload(nodes);

                    // Broadcast Request
                    OnGenerated?.Invoke(genId, json);

                    bool actionTaken = false;

                    // B. Send to API
                    if (generator.IsApiEnabled)
                    {
                        actionTaken = true;
                        if (generator.ApiConfig != null)
                        {
                            // FIX IS HERE: We now unpack the Tuple (Success, Status, Body)
                            var (success, status, body) = await httpSender.SendPayloadAsync(generator.ApiConfig, json);

                            // Broadcast Response Body to UI
                            OnApiResponse?.Invoke(genId, body);

                            if (success)
                            {
                                _logger.AddLog(generator.Name, "API", "Success", $"{status} OK");
                            }
                            else
                            {
                                _logger.AddLog(generator.Name, "API", "Error", $"{status}: {body}");
                            }
                        }
                        else
                        {
                            _logger.AddLog(generator.Name, "API", "Warning", "Enabled but no API Profile selected!");
                        }
                    }

                    // C. Insert to DB
                    if (generator.IsDbEnabled)
                    {
                        actionTaken = true;
                        if (generator.DbConnectionConfig != null && !string.IsNullOrEmpty(generator.TableName))
                        {
                            var (success, msg) = await dbInserter.InsertPayloadAsync(generator.DbConnectionConfig.ConnectionString, generator.TableName, json);
                            if (success)
                                _logger.AddLog(generator.Name, "DB", "Success", $"Inserted into {generator.TableName}");
                            else
                                _logger.AddLog(generator.Name, "DB", "Error", msg);
                        }
                        else
                        {
                            _logger.AddLog(generator.Name, "DB", "Warning", "Enabled but no Connection/Table selected!");
                        }
                    }

                    // D. Dry Run
                    if (!actionTaken)
                    {
                        string preview = json.Length > 100 ? json.Substring(0, 100) + "..." : json;
                        _logger.AddLog(generator.Name, "Generator", "Dry Run", $"Generated: {preview}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.AddLog(generator.Name, "System", "Crash", ex.Message);
                }

                // E. Wait
                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                var delay = generator.IntervalMs - (int)elapsed;
                if (delay > 0)
                {
                    try { await Task.Delay(delay, token); } catch { break; }
                }
            }
        }
    }
}