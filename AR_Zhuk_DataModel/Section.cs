using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AR_Zhuk_DataModel
{
    public class Section
    {
        /// <summary>
        /// Стартовая ячейка секции - левый верхний угол
        /// Для построения картинки
        /// </summary>
        public Cell ImageStart { get; set; }
        /// <summary>
        /// Угол поворота секции - по часовой стрелке.
        /// 0 - в стандартном положении секции.
        /// </summary>
        public int ImageAngle { get; set; }
        /// <summary>
        /// Это первая секция в доме
        /// </summary>
        public bool IsStartSectionInHouse { get; set; }
        /// <summary>
        /// Это последняя секция в доме
        /// </summary>
        public bool IsEndSectionInHouse { get; set; }
        /// <summary>
        /// Направление движения:
        /// 1 - вниз или вправо
        /// -1 - вверх или влево
        /// </summary>
        public int Direction { get; set; }
        /// <summary>
        /// Тип секции - Рядовая, Угловая левая, Угловая правая, Башня
        /// </summary>
        public SectionType SectionType { get; set; }
        /// <summary>
        /// Вид левого торца секции - Обычный, Торец или Деф.шов.
        /// Левый - от стандартного положения секции (ЛЛУ горизонтально сверху).
        /// </summary>
        public Joint JointLeft { get; set; }
        /// <summary>
        /// Правый торец секции - Обычный, Торец или Деф.шов.
        /// </summary>
        public Joint JointRight { get; set; }
        /// <summary>
        /// Инсоляция сверху - справа-налево
        /// Для боковой секции - от хвостового угла до 1 ячейки над ллу.
        /// </summary>
        public List<Module> InsTop { get; set; }
        /// <summary>
        /// Инсоляция снизу - слева-направо        
        /// Для угловой секции - от 1 углового углового шага (загиб) к хвосту, включая боковые ячейки
        /// </summary>
        public List<Module> InsBot { get; set; }
        /// <summary>
        /// боковая инсоляция стартового торца секции
        /// </summary>
        public List<Module> InsSideStart { get; set; }
        /// <summary>
        /// боковая инсоляция в конце секции
        /// </summary>
        public List<Module> InsSideEnd { get; set; }
        public int IdSection { get; set; }
        public int CountStep { get; set; }        
        public bool IsVertical { get; set; }
        public bool IsCorner { get; set; }
        /// <summary>
        /// Угловая секция начинается с хвоста (длинной части угловой секциии)
        /// </summary>
        public bool IsCornerStartTail { get; set; }
        public string SpotOwner { get; set; }
        public int NumberInSpot { get; set; }
        public int CountModules { get; set; }
        public int Floors { get; set; }
        public string Code { get; set; }
        public bool IsLeftBottomCorner { get; set; }
        public bool IsRightBottomCorner { get; set; }
        public double AxisArea { get; set; }
        public double TotalArea { get; set; }
        public List<FlatInfo> Sections = new List<FlatInfo>();
        public int TotalIndex = 0;
        public double RealIndex = 0;

        public Section Copy ()
        {
            var copySection = (Section)MemberwiseClone();
            return copySection;
        }
    }
}