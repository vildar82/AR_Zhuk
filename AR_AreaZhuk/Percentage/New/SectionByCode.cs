﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_DataModel;

namespace AR_AreaZhuk.Percentage.New
{
    class SectionByCode
    {        
        public List<FlatInfo> Sections { get; private set; }        
        public int IndexReq { get; private set; } 
        public int CountCodeIndexFlat { get; private set; }        
        /// <summary>
        /// Секции по индексу требования
        /// </summary>
        public List<SectionByCode> SectionsByCodeNextReq { get; private set; }

        public SectionByCode (int indexReq, int countCodeIndexFlat, List<FlatInfo> sections)
        {
            IndexReq = indexReq;
            CountCodeIndexFlat = countCodeIndexFlat;
            this.Sections = sections;            
            // Квартиры след индекса
            SectionsByCodeNextReq = GetSections(sections, indexReq + 1);
        }

        public static List<SectionByCode> GetSections(List<FlatInfo> sections, int indexReq)
        {
            if (indexReq == sections[0].CodeArray.Length)
            {
                return null;
            }
            // группировка сецйи по первому требованию (кол квартир в первой индексе кода секций)
            List<SectionByCode> secsByCode = sections.GroupBy(g => g.CodeArray[indexReq]).OrderBy(o=>o.Key).
                Select(v => new SectionByCode(indexReq, v.Key, v.ToList())).ToList();
            return secsByCode;
        }

        public List<int> GetCountFlatsByReq (int indexReq)
        {
            if (this.IndexReq == indexReq)
            {
                return new List<int>() { CountCodeIndexFlat };
            }
            else
            {
                return SectionsByCodeNextReq.SelectMany(s => s.GetCountFlatsByReq(indexReq)).ToList();
            }
        }
    }
}
