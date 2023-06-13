using _06_MvcWeb.Models;
using System.Collections.Generic;

namespace _06_MvcWeb.Services
{
    public class PlanetService:List<PlanetModel>
    {
        public PlanetService()
        {
            Add(new PlanetModel {Id=1,Name="Planet1",VnName= "Hành tinh 1", Content= "Hành tinh 1" });
            Add(new PlanetModel {Id=2,Name= "Planet2", VnName= "Hành tinh 2", Content= "Hành tinh 2" });
            Add(new PlanetModel {Id=3,Name="Planet3",VnName= "Hành tinh 3", Content= "Hành tinh 3" });
        }
    }
}
