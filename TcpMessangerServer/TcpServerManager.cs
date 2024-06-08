﻿using Networking;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.IO;

namespace TcpMessangerServer;

public class TcpServerManager
{
    private TcpListener _listener;
    private IPAddress _address;
    private int _port;
    private IFormatter _formatter = new BinaryFormatter();
    public Dictionary<string, TcpClient> _clients = new();

    public event Action<Request>? Received;

    public void Connect(string address, int port)
    {
        _address = IPAddress.Parse(address);
        _port = port;
        _listener = new TcpListener(_address, _port);
        _listener.Start();
        Thread thread = new Thread(Listen);
        thread.IsBackground = true;
        thread.Start();
    }

    public void Send(Request request)
    {
        foreach(var client in _clients.Values)
        {
            Send(request, client);
        }
    }

    private void Send(Request request, TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        MemoryStream memoryStream = new MemoryStream();
        _formatter.Serialize(memoryStream, request);
        byte[] buffer = memoryStream.ToArray();
        stream.Write(buffer, 0, buffer.Length);
        stream.Flush();
    }

    private void Listen()
    {
        while (true)
        {
            TcpClient client = _listener.AcceptTcpClient();

            Thread thread = new Thread(() => ListenClient(client));
            thread.IsBackground = true;
            thread.Start();
        }
    }

    private void ListenClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        StreamReader streamReader;
        try
        {
            while (true)
            {
                streamReader = new StreamReader(stream);
                if (stream.DataAvailable)
                {
                    Request request = (Request)_formatter.Deserialize(streamReader.BaseStream);

                    if (request.Path == "login")
                    {
                        string username = Encoding.UTF8.GetString(request.Data);
                        if (_clients.ContainsKey(username))
                        {
                            Request response = new Request()
                            {
                                Path = "error",
                                Data = Encoding.UTF8.GetBytes("Таке ім'я вже використовується")
                            };
                            Send(response, client);
                        }
                        else
                        {
                            _clients.Add(username, client);
                            Received?.Invoke(request);
                        }
                    }
                    else
                    {
                        Received?.Invoke(request);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка обробки даних: {ex.Message}");
        }
        
    }
}
