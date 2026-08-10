using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace DevFlowMonitor
{
   public partial class MainPage : ContentPage
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
         TreeScroll.IsVisible = false;

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
            //var (exitCode, stdout, stderr) = await RunDevFlowAsync(
            //   "ui tree --depth 3 --fields id,type,text,automationId",
            //   _selectedAgent);

            var (exitCode, stdout, stderr) = await RunDevFlowAsync(
               "ui tree",
               _selectedAgent);

            if (exitCode == 0 && stdout.Length > 0)
            {
               TreeEditor.Text = stdout;
               TreeScroll.IsVisible = true;
               ActionStatusLabel.Text = "Visual tree loaded";
            }
            else
            {
               TreeScroll.IsVisible = false;
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
         TreeScroll.IsVisible = false;
      }
   }

   public class DevFlowAgent
   {
      [JsonPropertyName("id")]
      public string Id { get; set; } = string.Empty;

      [JsonPropertyName("project")]
      public string Project { get; set; } = string.Empty;

      [JsonPropertyName("tfm")]
      public string Tfm { get; set; } = string.Empty;

      [JsonPropertyName("platform")]
      public string Platform { get; set; } = string.Empty;

      [JsonPropertyName("appName")]
      public string AppName { get; set; } = string.Empty;

      [JsonPropertyName("port")]
      public int Port { get; set; }

      [JsonPropertyName("version")]
      public string Version { get; set; } = string.Empty;

      [JsonPropertyName("sessionId")]
      public string SessionId { get; set; } = string.Empty;

      [JsonPropertyName("connectedAt")]
      public DateTimeOffset ConnectedAt { get; set; }
   }

   [JsonSerializable(typeof(List<DevFlowAgent>))]
   internal partial class DevFlowJsonContext : JsonSerializerContext { }
}


