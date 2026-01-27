namespace CIA.HelpDesk.Models
{
    // Classe abstrata base para usuários (Tecnico, etc.)
    public abstract class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string DocumentoIdentificacao { get; set; } // CPF, Passaporte ou ID Militar

        protected Usuario() { }

        protected Usuario(int id, string nome, string documento = null)
        {
            Id = id;
            Nome = nome;
            DocumentoIdentificacao = documento ?? string.Empty;
        }
    }

    // Tecnico é o agente que pode ser atribuído a chamados
    public class Tecnico : Usuario
    {
        public string Codinome { get; set; }
        public Tecnico() { }

        public Tecnico(int id, string nome, string codinome, string documento = null)
            : base(id, nome, documento)
        {
            Codinome = codinome;
        }
    }
}