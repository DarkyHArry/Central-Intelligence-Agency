using System.Collections.Generic;
using CIA.HelpDesk.Models;

namespace CIA.HelpDesk.Interfaces
{
    public interface IChamadoRepository
    {
        void Add(Chamado chamado);
        IEnumerable<Chamado> GetAll();
        IEnumerable<Chamado> GetByStatus(string status);
        IEnumerable<Chamado> GetByTecnico(Tecnico tecnico);
    }
}
