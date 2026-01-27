using System.Collections.Generic;
using System.Linq;
using CIA.HelpDesk.Interfaces;
using CIA.HelpDesk.Models;

namespace CIA.HelpDesk.Services
{
    public class InMemoryChamadoRepository : IChamadoRepository
    {
        private readonly List<Chamado> _store = new List<Chamado>();
        private readonly PersistenceService? _persistence;

        public InMemoryChamadoRepository()
        {
        }

        public InMemoryChamadoRepository(PersistenceService persistence)
        {
            _persistence = persistence;
            var loaded = _persistence.LoadChamados();
            if (loaded != null) _store.AddRange(loaded);
        }

        public void Add(Chamado chamado)
        {
            _store.Add(chamado);
            _persistence?.SaveChamados(_store);
        }

        public IEnumerable<Chamado> GetAll() => _store;

        public IEnumerable<Chamado> GetByStatus(string status) => _store.Where(c => c.Status == status);

        public IEnumerable<Chamado> GetByTecnico(Tecnico tecnico) => _store.Where(c => c.AgenteResponsavel == tecnico);
    }
}
