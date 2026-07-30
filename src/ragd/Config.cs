using System.Net;
using LLama.Native;

namespace ragd;

public record Config
{
    public string DatabasePath { get; set; } = "";
    public string ModelPath { get; set; } = "";
    public IPAddress Host { get; set; } = IPAddress.Loopback;
    public int Port { get; set; } = 53280;
    public string VectorExtensionPath { get; set; } = "";
    public uint? ContextSize { get; set; } = 1024;
    public LLamaAttentionType AttentionType { get; set; } = LLamaAttentionType.NonCausal;
}