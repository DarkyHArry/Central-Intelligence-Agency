
namespace CIA.HelpDesk.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Tipo { get; set; }
        public string Documento { get; set; }
        public string Endereco { get; set; }

        public Cliente() { }

        public Cliente(int id, string nome, string tipo, string documento, string endereco = "")
        {
            Id = id;
            Nome = nome;
            Tipo = tipo;
            Documento = documento;
            Endereco = endereco ?? "";
        }
    }

}
