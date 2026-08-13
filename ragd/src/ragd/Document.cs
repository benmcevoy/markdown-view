namespace ragd
{
    public record Document(
        string SourcePath,
        string Name,
        string Extension,
        string Content
    );
}