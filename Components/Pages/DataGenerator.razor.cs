using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataGen_v1.Data;
using DataGen_v1.Models;
using DataGen_v1.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DataGen_v1.Components.Pages
{
    public partial class DataGenerator : ComponentBase, IDisposable
    {
        [Parameter] public int GenId { get; set; }

        [Inject] public IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
        [Inject] public DbInsertService DbInserter { get; set; } = default!;
        [Inject] public GeneratorService GenService { get; set; } = default!;
        [Inject] public HttpSenderService HttpSender { get; set; } = default!;
        [Inject] public GenerationManager GenManager { get; set; } = default!;
        [Inject] public SystemLogService LogService { get; set; } = default!;
        [Inject] public NavigationManager Nav { get; set; } = default!;
        [Inject] public IToastService ToastService { get; set; } = default!;

        protected string CurrentGenName = "Loading...";

        //Config
        protected List<ApiConfig> AvailableApis = new();
        protected ApiConfig? ConnectedApi;
        protected ApiConfig? SelectedApiOption;
        protected List<DbConnectionConfig> AvailableDbs = new();
        protected DbConnectionConfig? SelectedDbOption;
        protected string SelectedTable = "";
        protected int IntervalMs = 1000;
        protected bool IsApiEnabled = false;
        protected bool IsDbEnabled = false;

        //Nodes
        protected List<NodeConfig> Nodes = new();
        protected NodeConfig NewNode { get; set; } = new();
        protected bool IsEditing = false;
        protected string TempListItem { get; set; } = "";
        protected List<string> CurrentList = new();

        // Data State
        protected string JsonOutput = "{}";
        protected string ApiResponseOutput = "";
        protected bool IsServiceRunning = false;
        protected List<LogEntry> ConsoleLogs = new();
        private Timer? _statusTimer;

        // Enums
        protected string SelectedType { get => NewNode.DataType.ToString(); set => NewNode.DataType = Enum.Parse<DataTypeEnum>(value); }
        protected string SelectedMode { get => NewNode.Mode.ToString(); set => NewNode.Mode = Enum.Parse<GenerationMode>(value); }
        protected string SelectedParentIdString { get => NewNode.ParentId?.ToString() ?? ""; set { NewNode.ParentId = string.IsNullOrEmpty(value) ? null : int.Parse(value); } }

        protected override async Task OnInitializedAsync()
        {
            await LoadApis(); 
            await LoadDbs(); 
            await LoadGeneratorInfo(); 
            await LoadNodes();
            IsServiceRunning = GenManager.IsRunning(GenId);

            GenManager.OnGenerated += OnLiveJsonReceived;
            GenManager.OnApiResponse += OnLiveApiReceived;
            LogService.OnLogAdded += OnLogReceived;

            _statusTimer = new Timer(_ =>
            {
                bool running = GenManager.IsRunning(GenId);
                if (running != IsServiceRunning)
                {
                    IsServiceRunning = running;
                    InvokeAsync(StateHasChanged);
                }
            }, null, 1000, 2000);
        }

        protected void OnLiveJsonReceived(int id, string json)
        {
            if (id == GenId) InvokeAsync(() => 
            { 
                JsonOutput = json; 
                StateHasChanged(); 
            });
        }

        protected void OnLiveApiReceived(int id, string response)
        {
            if (id == GenId) InvokeAsync(() =>
            {
                ApiResponseOutput += $"\n--- Response @ {DateTime.Now:HH:mm:ss} ---\n{response}\n";
                StateHasChanged();
            });
        }

        protected void OnLogReceived()
        {
            InvokeAsync(() =>
            {
                ConsoleLogs = LogService.GetLogs().Where(l => l.GeneratorName == CurrentGenName).Take(50).ToList();
                StateHasChanged();
            });
        }

        protected void ClearLogs()
        {
            ConsoleLogs.Clear();
            StateHasChanged();
        }

        protected void GeneratePreview()
        {
            JsonOutput = GenService.GeneratePayload(Nodes);
        }

        protected async Task ToggleService()
        {
            if (IsServiceRunning)
            {
                GenManager.StopGeneration(GenId);
            }
            else
            {
                await SaveConfiguration(false);
                GenManager.StartGeneration(GenId);
            }
            IsServiceRunning = !IsServiceRunning;
        }

        protected async Task LoadApis()
        {
            using var c = DbFactory.CreateDbContext();
            AvailableApis = await c.ApiConfigs.AsNoTracking().ToListAsync();
        }

        protected async Task LoadDbs()
        {
            using var c = DbFactory.CreateDbContext();
            AvailableDbs = await c.DbConfigs.AsNoTracking().ToListAsync();
        }

        protected async Task LoadNodes()
        {
            using var c = DbFactory.CreateDbContext();
            Nodes = await c.NodeConfigs.Where(x => x.GeneratorId == GenId).AsNoTracking().OrderBy(x => x.Id).ToListAsync();
        }

        protected async Task LoadGeneratorInfo()
        {
            using var context = DbFactory.CreateDbContext();
            var gen = await context.Generators.Include(g => g.ApiConfig).Include(g => g.DbConnectionConfig).FirstOrDefaultAsync(g => g.Id == GenId);
            if (gen != null)
            {
                CurrentGenName = gen.Name;
                IntervalMs = gen.IntervalMs == 0 ? 1000 : gen.IntervalMs;
                IsApiEnabled = gen.IsApiEnabled;
                IsDbEnabled = gen.IsDbEnabled;
                ConnectedApi = gen.ApiConfig;

                if (ConnectedApi != null)
                {
                    SelectedApiOption = AvailableApis.FirstOrDefault(a => a.Id == ConnectedApi.Id);
                }

                if (gen.DbConnectionConfig != null)
                {
                    SelectedDbOption = AvailableDbs.FirstOrDefault(d => d.Id == gen.DbConnectionConfig.Id);
                    SelectedTable = gen.TableName ?? "";
                }
            }
        }

        protected async Task SaveConfiguration(bool showMessage = true)
        {
            using var context = DbFactory.CreateDbContext();
            var gen = await context.Generators.FindAsync(GenId);
            if (gen != null)
            {
                gen.IntervalMs = IntervalMs;
                gen.IsApiEnabled = IsApiEnabled;
                gen.IsDbEnabled = IsDbEnabled;
                gen.ApiConfigId = SelectedApiOption?.Id;
                gen.DbConnectionConfigId = SelectedDbOption?.Id;
                gen.TableName = SelectedTable;
                await context.SaveChangesAsync();
                if (showMessage) ToastService.ShowSuccess("Saved.");
            }
        }

        protected void AddItemToList()
        {
            if (!string.IsNullOrWhiteSpace(TempListItem))
            {
                CurrentList.Add(TempListItem);
                TempListItem = "";
            }
        }

        protected void RemoveItem(string item)
        {
            CurrentList.Remove(item);
        }

        protected void CancelEdit()
        {
            IsEditing = false;
            NewNode = new NodeConfig();
            CurrentList.Clear();
        }

        protected async Task SaveNode()
        {
            NewNode.GeneratorId = GenId;
            if (NewNode.Mode == GenerationMode.RandomPick)
            {
                NewNode.ValueList = string.Join(",", CurrentList);
            }

            using var context = DbFactory.CreateDbContext();
            if (NewNode.Id == 0)
            {
                context.NodeConfigs.Add(NewNode);
            }
            else
            {
                context.NodeConfigs.Update(NewNode);
            }
            await context.SaveChangesAsync();

            NewNode = new NodeConfig();
            CurrentList.Clear();
            IsEditing = false;
            await LoadNodes();
        }

        protected void EditNode(NodeConfig node)
        {
            IsEditing = true;
            NewNode = new NodeConfig
            {
                Id = node.Id,
                NodeName = node.NodeName,
                DataType = node.DataType,
                Mode = node.Mode,
                FixedValue = node.FixedValue,
                MinRange = node.MinRange,
                MaxRange = node.MaxRange,
                ValueList = node.ValueList,
                ParentId = node.ParentId
            };
            CurrentList.Clear();
            
            if (NewNode.Mode == GenerationMode.RandomPick && !string.IsNullOrEmpty(NewNode.ValueList))
            {
                CurrentList.AddRange(NewNode.ValueList.Split(','));
            }
        }

        protected async Task DeleteNode(NodeConfig node)
        {
            using var c = DbFactory.CreateDbContext();
            c.NodeConfigs.Remove(node);
            await c.SaveChangesAsync();
            await LoadNodes();
        }

        public void Dispose()
        {
            _statusTimer?.Dispose();
            GenManager.OnGenerated -= OnLiveJsonReceived;
            GenManager.OnApiResponse -= OnLiveApiReceived;
            LogService.OnLogAdded -= OnLogReceived;
        }
    }
}
