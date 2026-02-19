namespace MyConsole2
{
    public class MyConsole
    {
        private const string startCommandLine = "PS>";
        private const string paramNotFoundExceptionText = "Не удалось найти подходящий параметр!";
        private const string paramNotExistsExceptionText = "У данной команды отсутствуют параметры!"; 
        private const string commandNotFoundExceptionText = "Команда не найдена!";

        public static void StartConsole()
        {
            string? commandLineText;
            while(true)
            {
                Console.Write(startCommandLine);
                commandLineText = Console.ReadLine();

                if (commandLineText == null || commandLineText == "") continue;
                var commandText = commandLineText.Split();
                ConsoleCommands commands = new ConsoleCommands();
                try
                {
                    switch (commandText[0])
                    {
                        case "Create":
                            if (commandText.Length != 2) throw new ArgumentException(paramNotFoundExceptionText);
                            commands.Create(commandText[1]);
                            break;
                        case "Open":
                            if (commandText.Length != 2) throw new ArgumentException(paramNotFoundExceptionText);
                            commands.Open(commandText[1]);
                            break;
                        case "Input":
                            if (commandText.Length == 2)
                            {
                                var tmp = commandText[1].Split('/');
                                commands.Input(tmp[0], tmp[1]);
                            }
                            if (commandText.Length == 3) commands.Input(commandText[1], commandText[2].ToCompanentType());
                            break;
                        case "Delete":
                            if (commandText.Length != 2) throw new ArgumentException(paramNotFoundExceptionText);
                            if (commandText[1].Contains('/'))
                            {
                                var tmp = commandText[1].Split('/');
                                commands.Delete(tmp[0], tmp[1]);
                            }
                            else commands.Delete(commandText[1]);
                            break;
                        case "Restore":
                            if (commandText.Length != 2) throw new ArgumentException(paramNotFoundExceptionText);
                            if (commandText[1] == "*") commands.Restore();
                            else commands.Restore(commandText[1]);
                            break;
                        case "Truncate":
                            if (commandText.Length != 1) throw new ArgumentException(paramNotExistsExceptionText);
                            commands.Truncate();
                            break;
                        case "Print":
                            if (commandText.Length != 2) throw new ArgumentException(paramNotFoundExceptionText);
                            if (commandText[1] == "*") commands.Print();
                            else commands.Print(commandText[1]);
                            break;
                        case "Help":
                            if (commandText.Length > 2) throw new ArgumentException(paramNotFoundExceptionText);
                            if (commandText.Length == 1) commands.Help();
                            else if (commandText.Length == 2) commands.Help(commandText[1]);

                            break;
                        case "Exit":
                            if (commandText.Length != 1) throw new ArgumentException(paramNotExistsExceptionText);
                            commands.Exit();
                            return;
                        default:
                            throw new ArgumentException(commandNotFoundExceptionText);
                            
                    }
                }
                catch(Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ошибка: " + e.Message);
                    Console.ResetColor();
                }
            }
        }
    }

    /// <summary>
    /// Команды для консоли
    /// </summary>
    public class ConsoleCommands
    {
        private FileStream? fileStream;
        private IFile? currentFile;

        private Type CheckFileType(string filename)
        {
            if (filename.EndsWith(".prd"))
                return typeof(ProductsListFile);
            else if (filename.EndsWith(".prs"))
                return typeof(SpecificationsListFile);
            else
                throw new ArgumentException("Имя файла указано не верно!");
        }
        private void CreateProductsListFile(string filename)
        {
            ProductFileNote fileNote = new ProductFileNote(filename.Replace(".prd", ".prs").ToCharArray());
            ProductsListFile file = new ProductsListFile(fileNote);
            using var writer = new BinaryWriter(File.Create(filename));
            writer.Write(file.FileNote.Length);
            //writer.Write(file.FileNote.FirstProductNotePtr.)
            
        }
        private void CreateSpecificationsFile(string filename)
        {
            throw new NotImplementedException();
        }
        private void OpenProductsListFile(string filename)
        {
            throw new NotImplementedException();
        }
        private void OpenSpecificationsFile(string filename)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Если файл существует и сигнатура соответствует заданию, команда требует
        /// подтверждения на перезапись файла. При положительном ответе, файлы очищаются, после
        /// чего создаются все необходимые структуры в памяти и файлах на диске. После успешного
        /// выполнения команды файлы считаются открытыми для работы. Если сигнатура файла
        /// отсутствует или не соответствует заданию, команда вызывает ошибку.
        /// </summary>
        /// <param name="filename">Имя файла</param>
        public void Create(string filename)
        {
            if(!File.Exists(filename))
            {
                var fileType = CheckFileType(filename);
                if (fileType == typeof(ProductsListFile))
                    CreateProductsListFile(filename);
                else if (fileType == typeof(SpecificationsListFile))
                    CreateSpecificationsFile(filename);

            }
            else
            {

            }
            //var fileType = CheckFileType(filename);

            //if (fileType == typeof(ProductsListFile))
            //    CreateProductsListFile(filename);
            //else if(fileType == typeof(SpecificationsFile))
            //    CreateSpecificationsFile(filename);

            throw new NotImplementedException();
        }
        /// <summary>
        /// Команда логически удаляет запись с именем компонента из списка,
        /// устанавливая бит удаления в -1. Если на компонент имеются ссылки в спецификациях
        /// других компонент, эта команда вызывает ошибку.
        /// </summary>
        /// <param name="companentName">Имя компонента</param>
        public void Delete(string companentName)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Команда логически удаляет комплектующее из спецификации компонента, устанавливая бит удаления в -1.
        /// Для детали эта команда вызывает ошибку.
        /// </summary>
        /// <param name="companentName">Имя компонента</param>
        /// <param name="accessoriesName">Имя комплектующего</param>
        public void Delete(string companentName, string accessoriesName)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Команда закрывает все файлы и завершает программу. Файлы при завершении
        /// программы не уничтожаются. Они уничтожаются вручную после просмотра дампа файлов
        /// при защите.
        /// </summary>
        public void Exit()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Команда выводит на экран или в указанный файл список команд.
        /// </summary>
        /// <param name="filename">Имя файла</param>
        public void Help(string? filename = null)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Команда включает компонент в список.
        /// </summary>
        /// <param name="companentName">Имя компонента</param>
        /// <param name="type">Тип компанента</param>
        public void Input(string companentName, ComponentType type)
        {

            throw new NotImplementedException();
        }
        /// <summary>
        /// Команда включает комплектующее в
        /// спецификацию компонента. Имя комплектующего должно быть в списке, в противном
        /// случае и для детали эта команда вызывает ошибку.
        /// </summary>
        /// <param name="companentName">Имя компонента</param>
        /// <param name="detailName">Имя комплектующего</param>
        public void Input(string companentName, string detailName)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Команда снимает бит удаления (присваивает значение 0) со всех
        /// записей, относящихся к заданному компоненту и ранее помеченных на удаление, а также
        /// восстанавливает алфавитный порядок, который мог быть нарушен из-за добавления новых записей.
        /// </summary>
        /// <param name="companentName">Имя компонента</param>
        public void Restore(string companentName)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Команда открывает указанный файл и связанные с ним файлы в режиме rw,
        /// создает все необходимые структуры в памяти. Если сигнатура файла отсутствует или не
        /// соответствует заданию, команда вызывает ошибку.
        /// </summary>
        /// <param name="filename">Имя файла</param>
        public void Open(string filename)
        {
            if(!File.Exists(filename))
                throw new FileNotFoundException("Файл не найден!");

            var fileType = CheckFileType(filename);
            //fileStream = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);

            if (fileType == typeof(ProductsListFile))
                OpenProductsListFile(filename);
            else if (fileType == typeof(SpecificationsListFile))
                OpenSpecificationsFile(filename);
            else
                throw new Exception("Тип файла не определен!");
        }
        /// <summary>
        /// Команда выводит на экран состав компонента (спецификацию) (для детали эта команда вызывает ошибку):
        /// </summary>
        /// <param name="componentName">Имя компонента</param>
        public void Print(string componentName)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Команда выводит на экран построчно список компонентов.
        /// </summary>
        public void Print()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Команда снимает бит удаления (присваивает значение 0) со всех записей, ранее
        /// помеченных на удаление, и восстанавливает алфавитный порядок, который мог быть
        /// нарушен из-за добавления новых записей.
        /// </summary>
        public void Restore()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Команда физически удаляет из списков записи, бит удаления которых установлен в
        /// -1, и перераспределяет записи списков таким образом, что все они становятся смежными, а
        /// свободная область располагается в конце файлов.Корректирует указатель на свободную
        /// область файла;
        /// </summary>
        public void Truncate()
        {
            throw new NotImplementedException();
        }
    }
}
