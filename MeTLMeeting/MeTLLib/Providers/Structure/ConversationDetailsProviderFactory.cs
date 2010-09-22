using MeTLLib.Providers;
namespace MeTLLib.Providers.Structure
{
    class ConversationDetailsProviderFactory
    {
        private static object instanceLock = new object();
        private static IConversationDetailsProvider instance;
        public static IConversationDetailsProvider Provider{
            get { 
                lock(instanceLock){
                    if (instance == null)
                        instance = new FileConversationDetailsProvider();
                    return instance;
                }
            }
        }
    }
}
