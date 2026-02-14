using System.ComponentModel.DataAnnotations;

namespace MyConsole2
{
    public class ProductFileNote
    {
        //private char[] ps;

        /// <summary>
        /// Длина записи данных
        /// </summary>
        public byte[] Length { get; set; }

        /// <summary>
        /// Указатель на логически первую запись списка изделий
        /// </summary>
        public byte[] FirstProductNotePtr { get; set; }

        /// <summary>
        /// Указатель на свободную область файла;
        /// </summary>
        public byte[] FreeAreaPtr {  get; set; }

        /// <summary>
        /// Имя файла спецификаций
        /// </summary>
        [MaxLength(16)]
        public char[] NameSpecificationFile { get; set; }

        public ProductFileNote(char[] nameSpecificationFile)
        {
            //ps = "PS".ToCharArray();
            Length = new byte[2];
                
            FirstProductNotePtr = new byte[4];
            FreeAreaPtr = new byte[4];
            NameSpecificationFile = nameSpecificationFile;
        }
    }

    public class SpecificationFileNote
    {
        /// <summary>
        /// Указатель на логически первую запись списка
        /// </summary>
        public byte[] FirstNotePtr { get; set; }

        /// <summary>
        /// Указатель на свободную область файла
        /// </summary>
        public byte[] FreeAreaPtr { get; set; }

        public SpecificationFileNote()
        { 
            FirstNotePtr = new byte[4];
            FreeAreaPtr = new byte[4];
        }
    }
    
    public class ProductNote
    {
        /// <summary>
        /// Бит удаления
        /// </summary>
        public bool DeletionBit { get; set; }

        /// <summary>
        /// Указатель на запись файла спецификаций, содержащую данные о первом компоненте данного изделия или узла
        /// </summary>
        public byte[] SpecificationNotePtr { get; set; }

        /// <summary>
        /// Указатель на запись файла спецификаций, содержащую данные о первом компоненте данного изделия или узла
        /// </summary>
        public byte[] NextProductNotePtr { get; set; }

        /// <summary>
        /// Область данных
        /// </summary>
        public char[] DataArea { get; set; }

        public ProductNote(char[] dataArea)
        {
            DeletionBit = false;
            SpecificationNotePtr = new byte[4];
            NextProductNotePtr = new byte[4];
            DataArea = dataArea;
        }
    }
}
