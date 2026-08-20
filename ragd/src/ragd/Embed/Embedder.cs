using LLama;
using LLama.Common;
using LLama.Extensions;

namespace ragd.Embed;

public interface IEmbedder
{
    int EmbeddingSize();
    Task<float[]> GetEmbedding(string content);
    int TrainedContextSize();
}

public class Embedder : IDisposable, IEmbedder
{
    private readonly LLamaWeights _weights;
    private readonly LLamaEmbedder _embedder;

    public Embedder(Config config)
    {
        var modelParameters = new ModelParams(config.ModelPath)
        {
            GpuLayerCount = -1,
            Embeddings = true
        };

        _weights = LLamaWeights.LoadFromFile(modelParameters);
        _embedder = new LLamaEmbedder(_weights, modelParameters);
    }

    /// <summary>
    /// Return euclidean normalized (L2) embedding vector
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<float[]> GetEmbedding(string content) =>
          (await _embedder.GetEmbeddings(content))[0].EuclideanNormalization();

    public int EmbeddingSize() => _embedder.EmbeddingSize;
    
    public int TrainedContextSize() => _weights.NativeHandle.ContextSize;

    private bool _isDisposing;
    public void Dispose()
    {
        if (_isDisposing) return;

        _isDisposing = true;
        _embedder.Dispose();
        _weights.Dispose();
    }
}