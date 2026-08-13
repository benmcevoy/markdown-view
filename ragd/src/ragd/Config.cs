using System.Net;
using LLama.Native;

namespace ragd;

public class Config
{
    public string DatabasePath { get; set; } = "./ragd.db";
    public string ModelPath { get; set; } = "./ragd.gguf";
    public IPAddress Host { get; set; } = IPAddress.Loopback;
    public int Port { get; set; } = 53280;
    public string VectorExtensionPath { get; set; } = "./vec0.so";
    public uint? ContextSize { get; set; } = 4096;
    public LLamaAttentionType AttentionType { get; set; } = LLamaAttentionType.Unspecified;
    public int TopNResults { get; set; } = 3;
}