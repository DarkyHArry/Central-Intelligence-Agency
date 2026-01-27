using System;

namespace CIA.HelpDesk.Storage
{
    public class Operacao
    {
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataAgendada { get; set; }
        public string Origem { get; set; }
    }
}
