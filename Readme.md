# Projeto CIA.HelpDesk

Este repositório contém o código-fonte de um sistema de Help Desk desenvolvido em C#, focado na aplicação dos princípios SOLID para garantir um design de software robusto, manutenível e extensível.

## Diagrama de Classes UML

Abaixo está o diagrama de classes UML que ilustra a estrutura e as relações entre as principais entidades do sistema.

![Diagrama de Classes UML](https://private-us-east-1.manuscdn.com/sessionFile/5rXjaG1Y00Ce60vKqhsr4k/sandbox/VDHNqlfmL7mQzoiKgt1glZ-images_1770426194470_na1fn_L2hvbWUvdWJ1bnR1L2NpYV9yZXBvL2RpYWdyYW0.png?Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9wcml2YXRlLXVzLWVhc3QtMS5tYW51c2Nkbi5jb20vc2Vzc2lvbkZpbGUvNXJYamFHMVkwMENlNjB2S3Foc3I0ay9zYW5kYm94L1ZESE5xbGZtTDdtUXpvaUtndDFnbFotaW1hZ2VzXzE3NzA0MjYxOTQ0NzBfbmExZm5fTDJodmJXVXZkV0oxYm5SMUwyTnBZVjl5WlhCdkwyUnBZV2R5WVcwLnBuZyIsIkNvbmRpdGlvbiI6eyJEYXRlTGVzc1RoYW4iOnsiQVdTOkVwb2NoVGltZSI6MTc5ODc2MTYwMH19fV19&Key-Pair-Id=K2HSFNDJXOU9YS&Signature=EJbBxL77tTSm1uTrzj1IMjdVBNsilnphqRDPcve1wdYKdAckChU91hlFM0BBYAc7uMpYj6ue8LLdMjtqQBj~~k21jB9SuH~shTmUpWxqrrCxyByC~n2YWvvTq1ePlz5tNM4n4aFanpo6agGQfNN6jA47nPCyfy5fnG4E2sjS0ai0ciyLcgZyKDejSlfm46AjZbTxTSJ~at7gnMqvTsUroSzJnLPbBG2wAO~r5Vyqt5s-ao5Ju3Kg0iwLm4mkZjS8aw541~Z5pVSyLNHOb1rY-~G7MqlEbwBPcGN4zRfTYRdzIbu7YVEU8ZP0F3gV3ihMop63cxMgDlJQDdZh1i~izA__)

## Tecnologias Utilizadas

O projeto foi desenvolvido utilizando as seguintes tecnologias:

*   **C#**: Linguagem de programação principal.
*   **.NET**: Framework para desenvolvimento de aplicações.
*   **JSON**: Utilizado para persistência de dados (clientes e chamados) em arquivos.
*   **Mermaid**: Para a geração do diagrama de classes UML.

## Aplicação dos Princípios SOLID

Os princípios SOLID são um conjunto de cinco princípios de design de software que visam tornar os designs de software mais compreensíveis, flexíveis e manuteníveis. Abaixo, detalhamos como cada princípio foi aplicado neste projeto.

### 1. Princípio da Responsabilidade Única (SRP - Single Responsibility Principle)

O SRP afirma que uma classe deve ter apenas uma razão para mudar, ou seja, deve ter apenas uma responsabilidade. No projeto CIA.HelpDesk, observamos a aplicação deste princípio em diversas classes:

*   **`Chamado`**: Esta classe é responsável por representar um chamado e suas operações intrínsecas, como atribuir um técnico e encerrar o chamado. Ela não se preocupa com a persistência ou com a lógica de negócios de listagem de chamados.

    ```csharp
    public class Chamado : IAtribuivel, IEncerravel
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
        public Categoria Categoria { get; set; }
        public string Status { get; set; } = "Aberto";
        public Cliente Solicitante { get; set; }
        public string Endereco { get; set; }
        public Tecnico? AgenteResponsavel { get; set; }
        public List<HistoricoChamado> Historicos { get; set; } = new List<HistoricoChamado>();

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
    ```

*   **`ChamadoService`**: Esta classe é responsável pela lógica de negócios relacionada aos chamados, como criar e listar chamados. Ela delega a persistência a uma interface de repositório (`IChamadoRepository`).

    ```csharp
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

        // ... outros métodos de listagem
    }
    ```

*   **`PersistenceService`**: Esta classe tem a única responsabilidade de lidar com a persistência de dados em arquivos JSON, salvando e carregando `Clientes` e `Chamados`.

    ```csharp
    public class PersistenceService
    {
        private readonly string _dataDir;
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions { WriteIndented = true };

        public PersistenceService(string dataDir = "data")
        {
            _dataDir = dataDir;
            if (!Directory.Exists(_dataDir)) Directory.CreateDirectory(_dataDir);
        }

        // ... métodos para salvar e carregar clientes e chamados
    }
    ```

### 2. Princípio Aberto/Fechado (OCP - Open/Closed Principle)

O OCP afirma que as entidades de software (classes, módulos, funções, etc.) devem ser abertas para extensão, mas fechadas para modificação. Isso é alcançado através do uso de abstrações (interfaces e classes abstratas).

*   **`IChamadoRepository` e `InMemoryChamadoRepository`**: A `ChamadoService` depende da interface `IChamadoRepository`. Isso significa que podemos estender o comportamento de persistência (por exemplo, criar um `DatabaseChamadoRepository`) sem modificar a `ChamadoService`.

    ```csharp
    // Interface
    public interface IChamadoRepository
    {
        void Add(Chamado chamado);
        IEnumerable<Chamado> GetAll();
        IEnumerable<Chamado> GetByStatus(string status);
        IEnumerable<Chamado> GetByTecnico(Tecnico tecnico);
    }

    // Implementação
    public class InMemoryChamadoRepository : IChamadoRepository
    {
        private readonly List<Chamado> _store = new List<Chamado>();
        private readonly PersistenceService? _persistence;

        public InMemoryChamadoRepository(PersistenceService persistence)
        {
            _persistence = persistence;
            var loaded = _persistence.LoadChamados();
            if (loaded != null) _store.AddRange(loaded);
        }

        // ... métodos de implementação da interface
    }
    ```

### 3. Princípio da Substituição de Liskov (LSP - Liskov Substitution Principle)

O LSP afirma que os objetos de um programa devem ser substituíveis por instâncias de seus subtipos sem alterar a correção do programa. Embora não haja uma hierarquia de herança complexa evidente para `Usuario`, `Tecnico` e `Cliente` no código fornecido (onde `Tecnico` e `Cliente` não herdam diretamente de `Usuario` como classes concretas), o uso de interfaces como `IAtribuivel` e `IEncerravel` em `Chamado` demonstra um passo em direção a este princípio. Se `Tecnico` e `Cliente` fossem subtipos de `Usuario` e `Chamado` interagisse com `Usuario` de forma polimórfica, o LSP seria mais explicitamente aplicado.

*   **`Chamado` implementando `IAtribuivel` e `IEncerravel`**: A classe `Chamado` implementa essas interfaces, o que significa que qualquer código que espera um `IAtribuivel` ou `IEncerravel` pode operar com um objeto `Chamado` sem problemas, garantindo que o comportamento esperado das interfaces seja mantido.

    ```csharp
    public class Chamado : IAtribuivel, IEncerravel
    {
        // ... implementação dos métodos da interface
    }
    ```

### 4. Princípio da Segregação de Interfaces (ISP - Interface Segregation Principle)

O ISP afirma que nenhum cliente deve ser forçado a depender de interfaces que não utiliza. Interfaces devem ser pequenas e coesas. Este projeto demonstra bem o ISP com interfaces específicas:

*   **`IAtribuivel` e `IEncerravel`**: Em vez de ter uma única interface grande com todas as funcionalidades de um chamado, o projeto as divide em interfaces menores e mais focadas. Uma classe que precisa apenas atribuir algo pode implementar `IAtribuivel`, e uma que precisa encerrar pode implementar `IEncerravel`.

    ```csharp
    // Interface para atribuição
    public interface IAtribuivel
    {
        void AtribuirTecnico(Tecnico tecnico);
    }

    // Interface para encerramento
    public interface IEncerravel
    {
        void EncerrarChamado();
        bool EstaEncerrado { get; }
    }
    ```

*   **`IChamadoRepository`**: Esta interface é coesa e foca apenas nas operações de persistência de chamados (adicionar, obter todos, obter por status, obter por técnico).

    ```csharp
    public interface IChamadoRepository
    {
        void Add(Chamado chamado);
        IEnumerable<Chamado> GetAll();
        IEnumerable<Chamado> GetByStatus(string status);
        IEnumerable<Chamado> GetByTecnico(Tecnico tecnico);
    }
    ```

### 5. Princípio da Inversão de Dependência (DIP - Dependency Inversion Principle)

O DIP afirma que módulos de alto nível não devem depender de módulos de baixo nível. Ambos devem depender de abstrações. Além disso, abstrações não devem depender de detalhes; detalhes devem depender de abstrações. Isso é evidente na injeção de dependência.

*   **`ChamadoService` dependendo de `IChamadoRepository`**: A `ChamadoService` (módulo de alto nível, lógica de negócios) não depende diretamente de uma implementação concreta de repositório (módulo de baixo nível, detalhes de persistência), mas sim da abstração `IChamadoRepository`. A implementação (`InMemoryChamadoRepository`) é injetada no construtor.

    ```csharp
    public class ChamadoService
    {
        private readonly IChamadoRepository _repository;

        public ChamadoService(IChamadoRepository repository)
        {
            _repository = repository;
        }
        // ...
    }
    ```

Este design permite que a `ChamadoService` seja testada e reutilizada com diferentes implementações de repositório sem a necessidade de modificações em seu código-fonte, demonstrando uma forte adesão ao DIP.
