namespace ragd.Clean
{
    public interface ICleaner
    {
        /// <summary>
        /// If a human read only this chunk, would this character/token help them understand the content?
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        /// <remarks>
        /// If a human read only this chunk, would this character/token help them understand the content?
        /// 
        /// Cleaning also depends on the embedding model and whether it "understands" the content, 
        /// e.g. does it understand image alt text? DOes it even know what an image is?
        /// </remarks>
        string Clean(string chunk);
    }
}
