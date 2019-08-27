using System.Collections;

namespace SCTV
{
    class browserAttributes
    {
        public System.Windows.Forms.Timer RefreshTimer;
        public bool WatchingVideo = false;
        public bool FoundCategory = false;
        public bool FoundVideo = false;
        public System.Windows.Forms.Timer KeepRunningTimer;
        ArrayList videosList;
    }
}
