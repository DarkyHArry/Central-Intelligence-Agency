using CIA.HelpDesk.Interfaces;

namespace CIA.HelpDesk.Models
{
    public class Chamado : IAtribuivel, IEncerravel
    {
        public int Id { get; set; }
        public string Descricao { get; set; } // Ex: "Busca por fugitivo X" ou "Suporte em TI"
        public Categoria Categoria { get; set; }
        public string Status { get; set; } = "Aberto";
        public Cliente Solicitante { get; set; }
        public string Endereco { get; set; } // Novo campo para armazenar endereço do incidente
        public Tecnico? AgenteResponsavel { get; set; }

        // Composição: histórico faz parte do chamado
        public List<HistoricoChamado> Historicos { get; set; } = new List<HistoricoChamado>();

        // Parameterless constructor required for JSON deserialization
        public Chamado() { }

        public Chamado(int id, string descricao, Cliente solicitante, Categoria categoria = Categoria.Geral, string endereco = "")
        {
            Id = id;
            Descricao = descricao;
            Solicitante = solicitante;
            Categoria = categoria;
            Endereco = endereco;
            Status = "Aberto";
        }

        public void AtribuirTecnico(Tecnico tecnico)
        {
            AgenteResponsavel = tecnico;
            Status = "Em Andamento";
            Historicos.Add(new HistoricoChamado($"Agente {tecnico.Codinome} assumiu o caso."));
        }

        public void EncerrarChamado()
        {
            Status = "Encerrado";
            Historicos.Add(new HistoricoChamado("Chamado encerrado."));
        }

        public bool EstaEncerrado => Status == "Encerrado";
    }
}