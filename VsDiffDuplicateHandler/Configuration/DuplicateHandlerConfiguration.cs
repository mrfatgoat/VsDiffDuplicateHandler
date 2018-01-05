namespace VsDiffDuplicateHandler.Configuration
{
    class DuplicateHandlerConfiguration : IDuplicateHandlerConfiguration
    {
        public bool DryRun { get; set; }
        public string GoodPath { get; set; }
        public string DuplicateFilePath { get; set; }
    }
}
