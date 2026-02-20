namespace MyConsole2
{
    public static class ByteArrayExtensions
    {
        public static bool IsEmpty(this byte[] array)
        {
            return array == BitConverter.GetBytes(-1);
        }
    }

    /// <summary>
    /// Основной класс для управления файлами
    /// </summary>
    public class FileManager : IDisposable
    {
        private FileStream _compFile;
        private FileStream _specFile;
        private ComponentListHeader _compHeader;
        private SpecificationHeader _specHeader;
        private readonly string _compFileName;
        private readonly string _specFileName;
        private string path = @$"C:\Users\{Environment.UserName}\Downloads\";

        public FileManager(string filename, ushort recordLength = 20, string? specFilename = null)
        {
            _compFileName = filename;

            if(specFilename == null)
                _specFileName = Path.ChangeExtension(_compFileName, ".prs");
            else
                _specFileName = specFilename;

            InitializeFiles(recordLength);
        }

        private void InitializeFiles(ushort dataRecordLength)
        {
            // Инициализация файла списка изделий
            bool compExists = File.Exists(path + _compFileName);
            _compFile = new FileStream(path + _compFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (!compExists || _compFile.Length == 0)
            {
                // Создаем новый файл
                _compHeader = new ComponentListHeader(dataRecordLength, _specFileName);
                _compFile.Write(_compHeader.ToBytes(), 0, ComponentListHeader.TotalSize);
            }
            else
            {
                // Читаем существующий заголовок
                byte[] headerBuffer = new byte[ComponentListHeader.TotalSize];
                _compFile.Read(headerBuffer, 0, ComponentListHeader.TotalSize);
                _compHeader = ComponentListHeader.FromBytes(headerBuffer);
            }

            // Инициализация файла спецификаций
            bool specExists = File.Exists(path + _specFileName);
            _specFile = new FileStream(path + _specFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            if (!specExists || _specFile.Length == 0)
            {
                // Создаем новый файл
                _specHeader = new SpecificationHeader();
                _specFile.Write(_specHeader.ToBytes(), 0, SpecificationHeader.TotalSize);
            }
            else
            {
                // Читаем существующий заголовок
                byte[] headerBuffer = new byte[SpecificationHeader.TotalSize];
                _specFile.Read(headerBuffer, 0, SpecificationHeader.TotalSize);
                _specHeader = SpecificationHeader.FromBytes(headerBuffer);
            }
            //if(compExists && specExists)
            //    RestorePtrs();
        }

        private void RestorePtrs()
        {
            throw new NotImplementedException();
        }

        private ComponentListRecord? FindMyComponentInFileByName(string name)
        {
            if (_compHeader.FirstRecord != null)
            {
                ComponentListRecord? tmp = _compHeader.FirstRecord;
                while (tmp != null)
                {
                    if (tmp.DataArea.ComponentName == name)
                        return tmp;
                    tmp = tmp.NextRecord;
                }
            }
            return null;
        }

        /// <summary>
        /// Добавление компонента в список изделий
        /// </summary>
        public void AddComponentToComponentList(MyComponent component)
        {
            if (FindMyComponentInFileByName(component.ComponentName) != null)
                throw new ArgumentException("Компонент c таким именем уже существует!");

            SpecificationRecord? tmpSpecRec = null;
            //Если лист спецификаций пустой, добавляем в него пустую спецификацию и записываем ссылку на нее в запись компонента
            if (_specHeader.FirstRecord == null)
            {
                _specHeader.FirstRecord = new SpecificationRecord();
                tmpSpecRec = _specHeader.FirstRecord;
            }
            //Иначе ищем пустую спецификацию в листе спецификаций
            else
            {
                SpecificationRecord? tmp = _specHeader.FirstRecord;
                while (tmp.NextRecord != null)
                {
                    tmp = tmp.NextRecord;
                }
                tmp.NextRecord = new SpecificationRecord();
                tmpSpecRec = tmp.NextRecord;
            }

            var record = new ComponentListRecord(component)
            {
                SpecificationRecord = tmpSpecRec,
                SpecificationRecordPtr = BitConverter.GetBytes(tmpSpecRec.GetHashCode())
            };

            //Добавляем компонент в список компонентов
            if (_compHeader.FirstRecord != null)
            {
                ComponentListRecord tmp = _compHeader.FirstRecord;
                while (tmp.NextRecord != null)
                {
                    tmp = tmp.NextRecord;
                }
                tmp.NextRecord = record;
                tmp.NextRecordPtr = BitConverter.GetBytes(record.GetHashCode());
            }
            else
            {
                _compHeader.FirstRecord = record;
                _compHeader.FirstRecordPtr = BitConverter.GetBytes(record.GetHashCode());
            }

            var tmpComp = _compHeader.ToBytes();
            var tmpSpec = _specHeader.ToBytes();

            _compFile.Write(tmpComp, 0, tmpComp.Length);
            _specFile.Write(tmpSpec, 0, tmpSpec.Length);
        }

        /// <summary>
        /// Добавление компонента в спецификацию
        /// </summary>
        public void AddComponentToSpecification(string component, string componentAdded)
        {
            var compRec = FindMyComponentInFileByName(component);
            if (compRec == null)
                throw new ArgumentException("Комплектующее не найдено!");

            if (compRec.SpecificationRecord?.Components.Count == compRec.SpecificationRecord?.Quantity)
                throw new Exception("Достигнут лимит компонентов в спецификации!");

            var tmpComp = FindMyComponentInFileByName(componentAdded);
            if (tmpComp == null)
                throw new Exception("Невозможно добавить не существующий компонент!");
            compRec.SpecificationRecord?.Components.Add(tmpComp.DataArea);

            var tmpCompArray = _compHeader.ToBytes();
            var tmpSpec = _specHeader.ToBytes();

            _compFile.Write(tmpCompArray, 0, tmpCompArray.Length);
            _specFile.Write(tmpSpec, 0, tmpSpec.Length);
        }

        public void Test()
        {
            MyComponent myComponent = new("Изделие1", ComponentType.Product);
            MyComponent myComponent1 = new("Узел1", ComponentType.Node);
            MyComponent myComponent2 = new("Узел2", ComponentType.Node);
            MyComponent myComponent3 = new("Деталь1", ComponentType.Detail);
            MyComponent myComponent4 = new("Деталь2", ComponentType.Detail);
            AddComponentToComponentList(myComponent);
            AddComponentToComponentList(myComponent1);
            AddComponentToComponentList(myComponent2);
            AddComponentToComponentList(myComponent3);
            AddComponentToComponentList(myComponent4);
            AddComponentToSpecification(myComponent.ComponentName, myComponent1.ComponentName);
            AddComponentToSpecification(myComponent.ComponentName, myComponent3.ComponentName);
            AddComponentToSpecification(myComponent1.ComponentName, myComponent2.ComponentName);
            AddComponentToSpecification(myComponent1.ComponentName, myComponent4.ComponentName);
        }

        ///// <summary>
        ///// Удаление записи из списка изделий (логическое удаление)
        ///// </summary>
        //public void DeletePartsListRecord(int position)
        //{
        //    _compFile.Seek(position, SeekOrigin.Begin);
        //    _compFile.WriteByte(0xFF); // Устанавливаем бит удаления в -1 (0xFF)
        //}

        ///// <summary>
        ///// Удаление записи из спецификации (логическое удаление)
        ///// </summary>
        //public void DeleteSpecificationRecord(int position)
        //{
        //    _specFile.Seek(position, SeekOrigin.Begin);
        //    _specFile.WriteByte(0xFF); // Устанавливаем бит удаления в -1 (0xFF)
        //}

        ///// <summary>
        ///// Вывод всей структуры на экран
        ///// </summary>
        //public void DisplayStructure()
        //{
        //    Console.WriteLine("=== СТРУКТУРА ДАННЫХ ===\n");

        //    Console.WriteLine("Файл списка изделий:");
        //    Console.WriteLine($"Длина области данных: {_compHeader.DataRecordLength} байт");
        //    Console.WriteLine($"Первый указатель: {_compHeader.FirstRecordPtr}");
        //    Console.WriteLine($"Указатель на свободную область: {_compHeader.FreeAreaPtr}");
        //    Console.WriteLine($"Имя файла спецификаций: {_compHeader.SpecFileName}\n");

        //    // Вывод всех записей списка изделий
        //    int current = _compHeader.FirstRecordPtr;
        //    int index = 1;

        //    while (current != -1)
        //    {
        //        byte[] buffer = new byte[ComponentListRecord.DeletionBitSize +
        //                                ComponentListRecord.SpecificationRecordPtrSize +
        //                                ComponentListRecord.NextRecordPtrSize +
        //                                _compHeader.DataRecordLength];

        //        _compFile.Seek(current, SeekOrigin.Begin);
        //        _compFile.Read(buffer, 0, buffer.Length);

        //        var record = ComponentListRecord.FromBytes(buffer, 0, _compHeader.DataRecordLength);

        //        Console.WriteLine($"Запись {index} (смещение {current}):");
        //        Console.WriteLine($"  Удалена: {(record.IsDeleted == -1 ? "Да" : "Нет")}");
        //        Console.WriteLine($"  Указатель на спецификацию: {record.SpecificationRecordPtr}");
        //        Console.WriteLine($"  Следующая запись: {record.NextRecordPtr}");
        //        Console.WriteLine($"  Наименование: {record.Name}");

        //        // Если есть спецификация, выводим её
        //        if (record.SpecificationRecordPtr != -1 && record.IsDeleted == 0)
        //        {
        //            DisplaySpecification(record.SpecificationRecordPtr, 2);
        //        }

        //        Console.WriteLine();
        //        current = record.NextRecordPtr;
        //        index++;
        //    }

        //    Console.WriteLine("========================\n");
        //}

        //private void DisplaySpecification(int specPtr, int indentLevel)
        //{
        //    string indent = new string(' ', indentLevel * 2);
        //    int current = specPtr;

        //    while (current != -1)
        //    {
        //        byte[] buffer = new byte[SpecificationRecord.TotalSize];
        //        _specFile.Seek(current, SeekOrigin.Begin);
        //        _specFile.Read(buffer, 0, SpecificationRecord.TotalSize);

        //        var record = SpecificationRecord.FromBytes(buffer);

        //        if (record.IsDeleted == 0)
        //        {
        //            // Получаем имя компонента из списка изделий
        //            string componentName = "???";
        //            if (record.ComponentRecordPtr != -1)
        //            {
        //                byte[] nameBuffer = new byte[_compHeader.DataRecordLength];
        //                _compFile.Seek(record.ComponentRecordPtr + ComponentListRecord.DeletionBitSize +
        //                               ComponentListRecord.SpecificationRecordPtrSize +
        //                               ComponentListRecord.NextRecordPtrSize, SeekOrigin.Begin);
        //                _compFile.Read(nameBuffer, 0, _compHeader.DataRecordLength);
        //                componentName = Encoding.ASCII.GetString(nameBuffer).TrimEnd();
        //            }

        //            Console.WriteLine($"{indent}Спецификация (смещение {current}):");
        //            Console.WriteLine($"{indent}  Компонент: {componentName} (указ. {record.ComponentRecordPtr})");
        //            Console.WriteLine($"{indent}  Количество: {record.Quantity}");
        //            Console.WriteLine($"{indent}  Следующая: {record.NextRecordPtr}");
        //        }

        //        current = record.NextRecordPtr;
        //    }
        //}

        //private void UpdatePartsHeader()
        //{
        //    _compFile.Seek(0, SeekOrigin.Begin);
        //    _compFile.Write(_compHeader.ToBytes(), 0, ComponentListHeader.TotalSize);
        //}

        //private void UpdateSpecHeader()
        //{
        //    _specFile.Seek(0, SeekOrigin.Begin);
        //    _specFile.Write(_specHeader.ToBytes(), 0, SpecificationHeader.TotalSize);
        //}

        public void Dispose()
        {
            //UpdatePartsHeader();
            //UpdateSpecHeader();

            _compFile?.Dispose();
            _specFile?.Dispose();
        }
    }
}
