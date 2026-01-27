
namespace CIA.HelpDesk.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Tipo { get; set; } // "Civil" ou "Oficial"
        public string TipoSetor { get; set; } // Compatibilidade com dados antigos
        public string Documento { get; set; } // CPF
        public string DocumentoIdentificacao { get; set; } // Compatibilidade com dados antigos
        public string Endereco { get; set; } // Campo para armazenar endereço

        // Parameterless constructor required for JSON deserialization
        public Cliente() { }

        public Cliente(int id, string nome, string tipo, string documento, string endereco = "")
        {
            Id = id;
            Nome = nome;
            Tipo = tipo;
            TipoSetor = tipo; // Mantém compatibilidade
            Documento = documento;
            DocumentoIdentificacao = documento; // Mantém compatibilidade
            Endereco = endereco ?? "";
        }
    }
}