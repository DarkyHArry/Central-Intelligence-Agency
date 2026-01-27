using System.Collections.Generic;
using CIA.HelpDesk.Storage;

namespace CIA.HelpDesk.Storage
{
    public class CentralRepository
    {
        public List<Criminoso> Criminosos { get; } = new List<Criminoso>();
        public List<Operacao> Operacoes { get; } = new List<Operacao>();
        public List<GovernmentSupport> Supports { get; } = new List<GovernmentSupport>();
    }
}
