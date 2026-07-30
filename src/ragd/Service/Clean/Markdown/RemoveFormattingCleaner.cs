namespace ragd.Service.Clean.Markdown
{
    /// <summary>
    /// Remove markdown formatting characters
    /// </summary>
    public class RemoveFormattingCleaner : ICleaner
    {
        public string Clean(string chunk)
        {
            // TODO: Do i need this?
            // remove inline code, bold, italic

            throw new NotImplementedException();
        }
    }
}
