namespace MyConsole2
{
    /// <summary>
    /// Комманды для консоли
    /// </summary>
    public static class ConsoleCommands
    {
        /// <summary>
        /// Если файл существует и сигнатура соответствует заданию, команда требует
        /// подтверждения на перезапись файла. При положительном ответе, файлы очищаются, после
        /// чего создаются все необходимые структуры в памяти и файлах на диске. После успешного
        /// выполнения команды файлы считаются открытыми для работы. Если сигнатура файла
        /// отсутствует или не соответствует заданию, команда вызывает ошибку.
        /// </summary>
        /// <param name="filename">Имя файла</param>
        public static void Create(string filename)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Команда открывает указанный файл и связанные с ним файлы в режиме rw,
        /// создает все необходимые структуры в памяти.Если сигнатура файла отсутствует или не
        /// соответствует заданию, команда вызывает ошибку.
        /// </summary>
        /// <param name="filename">Имя файла</param>
        public static void Open(string filename)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Команда включает компонент в список.
        /// </summary>
        /// <param name="companentName">Имя компонента</param>
        /// <param name="type">Тип компанента</param>
        public static void Input(string companentName, ComponentType type) 
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
        public static void Input(string companentName, string detailName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Команда логически удаляет запись с именем компонента из списка,
        /// устанавливая бит удаления в -1. Если на компонент имеются ссылки в спецификациях
        /// других компонент, эта команда вызывает ошибку.
        /// </summary>
        /// <param name="companentName">Имя компонента</param>
        public static void Delete(string companentName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Команда логически удаляет комплектующее из спецификации компонента, устанавливая бит удаления в -1.
        /// Для детали эта команда вызывает ошибку.
        /// </summary>
        /// <param name="companentName">Имя компонента</param>
        /// <param name="detailName">Имя комплектующего</param>
        public static void Delete(string companentName, string detailName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Команда снимает бит удаления (присваивает значение 0) со всех
        /// записей, относящихся к заданному компоненту и ранее помеченных на удаление, а также
        /// восстанавливает алфавитный порядок, который мог быть нарушен из-за добавления новых записей.
        /// </summary>
        /// <param name="companentName">Имя компонента</param>
        public static void Restore(string companentName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Команда снимает бит удаления (присваивает значение 0) со всех записей, ранее
        /// помеченных на удаление, и восстанавливает алфавитный порядок, который мог быть
        /// нарушен из-за добавления новых записей.
        /// </summary>
        public static void Restore()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Команда физически удаляет из списков записи, бит удаления которых установлен в
        /// -1, и перераспределяет записи списков таким образом, что все они становятся смежными, а
        /// свободная область располагается в конце файлов.Корректирует указатель на свободную
        /// область файла;
        /// </summary>
        public static void Truncate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Команда выводит на экран состав компонента (спецификацию) (для детали эта команда вызывает ошибку):
        /// </summary>
        /// <param name="componentName">Имя компонента</param>
        public static void Print(string componentName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Команда выводит на экран построчно список компонентов.
        /// </summary>
        public static void Print()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Команда выводит на экран или в указанный файл список команд.
        /// </summary>
        /// <param name="filename">Имя файла</param>
        public static void Help(string? filename = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Команда закрывает все файлы и завершает программу. Файлы при завершении
        /// программы не уничтожаются. Они уничтожаются вручную после просмотра дампа файлов
        /// при защите.
        /// </summary>
        public static void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
