namespace VueFormsApi.DataStructures.MallStructures
{
    public class Mall : IMall
    {
        private List<Store> stores = new List<Store>();

        public List<Store> GetStores()
        {
            return stores;
        }
    }
}
