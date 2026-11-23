using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.AspNetCore.SignalR.Client;

namespace View_WPF
{
    public partial class RemoteAccessWindow : Window
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private HubConnection? _hubConnection;
        private CancellationTokenSource? _hostStreamCts;
        private string? _activeHostRemoteCode;
        private string? _activeViewerRemoteCode;
        private bool _isHosting;
        private readonly int _currentUserId;

        private const string ApiBaseUrl = "https://localhost:7073";
        private const string HubUrl = "https://localhost:7073/hubs/remote-access";

        public RemoteAccessWindow(int currentUserId = 101)
        {
            InitializeComponent();
            _currentUserId = currentUserId;
            ShowActivated = true;
            WindowState = WindowState.Maximized;
            Closed += RemoteAccessWindow_Closed;
            Loaded += (_, _) =>
            {
                WindowState = WindowState.Maximized;
                Topmost = true;
                Activate();
                Focus();
                Topmost = false;
            };
        }

        private async void btnGenerateSession_Click(object sender, RoutedEventArgs e)
        {
            btnGenerateSession.IsEnabled = false;
            txtHostExpiryInfo.Text = "Generating session...";

            try
            {
                var payload = new { HostUserId = _currentUserId };
                var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/api/RemoteAccess/CreateSession", payload);

                if (!response.IsSuccessStatusCode)
                {
                    txtHostExpiryInfo.Text = "Failed to generate session.";
                    btnGenerateSession.IsEnabled = true;
                    return;
                }

                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                string remoteCode = json.GetProperty("remoteCode").GetString() ?? string.Empty;
                string accessPassword = json.GetProperty("accessPassword").GetString() ?? string.Empty;
                DateTime expiresAt = json.GetProperty("expiresAt").GetDateTime();

                txtMyRemoteId.Text = remoteCode;
                txtMyPassword.Text = accessPassword;
                txtHostExpiryInfo.Text = $"Active until (UTC): {expiresAt:yyyy-MM-dd HH:mm}";

                await EnsureHubConnectedAsync();
                if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("RegisterHost", remoteCode);
                }

                _activeHostRemoteCode = remoteCode;
                _isHosting = true;
                btnStopHosting.IsEnabled = true;
                txtConnectionStatus.Text = "Status: Hosting Ready";
            }
            catch (Exception ex)
            {
                txtHostExpiryInfo.Text = $"Error: {ex.Message}";
                btnGenerateSession.IsEnabled = true;
            }
        }

        private async void btnStopHosting_Click(object sender, RoutedEventArgs e)
        {
            await StopHostingAsync();
        }

        private async void btnConnectPartner_Click(object sender, RoutedEventArgs e)
        {
            string partnerId = txtPartnerRemoteId.Text.Trim();
            string partnerPassword = pwdPartnerPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(partnerId) || string.IsNullOrWhiteSpace(partnerPassword))
            {
                txtViewerStatusInfo.Text = "Please enter both Remote ID and Password.";
                return;
            }

            btnConnectPartner.IsEnabled = false;
            txtViewerStatusInfo.Text = "Validating session...";

            try
            {
                var payload = new
                {
                    RemoteCode = partnerId,
                    AccessPassword = partnerPassword,
                    ViewerUserId = _currentUserId
                };

                var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/api/RemoteAccess/ValidateSession", payload);

                if (!response.IsSuccessStatusCode)
                {
                    txtViewerStatusInfo.Text = "Invalid ID, password, or expired session.";
                    btnConnectPartner.IsEnabled = true;
                    return;
                }

                await EnsureHubConnectedAsync();
                if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("JoinAsViewer", partnerId);
                }

                _activeViewerRemoteCode = partnerId;
                txtConnectionStatus.Text = $"Status: Connected to {partnerId}";
                txtViewerStatusInfo.Text = "Connected. Waiting for screen frames...";

                var viewerWindow = new RemoteScreenViewerWindow(_hubConnection!, partnerId);
                viewerWindow.Show();
                btnConnectPartner.IsEnabled = true;
            }
            catch (Exception ex)
            {
                txtViewerStatusInfo.Text = $"Connection error: {ex.Message}";
                btnConnectPartner.IsEnabled = true;
            }
        }

        private async Task EnsureHubConnectedAsync()
        {
            if (_hubConnection != null)
            {
                if (_hubConnection.State == HubConnectionState.Disconnected)
                {
                    await _hubConnection.StartAsync();
                }
                return;
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(HubUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string>("ViewerConnected", async (viewerConnectionId) =>
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    txtConnectionStatus.Text = "Status: Viewer Connected";
                    txtHostExpiryInfo.Text = "Remote viewer is connected. Streaming screen...";
                });

                if (_isHosting && !string.IsNullOrWhiteSpace(_activeHostRemoteCode))
                {
                    StartScreenStreaming(_activeHostRemoteCode);
                }
            });

            _hubConnection.On<string, double, double, int>("ReceiveInputEvent", (eventType, xRatio, yRatio, keyCodeOrButton) =>
            {
                if (!_isHosting)
                {
                    return;
                }

                RemoteInputInjector.HandleRemoteInput(eventType, xRatio, yRatio, keyCodeOrButton);
            });

            _hubConnection.On("SessionDisconnected", async () =>
            {
                StopScreenStreaming();
                await Dispatcher.InvokeAsync(() =>
                {
                    txtConnectionStatus.Text = "Status: Session Disconnected";
                    txtHostExpiryInfo.Text = "Remote session ended.";
                });
            });

            await _hubConnection.StartAsync();
        }

        private void StartScreenStreaming(string remoteCode)
        {
            StopScreenStreaming();
            _hostStreamCts = new CancellationTokenSource();
            CancellationToken token = _hostStreamCts.Token;

            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (_hubConnection?.State == HubConnectionState.Connected)
                        {
                            byte[]? frameBytes = ScreenFrameCapturer.CapturePrimaryScreenJpeg(65, 1280, 720);
                            if (frameBytes != null && frameBytes.Length > 0)
                            {
                                await _hubConnection.InvokeAsync("SendScreenFrame", remoteCode, frameBytes, token);
                            }
                        }

                        await Task.Delay(100, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        await Task.Delay(250, token);
                    }
                }
            }, token);
        }

        private void StopScreenStreaming()
        {
            if (_hostStreamCts != null)
            {
                _hostStreamCts.Cancel();
                _hostStreamCts.Dispose();
                _hostStreamCts = null;
            }
        }

        private async Task StopHostingAsync()
        {
            StopScreenStreaming();
            _isHosting = false;

            if (!string.IsNullOrWhiteSpace(_activeHostRemoteCode))
            {
                try
                {
                    if (_hubConnection?.State == HubConnectionState.Connected)
                    {
                        await _hubConnection.InvokeAsync("DisconnectSession", _activeHostRemoteCode);
                    }

                    await _httpClient.PostAsync($"{ApiBaseUrl}/api/RemoteAccess/CloseSession/{_activeHostRemoteCode}", null);
                }
                catch
                {
                }

                _activeHostRemoteCode = null;
            }

            txtMyRemoteId.Text = string.Empty;
            txtMyPassword.Text = string.Empty;
            btnGenerateSession.IsEnabled = true;
            btnStopHosting.IsEnabled = false;
            txtConnectionStatus.Text = "Status: Ready";
            txtHostExpiryInfo.Text = "Session stopped.";
        }

        private void btnCloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void RemoteAccessWindow_Closed(object? sender, EventArgs e)
        {
            await StopHostingAsync();

            if (_hubConnection != null)
            {
                try
                {
                    await _hubConnection.DisposeAsync();
                }
                catch
                {
                }
            }
        }
    }

    internal static class ScreenFrameCapturer
    {
        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        public static byte[]? CapturePrimaryScreenJpeg(long quality, int targetWidth, int targetHeight)
        {
            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);

            if (screenWidth <= 0 || screenHeight <= 0)
            {
                return null;
            }

            using var fullBitmap = new Bitmap(screenWidth, screenHeight, PixelFormat.Format24bppRgb);
            using (var gFull = Graphics.FromImage(fullBitmap))
            {
                gFull.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(screenWidth, screenHeight), CopyPixelOperation.SourceCopy);
            }

            using var resizedBitmap = new Bitmap(targetWidth, targetHeight, PixelFormat.Format24bppRgb);
            using (var gResized = Graphics.FromImage(resizedBitmap))
            {
                gResized.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                gResized.DrawImage(fullBitmap, 0, 0, targetWidth, targetHeight);
            }

            ImageCodecInfo? jpegCodec = GetEncoder(ImageFormat.Jpeg);
            if (jpegCodec == null)
            {
                return null;
            }

            using var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

            using var ms = new MemoryStream();
            resizedBitmap.Save(ms, jpegCodec, encoderParams);
            return ms.ToArray();
        }

        private static ImageCodecInfo? GetEncoder(ImageFormat format)
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }

    internal static class RemoteInputInjector
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private const int INPUT_MOUSE = 0;
        private const int INPUT_KEYBOARD = 1;

        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public static void HandleRemoteInput(string eventType, double xRatio, double yRatio, int keyCodeOrButton)
        {
            double clampedX = Math.Max(0.0, Math.Min(1.0, xRatio));
            double clampedY = Math.Max(0.0, Math.Min(1.0, yRatio));

            int absX = (int)Math.Round(clampedX * 65535.0);
            int absY = (int)Math.Round(clampedY * 65535.0);

            if (eventType == "MouseMove")
            {
                SendMouse(absX, absY, MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE);
            }
            else if (eventType == "MouseDown")
            {
                uint flag = keyCodeOrButton == 1 ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_LEFTDOWN;
                SendMouse(absX, absY, MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | flag);
            }
            else if (eventType == "MouseUp")
            {
                uint flag = keyCodeOrButton == 1 ? MOUSEEVENTF_RIGHTUP : MOUSEEVENTF_LEFTUP;
                SendMouse(absX, absY, MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | flag);
            }
            else if (eventType == "KeyDown")
            {
                SendKey((ushort)keyCodeOrButton, 0);
            }
            else if (eventType == "KeyUp")
            {
                SendKey((ushort)keyCodeOrButton, KEYEVENTF_KEYUP);
            }
        }

        private static void SendMouse(int absX, int absY, uint flags)
        {
            var input = new INPUT
            {
                type = INPUT_MOUSE,
                u = new InputUnion
                {
                    mi = new MOUSEINPUT
                    {
                        dx = absX,
                        dy = absY,
                        mouseData = 0,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        }

        private static void SendKey(ushort virtualKey, uint flags)
        {
            var input = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = virtualKey,
                        wScan = 0,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            SendInput(1, new[] { input }, Marshal.SizeOf<INPUT>());
        }
    }

    public class RemoteScreenViewerWindow : Window
    {
        private readonly HubConnection _hubConnection;
        private readonly string _remoteCode;
        private readonly System.Windows.Controls.Image _screenImage;

        public RemoteScreenViewerWindow(HubConnection hubConnection, string remoteCode)
        {
            _hubConnection = hubConnection;
            _remoteCode = remoteCode;

            Title = $"Remote Screen - {remoteCode}";
            Width = 1024;
            Height = 640;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var rootGrid = new System.Windows.Controls.Grid();
            _screenImage = new System.Windows.Controls.Image
            {
                Stretch = System.Windows.Media.Stretch.Fill
            };

            _screenImage.MouseMove += ScreenImage_MouseMove;
            _screenImage.MouseLeftButtonDown += ScreenImage_MouseLeftButtonDown;
            _screenImage.MouseLeftButtonUp += ScreenImage_MouseLeftButtonUp;
            _screenImage.MouseRightButtonDown += ScreenImage_MouseRightButtonDown;
            _screenImage.MouseRightButtonUp += ScreenImage_MouseRightButtonUp;
            KeyDown += RemoteScreenViewerWindow_KeyDown;
            KeyUp += RemoteScreenViewerWindow_KeyUp;

            rootGrid.Children.Add(_screenImage);
            Content = rootGrid;

            _hubConnection.On<byte[]>("ReceiveScreenFrame", async (imageBytes) =>
            {
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    return;
                }

                await Dispatcher.InvokeAsync(() =>
                {
                    var bitmap = new BitmapImage();
                    using var ms = new MemoryStream(imageBytes);
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    _screenImage.Source = bitmap;
                });
            });
        }

        private async void ScreenImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var pos = e.GetPosition(_screenImage);
            if (_screenImage.ActualWidth <= 0 || _screenImage.ActualHeight <= 0) return;

            double xRatio = pos.X / _screenImage.ActualWidth;
            double yRatio = pos.Y / _screenImage.ActualHeight;

            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendInputEvent", _remoteCode, "MouseMove", xRatio, yRatio, 0);
            }
        }

        private async void ScreenImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(_screenImage);
            if (_screenImage.ActualWidth <= 0 || _screenImage.ActualHeight <= 0) return;

            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendInputEvent", _remoteCode, "MouseDown", pos.X / _screenImage.ActualWidth, pos.Y / _screenImage.ActualHeight, 0);
            }
        }

        private async void ScreenImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(_screenImage);
            if (_screenImage.ActualWidth <= 0 || _screenImage.ActualHeight <= 0) return;

            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendInputEvent", _remoteCode, "MouseUp", pos.X / _screenImage.ActualWidth, pos.Y / _screenImage.ActualHeight, 0);
            }
        }

        private async void ScreenImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(_screenImage);
            if (_screenImage.ActualWidth <= 0 || _screenImage.ActualHeight <= 0) return;

            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendInputEvent", _remoteCode, "MouseDown", pos.X / _screenImage.ActualWidth, pos.Y / _screenImage.ActualHeight, 1);
            }
        }

        private async void ScreenImage_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(_screenImage);
            if (_screenImage.ActualWidth <= 0 || _screenImage.ActualHeight <= 0) return;

            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendInputEvent", _remoteCode, "MouseUp", pos.X / _screenImage.ActualWidth, pos.Y / _screenImage.ActualHeight, 1);
            }
        }

        private async void RemoteScreenViewerWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int vk = KeyInterop.VirtualKeyFromKey(e.Key);
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendInputEvent", _remoteCode, "KeyDown", 0.0, 0.0, vk);
            }
        }

        private async void RemoteScreenViewerWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int vk = KeyInterop.VirtualKeyFromKey(e.Key);
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendInputEvent", _remoteCode, "KeyUp", 0.0, 0.0, vk);
            }
        }
    }
}