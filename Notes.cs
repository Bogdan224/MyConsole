using System.ComponentModel.DataAnnotations;

namespace MyConsole2
{
    public class ProductFileNote
    {
        /// <summary>
        /// Длина записи данных
        /// </summary>
        public byte[] Length { get; set; }

        /// <summary>
        /// Указатель на логически первую запись списка изделий
        /// </summary>
        public ProductNote? FirstProductNotePtr { get; set; }

        /// <summary>
        /// Указатель на свободную область файла
        /// </summary>
        public byte[] FreeAreaPtr {  get; set; }

        /// <summary>
        /// Имя файла спецификаций
        /// </summary>
        [MaxLength(16)]
        public char[] SpecificationFileName { get; set; }

        public ProductFileNote(char[] nameSpecificationFile)
        {
            if (nameSpecificationFile.Length > 16)
                throw new ArgumentOutOfRangeException("Количество знаков в названии файла не может быть больше 16!");

            Length = new byte[2];
            FreeAreaPtr = new byte[4];
            SpecificationFileName = nameSpecificationFile;
        }
    }

    public class SpecificationFileNote
    {
        /// <summary>
        /// Указатель на логически первую запись списка
        /// </summary>
        public SpecificationNote? FirstNotePtr { get; set; }

        /// <summary>
        /// Указатель на свободную область файла
        /// </summary>
        public byte[] FreeAreaPtr { get; set; }

        public SpecificationFileNote()
        {
            FreeAreaPtr = new byte[4];
        }
    }
    
    public class ProductNote
    {
        /// <summary>
        /// Бит удаления (может иметь значение false (запись активна) или true (запись помечена на удаление).
        /// </summary>
        public bool DeletionBit { get; set; }

        /// <summary>
        /// Указатель на запись файла спецификаций, содержащую данные о первом компоненте данного изделия или узла.
        /// Для деталей этот указатель пустой.
        /// </summary>
        public SpecificationFileNote? SpecificationNotePtr { get; set; }

        /// <summary>
        /// Указатель на следующую запись списка изделий
        /// </summary>
        public ProductNote? NextProductNotePtr { get; set; }

        /// <summary>
        /// Область данных
        /// </summary>
        public byte[] DataArea { get; set; }

        public ProductNote(byte[] dataArea)
        {
            DeletionBit = false;
            DataArea = dataArea;
        }
    }

    public class SpecificationNote
    {
        /// <summary>
        /// Бит удаления (может иметь значение false (запись активна) или true (запись помечена на удаление).
        /// </summary>
        public bool DeletionBit { get; set; }

        /// <summary>
        /// Указатель на запись файла списка изделий, содержащую наименование компонента спецификации
        /// </summary>
        public ProductFileNote? ProductFileNotePtr { get; set; }

        /// <summary>
        /// Кратность вхождения
        /// </summary>
        public byte[] Multiplicity { get; set; }

        /// <summary>
        /// Указатель на следующую запись списка-спецификации
        /// </summary>
        public SpecificationNote? NextSpecificationNotePtr { get; set; }

        public SpecificationNote()
        {
            DeletionBit = false;
            Multiplicity = new byte[2];
        }
    }
}
