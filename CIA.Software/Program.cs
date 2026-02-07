using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using CIA.HelpDesk.Models;
using CIA.HelpDesk.Services;
using CIA.HelpDesk.Utils;

/// ╔═══════════════════════════════════════════════════════════════════════════╗
/// ║              CIA IT HELP DESK SYSTEM - SISTEMA DE CHAMADOS               ║
/// ║                                                                           ║
/// ║ Este programa permite que funcionários abram chamados de TI e que         ║
/// ║ técnicos gerenciem e resolvam esses chamados através de um painel.       ║
/// ╚═══════════════════════════════════════════════════════════════════════════╝

// Inicializa os serviços de persistência e repositório de chamados
var persistence = new PersistenceService();
var repo = new InMemoryChamadoRepository(persistence);
var service = new ChamadoService(repo);

// Gera um código administrativo único e temporário para acesso do técnico
var adminCode = GenerateAdminCode();

ColorConsole.PrintMenu("═══════════════════════════════════════════════════════════════");
ColorConsole.PrintMenu("                   CIA IT HELP DESK SYSTEM                     ");
ColorConsole.PrintMenu("═══════════════════════════════════════════════════════════════");
Console.WriteLine();
ColorConsole.WriteMenu("Você é: ");
ColorConsole.WriteCorporation("1) Técnico De TI-CIA OFFICE");
Console.Write("  ");
ColorConsole.WriteCivil("2) Funcionário (Solicitante)");
Console.WriteLine();

// Menu principal de escolha entre Técnico e Funcionário
var role = Console.ReadLine();

// ═══════════════════════════════════════════════════════════════════════════
// FLUXO DE FUNCIONÁRIO (Solicitante)
// ═══════════════════════════════════════════════════════════════════════════
if (role == "2")
{
    ColorConsole.PrintCivil("╔════════════════════════════════════════════════════════════╗");
    ColorConsole.PrintCivil("║              SISTEMA DE ABERTURA DE CHAMADOS               ║");
    ColorConsole.PrintCivil("╚════════════════════════════════════════════════════════════╝");
    Console.WriteLine();
    
    // Coleta o nome completo do funcionário
    ColorConsole.WriteCivil("Seu Nome completo: ");
    var nome = Console.ReadLine() ?? string.Empty;
    
    // Valida se o nome contém apenas letras e espaços
    if (!IsValidName(nome)) 
    { 
        ColorConsole.PrintError("Nome inválido. Use apenas letras e espaços."); 
        return; 
    }

    // Carrega lista de clientes/funcionários já cadastrados
    var clients = persistence.LoadClients();
    var cliente = clients.Find(c => string.Equals(c.Nome, nome, StringComparison.InvariantCultureIgnoreCase));

    // Se o funcionário não existe no sistema, realiza novo cadastro
    if (cliente == null)
    {
        // Solicitação de CPF com validação de duplicidade
        string documento = string.Empty;
        bool cpfValido = false;
        
        while (!cpfValido)
        {
            ColorConsole.WriteCivil("CPF (11 dígitos): ");
            documento = Console.ReadLine() ?? string.Empty;
            
            // Valida o formato do CPF (11 dígitos numéricos)
            if (!IsValidDocument(documento)) 
            { 
                ColorConsole.PrintError("CPF inválido. Deve ter 11 dígitos."); 
                continue;
            }
            
            // Verifica se o CPF já está cadastrado no sistema
            if (CpfJaExiste(documento, clients))
            {
                ColorConsole.PrintError("✗ Este CPF já está cadastrado no sistema. Digite um CPF diferente.");
                continue;
            }
            
            cpfValido = true;
        }
        
        // Coleta o departamento/setor do funcionário
        ColorConsole.WriteCivil("Seu Departamento/Setor: ");
        var endereco = Console.ReadLine() ?? string.Empty;
        
        // Cria novo cliente com os dados coletados
        cliente = new Cliente(clients.Count + 1, nome, "Funcionário", documento, endereco);
        clients.Add(cliente);
        persistence.SaveClients(clients);
        ColorConsole.PrintSuccess("Cadastro de funcionário realizado.");
    }
    else
    {
        // Se o funcionário existe, apenas atualiza o departamento/setor se necessário
        ColorConsole.WriteCivil("Confirmar Departamento/Setor: ");
        var endereco = Console.ReadLine() ?? string.Empty;
        if (!string.IsNullOrEmpty(endereco))
        {
            cliente.Endereco = endereco;
            persistence.SaveClients(clients);
        }
    }

    // Coleta a descrição do problema técnico
    ColorConsole.WriteCivil("Descreva o problema técnico: ");
    var descricao = Console.ReadLine() ?? string.Empty;
    
    // Analisa a descrição e detecta automaticamente a categoria do problema
    var categoriaDetectada = AnalyzeProblem(descricao);
    ColorConsole.PrintInfo($"➜  Categoria detectada: {categoriaDetectada}");

    // Cria o chamado e salva na persistência
    service.CriarChamado(descricao, cliente, categoriaDetectada, cliente.Endereco);
    persistence.SaveChamados(service.ListarChamados());
    ColorConsole.PrintSuccess("✓ Chamado de TI aberto com sucesso.");
}

// ═══════════════════════════════════════════════════════════════════════════
// FLUXO DE TÉCNICO DE TI
// ═══════════════════════════════════════════════════════════════════════════
else if (role == "1")
{
    ColorConsole.PrintCorporation("╔════════════════════════════════════════════════════════════╗");
    ColorConsole.PrintCorporation("║                PAINEL DO TÉCNICO DE TI                    ║");
    ColorConsole.PrintCorporation("╚════════════════════════════════════════════════════════════╝");
    Console.WriteLine();
    
    // Exibe o código de acesso temporário gerado para o técnico
    ColorConsole.WriteInfo($"Código de acesso técnico (temporário): ");
    ColorConsole.PrintInfo(adminCode);
    ColorConsole.WriteCorporation("Digite o código para acessar o painel: ");
    var input = Console.ReadLine() ?? string.Empty;

    // Valida o código de acesso do técnico
    if (input != adminCode)
    {
        ColorConsole.PrintError("✗ Código inválido. Acesso negado.");
        return;
    }

    ColorConsole.PrintSuccess("✓ Acesso técnico concedido.");
    
    // Loop principal do painel do técnico
    bool running = true;
    while (running)
    {
        Console.WriteLine();
        ColorConsole.PrintMenu("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        ColorConsole.PrintMenu("MENU DE GESTÃO DE TI");
        ColorConsole.PrintMenu("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.Write("  ");
        ColorConsole.WriteCorporation("1) Listar Chamados Abertos");
        Console.Write("      ");
        ColorConsole.WriteCorporation("2) Atender Chamado");
        Console.WriteLine();
        Console.Write("  ");
        ColorConsole.WriteCorporation("3) Encerrar Chamado");
        Console.Write("           ");
        ColorConsole.WriteMenu("4) Histórico Geral");
        Console.WriteLine();
        Console.Write("  ");
        ColorConsole.WriteMenu("0) Sair");
        Console.WriteLine();
        ColorConsole.PrintMenu("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        ColorConsole.WriteMenu("Escolha: ");
        
        // Processa a opção selecionada pelo técnico
        var opt = Console.ReadLine();
        switch (opt)
        {
            case "1":
                ListarChamados(service, "Aberto");
                break;
            case "2":
                AtenderChamado(service, persistence);
                break;
            case "3":
                EncerrarChamado(service, persistence);
                break;
            case "4":
                ListarChamados(service, null);
                break;
            case "0":
                running = false; 
                ColorConsole.PrintSuccess("✓ Encerrando sistema...");
                break;
            default:
                ColorConsole.PrintError("✗ Opção inválida."); 
                break;
        }
    }
}
else
{
    // Opção inválida no menu inicial
    ColorConsole.PrintError("✗ Opção inválida. Encerrando.");
}

// ═══════════════════════════════════════════════════════════════════════════
// FUNÇÕES AUXILIARES
// ═══════════════════════════════════════════════════════════════════════════

///
/// Analisa a descrição do problema e detecta automaticamente a categoria
/// baseado em palavras-chave presentes no texto
///
Categoria AnalyzeProblem(string texto)
{
    var lower = texto.ToLowerInvariant();
    
    // Detecta problemas de segurança
    if (lower.Contains("senha") || lower.Contains("login") || lower.Contains("acesso") || lower.Contains("bloqueado"))
        return Categoria.Segurança;
    
    // Detecta problemas de hardware/TI
    if (lower.Contains("computador") || lower.Contains("lento") || lower.Contains("mouse") || lower.Contains("teclado") || lower.Contains("monitor") || lower.Contains("impressora"))
        return Categoria.TI;
    
    // Detecta problemas de rede/conectividade
    if (lower.Contains("internet") || lower.Contains("rede") || lower.Contains("wifi") || lower.Contains("conexão") || lower.Contains("cabo"))
        return Categoria.Transporte;

    // Categoria padrão
    return Categoria.Geral;
}

/// <summary>
/// Lista todos os chamados com opção de filtrar por status
/// </summary>
void ListarChamados(ChamadoService svc, string status)
{
    var chamados = status == null ? svc.ListarChamados() : svc.ListarChamados(status);
    Console.WriteLine();
    ColorConsole.PrintMenu($"════════ LISTA DE CHAMADOS {(status?.ToUpper() ?? "GERAL")} ════════");
    if (!chamados.Any())
    {
        ColorConsole.PrintInfo("Nenhum chamado encontrado.");
        return;
    }
    foreach (var c in chamados)
    {
        ColorConsole.PrintInfo($"ID: {c.Id} | Solicitante: {c.Solicitante.Nome} | Categoria: {c.Categoria} | Status: {c.Status}");
        Console.WriteLine($"   Descrição: {c.Descricao}");
    }
}

/// <summary>
/// Permite que o técnico atenda um chamado aberto
/// Altera o status para "Em Andamento" e atribui o técnico responsável
/// </summary>
void AtenderChamado(ChamadoService svc, PersistenceService ps)
{
    ColorConsole.WriteCorporation("ID do chamado para atender: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        var chamado = svc.ListarChamados().FirstOrDefault(c => c.Id == id);
        if (chamado != null && chamado.Status == "Aberto")
        {
            // Cria e atribui um técnico ao chamado
            var tecnico = new Tecnico(1, "Técnico de Plantão", "TECH_01");
            chamado.AtribuirTecnico(tecnico);
            ps.SaveChamados(svc.ListarChamados());
            ColorConsole.PrintSuccess($"✓ Chamado {id} agora está EM ANDAMENTO.");
        }
        else ColorConsole.PrintError("Chamado não encontrado ou já em atendimento.");
    }
}

/// <summary>
/// Permite que o técnico encerre um chamado que está em andamento
/// Altera o status para "Encerrado" e registra no histórico
/// </summary>
void EncerrarChamado(ChamadoService svc, PersistenceService ps)
{
    ColorConsole.WriteCorporation("ID do chamado para encerrar: ");
    if (int.TryParse(Console.ReadLine(), out int id))
    {
        var chamado = svc.ListarChamados().FirstOrDefault(c => c.Id == id);
        if (chamado != null && chamado.Status == "Em Andamento")
        {
            chamado.EncerrarChamado();
            ps.SaveChamados(svc.ListarChamados());
            ColorConsole.PrintSuccess($"✓ Chamado {id} ENCERRADO com sucesso.");
        }
        else ColorConsole.PrintError("Chamado não encontrado ou não está em andamento.");
    }
}

/// <summary>
/// Valida se o nome contém apenas letras, espaços e caracteres válidos
/// </summary>
bool IsValidName(string nome)
{
    if (string.IsNullOrWhiteSpace(nome)) return false;
    return Regex.IsMatch(nome, @"^[\p{L}\p{M}\s''-]+$", RegexOptions.IgnoreCase);
}

/// <summary>
/// Valida se o CPF tem exatamente 11 dígitos numéricos
/// </summary>
bool IsValidDocument(string doc)
{
    if (string.IsNullOrWhiteSpace(doc)) return false;
    return Regex.IsMatch(doc, "^\\d{11}$");
}

/// <summary>
/// Verifica se um CPF já existe na lista de clientes cadastrados
/// </summary>
bool CpfJaExiste(string cpf, List<Cliente> clients)
{
    return clients.Any(c => c.Documento == cpf);
}

/// <summary>
/// Gera um código administrativo único e aleatório para acesso do técnico
/// </summary>
string GenerateAdminCode()
{
    return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
}
