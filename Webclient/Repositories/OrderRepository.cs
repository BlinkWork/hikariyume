using System.Data;
using System.Data.SqlClient;
using Webclient.Models;

namespace Webclient.Repositories
{

    public class OrderRepository
    {
        string connectionString;

        public OrderRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<Order> GetOrders()
        {
            List<Order> orders = new List<Order>();
            Order order;

            var data = GetOrderDetailsFromDb();

            Console.WriteLine("Total rows retrieved: " + data.Rows.Count);

            foreach (DataRow row in data.Rows)
            {
                order = new Order
                {
                    OrderId = Convert.ToInt32(row["order_id"]),
                    TotalPrice = Convert.ToDecimal(row["total_price"]),
                    Status = row["status"].ToString(),
                    CreatedAt = Convert.ToDateTime(row["created_at"])
                };

                Console.WriteLine($"OrderId: {order.OrderId}, TotalPrice: {order.TotalPrice}, Status: {order.Status}, CreatedAt: {order.CreatedAt}"); // Log chi tiết mỗi Order
                orders.Add(order);
            }

            return orders;
        }
        private DataTable GetOrderDetailsFromDb()
        {
            var query = "SELECT order_id, total_price, status, created_at FROM Orders";
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                    }

                    return dataTable;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        public List<OrderForGraph> GetOrdersForGraph()
        {
            List<OrderForGraph> ordersForGraph = new List<OrderForGraph>();
            OrderForGraph orderForGraph;

            var data = GetOrdersForGraphFromDb();
            foreach (DataRow row in data.Rows)
            {
                orderForGraph = new OrderForGraph
                {
                    TotalPrice = Convert.ToDecimal(row["total_price"]),
                    OrderTime = Convert.ToDateTime(row["created_at"])
                };
                ordersForGraph.Add(orderForGraph);
            }

            return ordersForGraph;
        }


        private DataTable GetOrdersForGraphFromDb()
        {
            var query = "SELECT created_at, SUM(total_price) AS total_price FROM Orders GROUP BY created_at";
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                    }

                    return dataTable;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }


    }
}
