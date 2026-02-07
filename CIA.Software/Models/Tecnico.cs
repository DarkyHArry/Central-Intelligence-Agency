namespace CIA.HelpDesk.Models
{
    public class Tecnico
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Codinome { get; set; } = string.Empty;

        public Tecnico() { }

        public Tecnico(int id, string nome, string codinome)
        {
            Id = id;
            Nome = nome;
            Codinome = codinome;
        }
    }
}