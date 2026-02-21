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
        private FileStream? _compFile;
        private FileStream? _specFile;
        private ComponentListHeader _compHeader;
        private SpecificationHeader _specHeader;
        private readonly string _compFileName;
        private readonly string _specFileName;
        private string _path = @$"C:\Users\{Environment.UserName}\Downloads\";

        private List<MyComponent> components;

        public FileManager(string filename, string? specFilename = null)
        {
            components = new List<MyComponent>();
            _compFileName = filename;

            if (specFilename == null)
                _specFileName = Path.ChangeExtension(_compFileName, ".prs");
            else
                _specFileName = specFilename;
        }

        private void InitializeFiles(ushort dataRecordLength, out FileStream compFile, out FileStream specFile)
        {
            // Инициализация файла списка изделий
            bool compExists = File.Exists(_path + _compFileName);
            compFile = new FileStream(_path + _compFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            if (!compExists || compFile.Length == 0)
            {
                // Создаем новый файл
                _compHeader = new ComponentListHeader(dataRecordLength, _specFileName);
                compFile.Write(_compHeader.ToBytes(), 0, ComponentListHeader.TotalSize);
            }
            else
            {
                // Читаем существующий заголовок
                byte[] headerBuffer = new byte[ComponentListHeader.TotalSize];
                compFile.Read(headerBuffer, 0, ComponentListHeader.TotalSize);
                _compHeader = ComponentListHeader.FromBytes(headerBuffer);
            }

            // Инициализация файла спецификаций
            bool specExists = File.Exists(_path + _specFileName);
            specFile = new FileStream(_path + _specFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            if (!specExists || specFile.Length == 0)
            {
                // Создаем новый файл
                _specHeader = new SpecificationHeader();
                specFile.Write(_specHeader.ToBytes(), 0, SpecificationHeader.TotalSize);
            }
            else
            {
                // Читаем существующий заголовок
                byte[] headerBuffer = new byte[SpecificationHeader.TotalSize];
                specFile.Read(headerBuffer, 0, SpecificationHeader.TotalSize);
                _specHeader = SpecificationHeader.FromBytes(headerBuffer);
            }
        }

        public void RestoreFiles(ushort recordLength = 20)
        {
            File.Delete(_path + _compFileName);
            File.Delete(_path + _specFileName);

            CreateFiles();
        }

        public void CreateFiles(ushort recordLength = 20)
        {
            _compFile = new FileStream(_path + _compFileName, FileMode.Create, FileAccess.ReadWrite);

            _compHeader = new ComponentListHeader(recordLength, _specFileName);
            _compFile.Write(_compHeader.ToBytes(), 0, ComponentListHeader.TotalSize);

            _specFile = new FileStream(_path + _specFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            
            _specHeader = new SpecificationHeader();
            _specFile.Write(_specHeader.ToBytes(), 0, SpecificationHeader.TotalSize);
        }

        public void OpenFiles(ushort recordLength = 20)
        {
            //_compFile = new FileStream(_path + _compFileName, FileMode.Open, FileAccess.ReadWrite);
        }

        private void RestorePtrs()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Метод ищет запись с названием компонента
        /// </summary>
        /// <param name="name">Название компонента</param>
        /// <returns>Если запись с компонентом найдена, то возвращает запись, иначе null</returns>
        private ComponentListRecord? FindMyCompInCompListByName(string name)
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

        private void AddMyCompInSpecByName(string name, string nameAdded)
        {
            if (_specHeader.FirstRecord != null)
            {
                ComponentListRecord? tmp = _compHeader.FirstRecord;
                while (tmp != null)
                {
                    if (tmp.DataArea.ComponentName == name)
                    {
                        var tmpAdded = FindMyCompInCompListByName(nameAdded);
                        if (tmpAdded == null)
                            throw new Exception("Комплектующего с таким именем не существует");
                        //tmp.SpecificationRecord.Components.Add()
                    }

                    tmp = tmp.NextRecord;
                }
            }
            throw new Exception("Не удалось добавить комплектующее!");
        }

        private bool CheckOpenFiles()
        {
            return _compFile != null && _specFile != null;
        }

        /// <summary>
        /// Добавление компонента в список изделий
        /// </summary>
        public void AddComponentToComponentList(MyComponent component)
        {
            if (!CheckOpenFiles())
                throw new Exception("Файлы должны быть открыты!");

            if (FindMyCompInCompListByName(component.ComponentName) != null)
                throw new ArgumentException("Компонент c таким именем уже существует!");

            var record = new ComponentListRecord(component);

            if (component.ComponentType != ComponentType.Detail)
            {
                SpecificationRecord? tmpSpecRec;
                //Если лист спецификаций пустой, добавляем в него пустую спецификацию и записываем ссылку на нее в запись компонента
                if (_specHeader.FirstRecord == null)
                {
                    _specHeader.FirstRecord = new SpecificationRecord()
                    {
                        ComponentRecord = record,
                        ComponentRecordPtr = record.GetHashCode(),
                    };
                    _specHeader.FirstRecordPtr = _specHeader.FirstRecord.GetHashCode();
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
                    tmp.NextRecord = new SpecificationRecord()
                    {
                        ComponentRecord = record,
                        ComponentRecordPtr = record.GetHashCode(),
                    };
                    tmp.NextRecordPtr = tmp.NextRecord.GetHashCode();
                    tmpSpecRec = tmp.NextRecord;
                }

                record.SpecificationRecord = tmpSpecRec;
                record.SpecificationRecordPtr = tmpSpecRec.GetHashCode();
            }

            //Добавляем компонент в список компонентов
            if (_compHeader.FirstRecord != null)
            {
                ComponentListRecord tmp = _compHeader.FirstRecord;
                while (tmp.NextRecord != null)
                {
                    tmp = tmp.NextRecord;
                }
                tmp.NextRecord = record;
                tmp.NextRecordPtr = record.GetHashCode();
            }
            else
            {
                _compHeader.FirstRecord = record;
                _compHeader.FirstRecordPtr = record.GetHashCode();
            }

            components.Add(component);
        }

        /// <summary>
        /// Добавление компонента в спецификацию
        /// </summary>
        public void AddComponentToSpecification(string component, string componentAdded)
        {
            if (!CheckOpenFiles())
                throw new Exception("Файлы должны быть открыты!");

            var compRec = FindMyCompInCompListByName(component);
            if (compRec == null)
                throw new ArgumentException("Компонент не найден!");
            if (compRec.DataArea.ComponentType == ComponentType.Detail)
                throw new Exception("Деталь не может иметь спецификацию!");

            var tmpComp = FindMyCompInCompListByName(componentAdded);
            if (tmpComp == null)
                throw new Exception("Невозможно добавить не существующее комплектующее!");
            if (tmpComp.DataArea.ComponentType == ComponentType.Product)
                throw new Exception("Нельзя добавить изделие в спецификацию!");
            compRec.SpecificationRecord?.AddComponent(tmpComp.DataArea);
        }

        public void Test()
        {
            if (!CheckOpenFiles())
                throw new Exception("Файлы должны быть открыты!");

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

            UpdateFiles();
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

        private void UpdateCompFile()
        {
            var comp = _compHeader.ToBytes();

            _compFile.Seek(0, SeekOrigin.Begin);
            _compFile.Write(comp, 0, comp.Length);
        }

        private void UpdateSpecFile()
        {
            var spec = _specHeader.ToBytes();

            _specFile.Seek(0, SeekOrigin.Begin);
            _specFile.Write(spec, 0, spec.Length);
        }

        private void UpdateFiles()
        {
            UpdateCompFile();
            UpdateSpecFile();
        }

        public void Dispose()
        {
            UpdateFiles();

            _compFile?.Dispose();
            _specFile?.Dispose();
        }
    }
}
