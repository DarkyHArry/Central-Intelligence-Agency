using System.Collections.Generic;
using System.Linq;
using CIA.HelpDesk.Interfaces;
using CIA.HelpDesk.Models;

namespace CIA.HelpDesk.Services
{
    public class ChamadoService
    {
        private readonly IChamadoRepository _repository;

        public ChamadoService(IChamadoRepository repository)
        {
            _repository = repository;
        }

        public void CriarChamado(string descricao, Cliente solicitante, Categoria categoria = Categoria.Geral, string endereco = "")
        {
            var nextId = _repository.GetAll().Count() + 1;
            var chamado = new Chamado(nextId, descricao, solicitante, categoria, endereco);
            _repository.Add(chamado);
        }

        // Polimorfismo: métodos sobrecarregados para listar chamados por diferentes critérios
        public List<Chamado> ListarChamados()
        {
            return _repository.GetAll().ToList();
        }

        public List<Chamado> ListarChamados(string status)
        {
            return _repository.GetByStatus(status).ToList();
        }

        public List<Chamado> ListarChamados(Tecnico tecnico)
        {
            return _repository.GetByTecnico(tecnico).ToList();
        }
    }
}