
using Dicom.Network.Client.EventArguments;

namespace PlayBook3DTSL.Repository.PacsServer.PacsServerFactory
{
    public abstract class PacsConnector
    {
       
        public PacsConnector()
        {

        }


        public static void OnAssociationAccepted(object sender, AssociationAcceptedEventArgs e)
        {
            LogToDebugConsole($"Association was accepted by:{e.Association.RemoteHost}");
        }

        public static void OnAssociationRejected(object sender, AssociationRejectedEventArgs e)
        {
            LogToDebugConsole($"Association was rejected. Rejected Reason:{e.Reason}");
        }

        public static void OnAssociationReleased(object sender, EventArgs e)
        {
            LogToDebugConsole("Association was released. BYE BYE");
        }

        public static void LogToDebugConsole(string informationToLog)
        {
            Console.WriteLine(informationToLog);
        }
    }
}
