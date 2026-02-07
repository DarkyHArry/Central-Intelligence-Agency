using System;

namespace CIA.HelpDesk.Utils
{
    public static class ColorConsole
    {
        // Cores ANSI
        public const string RESET = "\u001b[0m";
        public const string GREEN = "\u001b[32m";
        public const string BRIGHT_GREEN = "\u001b[92m";
        public const string BRIGHT_RED = "\u001b[91m";
        public const string BRIGHT_YELLOW = "\u001b[93m";
        public const string BRIGHT_CYAN = "\u001b[96m";
        public const string BOLD = "\u001b[1m";
        
        /// <summary>Imprime texto em verde (menu)</summary>
        public static void PrintMenu(string text)
        {
            Console.WriteLine($"{BRIGHT_GREEN}{BOLD}{text}{RESET}");
        }
        
        /// <summary>Imprime texto em verde para civis</summary>
        public static void PrintCivil(string text)
        {
            Console.WriteLine($"{GREEN}{text}{RESET}");
        }
        
        /// <summary>Imprime texto em vermelho para corporações</summary>
        public static void PrintCorporation(string text)
        {
            Console.WriteLine($"{BRIGHT_RED}{text}{RESET}");
        }
        
        /// <summary>Imprime texto em amarelo para informações</summary>
        public static void PrintInfo(string text)
        {
            Console.WriteLine($"{BRIGHT_YELLOW}{text}{RESET}");
        }
        
        /// <summary>Imprime texto em cyan para sucesso</summary>
        public static void PrintSuccess(string text)
        {
            Console.WriteLine($"{BRIGHT_CYAN}{text}{RESET}");
        }
        
        /// <summary>Imprime texto em vermelho para erros</summary>
        public static void PrintError(string text)
        {
            Console.WriteLine($"{BRIGHT_RED}{text}{RESET}");
        }
        
        /// <summary>Escreve sem quebra de linha em verde</summary>
        public static void WriteMenu(string text)
        {
            Console.Write($"{BRIGHT_GREEN}{BOLD}{text}{RESET}");
        }
        
        /// <summary>Escreve sem quebra de linha em verde para civis</summary>
        public static void WriteCivil(string text)
        {
            Console.Write($"{GREEN}{text}{RESET}");
        }
        
        /// <summary>Escreve sem quebra de linha em vermelho para corporações</summary>
        public static void WriteCorporation(string text)
        {
            Console.Write($"{BRIGHT_RED}{text}{RESET}");
        }
        
        /// <summary>Escreve sem quebra de linha em amarelo</summary>
        public static void WriteInfo(string text)
        {
            Console.Write($"{BRIGHT_YELLOW}{text}{RESET}");
        }
    }
}
