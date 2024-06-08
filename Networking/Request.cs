namespace Networking;

[Serializable]
public class Request
{
    public string Path { get; set; }
    public string UserName { get; set; }
    public byte[] Data { get; set; }
}
