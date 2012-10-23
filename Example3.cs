using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Solid
{
    //Есть интерфейс для топливораздаточной колонки. В интерфейсе есть метод для начала налива (Start)
    //и для снятия/повешения пистолета. Функционал снять/повесить пистолет в случае реальной колонки не имеет
    //смысла, так как это все осуществляется человеком, а вслучае эмулятора смысл имеет. 
    //Здесь нарушение Interface-segregation principle.

    //Интерфейс колонки
    public interface IPump
    {
        void Start(int nozzleNum, decimal amount);

        void NozzleOff(int nozzleNum);

        void NozzleOn(int nozzleNum);
    }

    //Класс реальной колонки
    public class RealPump : IPump
    {
        public void Start(int nozzleNum, decimal amount)
        {
            //Логика
        }

        public void NozzleOff(int nozzleNum)
        {
            throw new NotImplementedException("Не имеет смысла");
        }

        public void NozzleOn(int nozzleNum)
        {
            throw new NotImplementedException("Не имеет смысла");
        }
    }

    //Класс симулятора колонки
    public class SimulatorPump : IPump
    {
        public void Start(int nozzleNum, decimal amount)
        {
            //Логика
        }

        public void NozzleOff(int nozzleNum)
        {
            //Логика
        }

        public void NozzleOn(int nozzleNum)
        {
            //Логика
        }
    }

    //Исправляем, вводя 2 интерфейса

    //Интерфейс колонки
    public interface IPumpNew
    {
        void Start(int nozzleNum, decimal amount);
    }

    //Интерфейс контроллера пистолетов
    public interface INozzleController
    {
        void NozzleOff(int nozzleNum);

        void NozzleOn(int nozzleNum);
    }

    //Класс реальной колонки
    public class RealPumpNew : IPumpNew
    {
        public void Start(int nozzleNum, decimal amount)
        {
            //Логика
        }
    }

    //Класс симулятора колонки
    public class SimulatorPumpNew : IPump, INozzleController
    {
        public void Start(int nozzleNum, decimal amount)
        {
            //Логика
        }

        public void NozzleOff(int nozzleNum)
        {
            //Логика
        }

        public void NozzleOn(int nozzleNum)
        {
            //Логика
        }
    }
}
