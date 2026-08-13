namespace ragd.Clean.Markdown
{
    public class CodeBlockCleaner : ICleaner
    {
        public string Clean(string chunk)
        {
            chunk = chunk.Replace("```", "");
            chunk = chunk.Replace("~~~", "");

            return chunk;
        }
    }
}
