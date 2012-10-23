using System.Data;

namespace Solid
{
    //Пусть у нас есть 3 класса OracleDbFacade, Сheque, BizService

    /// <summary>
    /// Класс для доступа к БД, обращение к которому идет через OracleDbFacade.Instance 
    /// </summary>
    public class OracleDbFacade
    {
        public static OracleDbFacade Instance = new OracleDbFacade();

        public DataTable ExecuteQuery(string text, params object[] pars)
        {
            return null;
        }

        public int ExecuteNonQuery(string text, params object[] pars)
        {
            return 0;
        }
    }

    /// <summary>
    /// Класс кассовый чек.
    /// </summary>
    public class Cheque
    {
        private long id;

        private int number;
               
        /// <summary>
        /// Наличие подобного метода в классе домена нарушает сразу 2 принципа
        /// Single responsibility principle - из-за наличия в классе домена данного метода вообще
        /// Open/closed principle - из-за наличия жесткой связи с OracleDbFacade, в случае необходимости замены на SQLiteDbFacade придется менять код.
        /// Dependency inversion principle - из-за наличия жесткой связи с OracleDbFacade, поэтому этот код невозможно протестировать без Oracle. 
        /// </summary>
        public void Save()
        {
            if (id == 0)
            {
                OracleDbFacade.Instance.ExecuteNonQuery("INSERT ...");
            }
            else
            {
                OracleDbFacade.Instance.ExecuteNonQuery("UPDATE ...");
            }
        }

        public long Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public int Number
        {
            get
            {
                return number;
            }
            set
            {
                number = value;
            }
        }
    }

    /// <summary>
    /// Класс бизнес сервиса, который реализует бизнес логику с использованием объектов домена, который невозможно протестировать без Oracle
    /// </summary>
    public class BizService
    {
        public Cheque Create()
        {
            Cheque cheque = new Cheque();
            cheque.Save();
            return cheque;
        }
    }

    //Исправляем. Убираем метод Save из класса Cheque и вводим понятие ChequeRepository для работы с БД.

    /// <summary>
    /// Класс кассовый чек, без метода Save
    /// </summary>
    public class ChequeNew
    {
        private long id;

        private int number;

        public long Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public int Number
        {
            get
            {
                return number;
            }
            set
            {
                number = value;
            }
        }
    }

    /// <summary>
    /// Интерфейс для репозитория чеков
    /// </summary>
    public interface IChequeRepository
    {
        void Save(ChequeNew cheque);
    }

    /// <summary>
    /// Класс репозиторий чеков, который занимается их сохранением
    /// </summary>
    public class OracleChequeRepository : IChequeRepository
    {
        public void Save(ChequeNew cheque)
        {
            if (cheque.Id == 0)
            {
                OracleDbFacade.Instance.ExecuteNonQuery("INSERT ...");
            }
            else
            {
                OracleDbFacade.Instance.ExecuteNonQuery("UPDATE ...");
            }
        }
    }

    /// <summary>
    /// Новый класс бизнес сервиса, который реализует бизнес логику с использованием объектов домена, который легко тестируется с использованием мок фреймворков,
    /// для которого можно поменять сохранение в Oracle на сохранение в SQLite просто реализовав SQLiteChequeRepository.
    /// </summary>
    public class BizServiceNew
    {
        private IChequeRepository chequeRepo;

        public BizServiceNew(IChequeRepository chequeRepo)
        {
            this.chequeRepo = chequeRepo;
        }

        public ChequeNew Create()
        {
            ChequeNew cheque = new ChequeNew();
            chequeRepo.Save(cheque);
            return cheque;
        }
    }
}
