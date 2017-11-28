namespace Calculon.Models
{
    public class Session
    {
        public SessionState State { get; set; }
    }

    public enum SessionState
    {
        FirstAccess,
        Answering,
        FirstNumber,
        SecondNumber,
        Operation,
        Restart,
        End
    }
}