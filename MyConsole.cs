namespace MyConsole2
{
    public static class MyConsole
    {
        private const string startCommandLine = "PS>";
        private const string paramNotFoundExceptionText = "Не удалось найти подходящий параметр!";
        private const string paramNotExistsExceptionText = "У данной команды отсутствуют параметры!"; 
        private const string commandNotFoundExceptionText = "Комманда не найдена!";

        public static void StartConsole()
        {
            string? commandLineText = "";
            while(true)
            {
                Console.Write(startCommandLine);
                commandLineText = Console.ReadLine();

                if (commandLineText == null || commandLineText == "") continue;
                var commandText = commandLineText.Split();

                try
                {
                    switch (commandText[0])
                    {
                        case "Create":
                            if (commandText.Length != 2) throw new ArgumentException(paramNotFoundExceptionText);
                            ConsoleCommands.Create(commandText[1]);
                            break;
                        case "Open":
                            if (commandText.Length != 2) throw new ArgumentException(paramNotFoundExceptionText);
                            ConsoleCommands.Open(commandText[1]);
                            break;
                        case "Input":
                            if (commandText.Length == 2)
                            {
                                var tmp = commandText[1].Split('/');
                                ConsoleCommands.Input(tmp[0], tmp[1]);
                            }
                            if (commandText.Length == 3) ConsoleCommands.Input(commandText[1], commandText[2].ToCompanentType());
                            break;
                        case "Delete":
                            if (commandText.Length != 2) throw new ArgumentException(paramNotFoundExceptionText);
                            if (commandText[1].Contains('/'))
                            {
                                var tmp = commandText[1].Split('/');
                                ConsoleCommands.Delete(tmp[0], tmp[1]);
                            }
                            else ConsoleCommands.Delete(commandText[1]);
                            break;
                        case "Restore":
                            if (commandText.Length != 2) throw new ArgumentException(paramNotFoundExceptionText);
                            if (commandText[1] == "*") ConsoleCommands.Restore();
                            else ConsoleCommands.Restore(commandText[1]);
                            break;
                        case "Truncate":
                            if (commandText.Length != 1) throw new ArgumentException(paramNotExistsExceptionText);
                            ConsoleCommands.Truncate();
                            break;
                        case "Print":
                            if (commandText.Length != 2) throw new ArgumentException(paramNotFoundExceptionText);
                            if (commandText[1] == "*") ConsoleCommands.Print();
                            else ConsoleCommands.Print(commandText[1]);
                            break;
                        case "Help":
                            if (commandText.Length > 2) throw new ArgumentException(paramNotFoundExceptionText);
                            if (commandText.Length == 1) ConsoleCommands.Help();
                            else if (commandText.Length == 2) ConsoleCommands.Help(commandText[1]);

                            break;
                        case "Exit":
                            if (commandText.Length != 1) throw new ArgumentException(paramNotExistsExceptionText);
                            ConsoleCommands.Exit();
                            return;
                        default:
                            throw new ArgumentException(commandNotFoundExceptionText);
                            
                    }
                }
                catch(ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (NotImplementedException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
