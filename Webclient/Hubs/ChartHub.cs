using Microsoft.AspNetCore.SignalR;
using Webclient.Repositories;

namespace Webclient.Hubs
{
    public class ChartHub : Hub
    {
        OrderRepository orderRepository;
        public ChartHub(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MyCnn");
            orderRepository = new OrderRepository(connectionString); 
        }
        public async Task SendOrders()
        {
            try
            {
                var orders = orderRepository.GetOrders();
                await Clients.All.SendAsync("ReceivedOrders", orders);

                var ordersForGraph = orderRepository.GetOrdersForGraph();
                await Clients.All.SendAsync("ReceivedOrdersForGraph", ordersForGraph);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SendOrders: " + ex.Message); 
                throw; 
            }
        }



    }
}
