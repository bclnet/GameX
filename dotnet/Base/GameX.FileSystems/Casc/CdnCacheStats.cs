using System;

namespace GameX.FileSystems.Casc
{
    public static class CDNCacheStats
    {
        public static TimeSpan timeSpentDownloading = TimeSpan.Zero;
        public static int numFilesOpened = 0;
        public static int numFilesDownloaded = 0;
    }
}
