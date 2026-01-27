namespace CIA.HelpDesk.Interfaces
{
       public interface IEncerravel
    {
        void EncerrarChamado();
        bool EstaEncerrado { get; }
    }
}