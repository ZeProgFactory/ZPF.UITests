using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevFlowMonitor
{
   public partial class MainPage : ContentPage, INotifyPropertyChanged
   {
      private const int BrokerPort = 19223;
      private const int PollIntervalMs = 5_000;

      // Android emulator reaches the host via 10.0.2.2; all other platforms use localhost.
      private static string BrokerHost =>
#if ANDROID
         "10.0.2.2";
#else
         "localhost";
#endif

      private static string BrokerAgentsUrl => $"http://{BrokerHost}:{BrokerPort}/api/agents";

      private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(4) };

      private CancellationTokenSource? _cts;
      private DevFlowAgent? _selectedAgent;
      private string? _lastScreenshotPath;

      public ObservableCollection<DevFlowAgent> Agents { get; } = [];
      public ObservableCollection<TreeNodeItem> TreeItems { get; } = [];

      private TreeNodeItem? _selectedTreeNode;
      public TreeNodeItem? SelectedTreeNode
      {
         get => _selectedTreeNode;
         set
         {
            _selectedTreeNode = value;
            OnPropertyChanged();
            NodeDetailFrame.IsVisible = value is not null;
         }
      }

      public event PropertyChangedEventHandler? PropertyChanged;
      private void OnPropertyChanged([CallerMemberName] string? name = null)
         => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

      public MainPage()
      {
         InitializeComponent();
         BindingContext = this;
      }

      // ── Monitor start/stop ───────────────────────────────────────────────

      private void OnMonitorClicked(object? sender, EventArgs e)
      {
         if (_cts is null)
            StartMonitoring();
         else
            StopMonitoring();
      }

      private void StartMonitoring()
      {
         _cts = new CancellationTokenSource();
         MonitorBtn.Text = "Stop";
         StatusLabel.Text = "Connecting to broker…";
         _ = MonitorDevFlowApp(_cts.Token);
      }

      private void StopMonitoring()
      {
         _cts?.Cancel();
         _cts = null;
         MonitorBtn.Text = "Start";
         StatusLabel.Text = "Monitoring stopped.";
      }

      /// <summary>
      /// Polls the DevFlow broker every 5 seconds and refreshes the agent list.
      /// </summary>
      private async Task MonitorDevFlowApp(CancellationToken ct)
      {
         while (!ct.IsCancellationRequested)
         {
            try
            {
               var agents = await _http.GetFromJsonAsync<List<DevFlowAgent>>(
                  BrokerAgentsUrl,
                  DevFlowJsonContext.Default.ListDevFlowAgent,
                  ct);

               MainThread.BeginInvokeOnMainThread(() =>
               {
                  Agents.Clear();
                  if (agents is { Count: > 0 })
                  {
                     foreach (var agent in agents)
                        Agents.Add(agent);
                     StatusLabel.Text = $"Last refresh: {DateTime.Now:HH:mm:ss}  —  {agents.Count} agent(s)";
                  }
                  else
                  {
                     StatusLabel.Text = $"Last refresh: {DateTime.Now:HH:mm:ss}  —  No agents connected";
                  }
               });
            }
            catch (OperationCanceledException)
            {
               break;
            }
            catch (Exception ex)
            {
               MainThread.BeginInvokeOnMainThread(() =>
                  StatusLabel.Text = $"Broker error: {ex.Message}");
            }

            try { await Task.Delay(PollIntervalMs, ct); }
            catch (OperationCanceledException) { break; }
         }

         _cts = null;
         MainThread.BeginInvokeOnMainThread(() =>
         {
            MonitorBtn.Text = "Start";
            StatusLabel.Text = "Monitoring stopped.";
         });
      }

      // ── Agent selection ──────────────────────────────────────────────────

      private void OnAgentSelectionChanged(object? sender, SelectionChangedEventArgs e)
      {
         _selectedAgent = e.CurrentSelection.FirstOrDefault() as DevFlowAgent;
         bool hasAgent = _selectedAgent is not null;

#if ANDROID
         // The maui devflow CLI is not available on Android, so Screenshot/Tree cannot be run.
         ScreenshotBtn.IsEnabled = false;
         TreeBtn.IsEnabled = false;
         TapCounterBtn.IsEnabled = false;
         ActionStatusLabel.Text = hasAgent
            ? $"Agent: {_selectedAgent!.AppName} ({_selectedAgent.Platform}) — Screenshot/Tree/Tap require Windows"
            : "Select an agent below";
#else
         ScreenshotBtn.IsEnabled = hasAgent;
         TreeBtn.IsEnabled = hasAgent;
         TapCounterBtn.IsEnabled = hasAgent;
         ActionStatusLabel.Text = hasAgent
            ? $"Agent: {_selectedAgent!.AppName} ({_selectedAgent.Platform})"
            : "Select an agent below";
#endif

         HideResults();
      }

      // ── ADB port forward ──────────────────────────────────────────────────

      /// <summary>
      /// Runs <c>adb -s {serial} reverse tcp:19223 tcp:19223</c> for every connected
      /// Android emulator so the DevFlow agent inside the emulator can reach the broker
      /// on the host at localhost:19223.
      /// </summary>
      private async void OnAdbForwardClicked(object? sender, EventArgs e)
      {
         AdbForwardBtn.IsEnabled = false;
         ActionStatusLabel.Text = "Setting up ADB port forward…";
         try
         {
            var (devices, err) = await RunAdbAsync("devices");
            if (err.Length > 0 && devices.Length == 0)
            {
               ActionStatusLabel.Text = $"adb error: {err.Trim()}";
               return;
            }

            // Parse "emulator-XXXX\tdevice" lines
            var serials = devices
               .Split('\n', StringSplitOptions.RemoveEmptyEntries)
               .Skip(1)  // skip "List of devices attached" header
               .Select(l => l.Split('\t'))
               .Where(p => p.Length >= 2 && p[1].Trim() == "device")
               .Select(p => p[0].Trim())
               .Where(s => s.StartsWith("emulator-"))
               .ToList();

            if (serials.Count == 0)
            {
               ActionStatusLabel.Text = "No Android emulators found. Start an emulator first.";
               return;
            }

            var results = new List<string>();
            foreach (var serial in serials)
            {
               var (stdout, stderr) = await RunAdbAsync($"-s {serial} reverse tcp:{BrokerPort} tcp:{BrokerPort}");
               var line = stdout.Trim().Length > 0 ? stdout.Trim() : stderr.Trim();
               results.Add($"{serial}: {(line.Length > 0 ? line : "ok")}");
            }
            ActionStatusLabel.Text = "ADB forward: " + string.Join(" | ", results);
         }
         catch (Exception ex)
         {
            ActionStatusLabel.Text = $"ADB error: {ex.Message}";
         }
         finally
         {
            AdbForwardBtn.IsEnabled = true;
         }
      }

      private static async Task<(string Stdout, string Stderr)> RunAdbAsync(string arguments)
      {
         // Prefer ANDROID_HOME env var, fall back to common Windows install path.
         var sdkRoot = Environment.GetEnvironmentVariable("ANDROID_HOME")
            ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT")
            ?? @"C:\Program Files (x86)\Android\android-sdk";
         var adbPath = Path.Combine(sdkRoot, "platform-tools", "adb.exe");
         if (!File.Exists(adbPath))
            adbPath = "adb";  // fall back to PATH

         var psi = new ProcessStartInfo(adbPath, arguments)
         {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
         };
         using var proc = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start adb.");
         var stdout = await proc.StandardOutput.ReadToEndAsync();
         var stderr = await proc.StandardError.ReadToEndAsync();
         await proc.WaitForExitAsync();
         return (stdout, stderr);
      }

      // ── Screenshot ───────────────────────────────────────────────────────

      private async void OnScreenshotClicked(object? sender, EventArgs e)
      {
#if ANDROID
         ActionStatusLabel.Text = "Screenshot requires running DevFlowMonitor on Windows.";
         return;
#else
         if (_selectedAgent is null) return;

         SetBusy(true, "Taking screenshot…");
         TreeColumn.IsVisible = false;

         try
         {
            var tmpPath = Path.ChangeExtension(Path.GetTempFileName(), ".png");
            _lastScreenshotPath = tmpPath;

            var (exitCode, _, stderr) = await RunDevFlowAsync(
               $"ui screenshot --output \"{tmpPath}\" --overwrite",
               _selectedAgent);

            if (exitCode == 0 && File.Exists(tmpPath))
            {
               ScreenshotImage.Source = ImageSource.FromFile(tmpPath);
               ScreenshotImage.IsVisible = true;
               ActionStatusLabel.Text = $"Screenshot captured — {new FileInfo(tmpPath).Length / 1024} KB";
            }
            else
            {
               ScreenshotImage.IsVisible = false;
               ActionStatusLabel.Text = $"Screenshot failed: {stderr.Trim()}";
            }
         }
         catch (Exception ex)
         {
            ActionStatusLabel.Text = $"Error: {ex.Message}";
         }
         finally
         {
            SetBusy(false);
         }
#endif
      }

      // ── Visual tree ──────────────────────────────────────────────────────

      private async void OnTreeClicked(object? sender, EventArgs e)
      {
#if ANDROID
         ActionStatusLabel.Text = "Visual tree requires running DevFlowMonitor on Windows.";
         return;
#else
         if (_selectedAgent is null) return;

         SetBusy(true, "Fetching visual tree…");
         ScreenshotImage.IsVisible = false;

         try
         {
            var (exitCode, stdout, stderr) = await RunDevFlowAsync(
               "ui tree --depth 0",
               _selectedAgent);

            if (exitCode == 0 && stdout.Length > 0)
            {
               var roots = JsonSerializer.Deserialize(
                  stdout, DevFlowJsonContext.Default.ListTreeNode);

               TreeItems.Clear();
               SelectedTreeNode = null;
               if (roots is { Count: > 0 })
               {
                  foreach (var root in roots)
                     FlattenTree(root, 0, expandDepth: 1);
               }
               TreeColumn.IsVisible = true;
               ActionStatusLabel.Text = $"Tree loaded — {TreeItems.Count} nodes";
            }
            else
            {
               TreeColumn.IsVisible = false;
               ActionStatusLabel.Text = $"Tree failed: {stderr.Trim()}";
            }
         }
         catch (Exception ex)
         {
            ActionStatusLabel.Text = $"Error: {ex.Message}";
         }
         finally
         {
            SetBusy(false);
         }
#endif
      }

      // ── Helpers ──────────────────────────────────────────────────────────

#if !ANDROID
      /// <summary>
      /// Runs a <c>maui devflow</c> sub-command targeting the given agent's port
      /// and returns (exitCode, stdout, stderr).
      /// Only available on platforms where the <c>maui</c> CLI is installed (Windows).
      /// </summary>
      private static async Task<(int ExitCode, string Stdout, string Stderr)> RunDevFlowAsync(
         string arguments, DevFlowAgent agent)
      {
         var psi = new ProcessStartInfo("maui", $"devflow --agent-port {agent.Port} {arguments}")
         {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
         };

         using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start maui devflow process.");

         var stdoutTask = process.StandardOutput.ReadToEndAsync();
         var stderrTask = process.StandardError.ReadToEndAsync();
         await process.WaitForExitAsync();

         return (process.ExitCode, await stdoutTask, await stderrTask);
      }
#endif

      // ── Tap CounterBtn ────────────────────────────────────────────────────

      private async void OnTapCounterBtnClicked(object? sender, EventArgs e)
      {
#if ANDROID
         ActionStatusLabel.Text = "Tap requires running DevFlowMonitor on Windows.";
         return;
#else
         if (_selectedAgent is null) return;

         SetBusy(true, "Tapping CounterBtn…");
         try
         {
            var (exitCode, _, stderr) = await RunDevFlowAsync(
               "ui tap --automationId \"CounterBtn\"",
               _selectedAgent);

            ActionStatusLabel.Text = exitCode == 0
               ? "CounterBtn tapped ✓"
               : $"Tap failed: {stderr.Trim()}";
         }
         catch (Exception ex)
         {
            ActionStatusLabel.Text = $"Error: {ex.Message}";
         }
         finally
         {
            SetBusy(false);
         }
#endif
      }

      // ── Tree expand / collapse / selection ──────────────────────────────────

      private void OnTreeNodeSelected(object? sender, SelectionChangedEventArgs e)
         => SelectedTreeNode = e.CurrentSelection.FirstOrDefault() as TreeNodeItem;

      private void OnExpandToggled(object? sender, TappedEventArgs e)
      {
         if (e.Parameter is not TreeNodeItem item || !item.HasChildren) return;
         if (item.IsExpanded) CollapseNode(item);
         else ExpandNode(item);
      }

      /// <summary>Inserts the immediate children of <paramref name="item"/> into the flat list.</summary>
      private void ExpandNode(TreeNodeItem item)
      {
         item.IsExpanded = true;
         var idx = TreeItems.IndexOf(item);
         if (idx < 0 || item.Node.Children is null) return;
         int insertAt = idx + 1;
         foreach (var child in item.Node.Children)
         {
            var childItem = new TreeNodeItem { Node = child, Depth = item.Depth + 1 };
            TreeItems.Insert(insertAt++, childItem);
         }
      }

      /// <summary>Removes the entire subtree below <paramref name="item"/> from the flat list.</summary>
      private void CollapseNode(TreeNodeItem item)
      {
         item.IsExpanded = false;
         var idx = TreeItems.IndexOf(item);
         if (idx < 0) return;
         while (idx + 1 < TreeItems.Count && TreeItems[idx + 1].Depth > item.Depth)
            TreeItems.RemoveAt(idx + 1);
      }

      /// <summary>
      /// Adds <paramref name="node"/> and its children to <see cref="TreeItems"/>.
      /// Children are expanded automatically up to <paramref name="expandDepth"/> levels.
      /// </summary>
      private void FlattenTree(TreeNode node, int depth, int expandDepth)
      {
         var item = new TreeNodeItem { Node = node, Depth = depth };
         if (depth < expandDepth && node.Children?.Count > 0)
            item.IsExpanded = true;
         TreeItems.Add(item);
         if (item.IsExpanded && node.Children is not null)
            foreach (var child in node.Children)
               FlattenTree(child, depth + 1, expandDepth);
      }

      private void SetBusy(bool busy, string? message = null)
      {
         Busy.IsRunning = busy;
         Busy.IsVisible = busy;
         ScreenshotBtn.IsEnabled = !busy && _selectedAgent is not null;
         TreeBtn.IsEnabled = !busy && _selectedAgent is not null;
         TapCounterBtn.IsEnabled = !busy && _selectedAgent is not null;
         if (message is not null)
            ActionStatusLabel.Text = message;
      }

      private void HideResults()
      {
         ScreenshotImage.IsVisible = false;
         TreeColumn.IsVisible = false;
         TreeItems.Clear();
         SelectedTreeNode = null;
      }
   }
}


