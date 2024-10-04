using TableDependency.SqlClient;
using Webclient.Hubs;
using Webclient.Models;

namespace Webclient.SubscribeTableDependencies
{
    public class SubscribeOrderTableDependency
    {
        SqlTableDependency<Order> tableDependency;
        ChartHub chartHub;
        public SubscribeOrderTableDependency(ChartHub chartHub)
        {
            this.chartHub = chartHub;
        }
        public void SubscribeTableDependency()
        {
            //string connectionString = "server=DESKTOP-5TITJRQ\\MSSQLSERVER01;database=HikariYume;user=sa;password=123;TrustServerCertificate=true";
            string connectionString = "server =(local); database = HikariYume;uid=sa;pwd=sa;";
            tableDependency = new SqlTableDependency<Order>(connectionString, "Orders");
            tableDependency.OnChanged += TableDependency_OnChanged;
            tableDependency.OnError += TableDependency_OnError;
            tableDependency.Start();
        }
        private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Console.WriteLine($"{nameof(Product)} SqlTableDependency error: {e.Error.Message}");
        }


        private void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<Order> e)
        {
            if (e.ChangeType != TableDependency.SqlClient.Base.Enums.ChangeType.None)
            {
                chartHub.SendOrders();
            }
        }
    }
}
