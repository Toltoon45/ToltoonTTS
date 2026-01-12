using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    const string ClientId = "ТВОЙ_CLIENT_ID";
    const string ClientSecret = "ТВОЙ_CLIENT_SECRET";
    const string RedirectUri = "http://localhost:5000/callback/";

    static async Task Main()
    {
        using var http = new HttpClient();

        // 1. Формируем URL авторизации
        var authUrl =
            "https://auth.live.vkvideo.ru/app/oauth2/authorize?" +
            $"client_id={ClientId}" +
            $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
            "&response_type=code" +
            "&scope=video offline" +
            "&audience=vkvideo_live";

        // 2. Открываем браузер
        Process.Start(new ProcessStartInfo
        {
            FileName = authUrl,
            UseShellExecute = true
        });

        // 3. Ловим code
        var code = await WaitForCodeAsync();
        Console.WriteLine($"CODE: {code}");

        // 4. Меняем code → access_token
        var accessToken = await GetAccessTokenAsync(http, code);
        Console.WriteLine($"ACCESS TOKEN: {accessToken}");

        // 5. Получаем WebSocket token
        var wsToken = await GetWebSocketTokenAsync(http, accessToken);
        Console.WriteLine($"WEBSOCKET TOKEN: {wsToken}");

        Console.WriteLine("ГОТОВО");
        Console.ReadLine();
    }

    static async Task<string> WaitForCodeAsync()
    {
        var listener = new HttpListener();
        listener.Prefixes.Add(RedirectUri);
        listener.Start();

        var context = await listener.GetContextAsync();
        var code = context.Request.QueryString["code"];

        var responseText = "Авторизация успешна. Можете закрыть окно.";
        var buffer = Encoding.UTF8.GetBytes(responseText);

        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer);
        context.Response.OutputStream.Close();

        listener.Stop();

        if (string.IsNullOrEmpty(code))
            throw new Exception("Code не получен");

        return code;
    }

    static async Task<string> GetAccessTokenAsync(HttpClient http, string code)
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("client_id", ClientId),
            new KeyValuePair<string, string>("client_secret", ClientSecret),
            new KeyValuePair<string, string>("redirect_uri", RedirectUri),
            new KeyValuePair<string, string>("code", code),
        });

        var response = await http.PostAsync(
            "https://auth.live.vkvideo.ru/app/oauth2/token",
            content);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<AccessTokenResponse>(json);

        return token!.access_token;
    }

    static async Task<string> GetWebSocketTokenAsync(HttpClient http, string accessToken)
    {
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await http.GetAsync(
            "https://api.live.vkvideo.ru/v1/websocket/token");

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var ws = JsonSerializer.Deserialize<WebSocketTokenResponse>(json);

        return ws!.data.token;
    }

    record AccessTokenResponse(
        string access_token,
        int expires_in,
        string token_type
    );

    record WebSocketTokenResponse(WebSocketTokenData data);
    record WebSocketTokenData(string token);
}
