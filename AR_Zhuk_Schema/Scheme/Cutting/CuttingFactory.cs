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

        public static ICutting Create (HouseSpot houseSpot)
        {
            ICutting cutting;
            insService = new InsolationSection();

            if (dbService == null)
                dbService = new DBService();
            else
                dbService.ResetSections();

            if (houseSpot.IsTower)
            {
                cutting = null;                    
            }
            else
            {
                cutting = new CuttingOrdinary(houseSpot, dbService, insService);
            }
            return cutting;
        }
    }
}