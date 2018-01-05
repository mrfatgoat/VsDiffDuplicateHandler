namespace VsDiffDuplicateHandler.Configuration
{
    public interface IDuplicateHandlerConfiguration
    {
        bool DryRun { get; }
        string GoodPath { get; }
        string DuplicateFilePath { get; }
    }
}
