using System;

namespace CIA.HelpDesk.Models
{
    public class HistoricoChamado
    {
        public DateTime Timestamp { get; set; }
        public string Evento { get; set; }

        public HistoricoChamado() { Evento = string.Empty; Timestamp = DateTime.UtcNow; }

        public HistoricoChamado(string evento)
        {
            Timestamp = DateTime.UtcNow;
            Evento = evento;
        }
    }
}
