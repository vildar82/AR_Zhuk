using System;
using AR_Zhuk_DataModel;
using AR_Zhuk_Schema.DB;
using AR_Zhuk_Schema.Insolation;

namespace AR_Zhuk_Schema.Scheme.Cutting
{
    public static class CuttingFactory
    {
        static IInsolation insService;
        static IDBService dbService;

        public static void ResetData()
        {
            insService = null;
            dbService = null;
        }

        public static ICutting Create (HouseSpot houseSpot, SpotInfo sp, int maxSectionbySize, int maxHousesBySpot)
        {
            ICutting cutting;
            if (insService == null)
                insService = new InsolationSection(sp);
            if (dbService == null)
                dbService = new DBService(sp, maxSectionbySize);

            if (houseSpot.IsTower)
            {
                cutting = null;                    
            }
            else
            {
                cutting = new CuttingOrdinary(houseSpot, dbService, insService, sp, maxHousesBySpot);
            }
            return cutting;
        }
    }
}