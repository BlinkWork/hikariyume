"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chartHub").build();

$(function () {
	connection.start().then(function () {
		alert('Connected to chartHub');
		InvokeOrders();
	}).catch(function (err) {
		return console.error(err.toString());
	});
});

// Orders

function InvokeOrders() {
	connection.invoke("SendOrders").catch(function (err) {
		console.error('InvokeOrders error:', err.toString());
	});
}


connection.on("ReceivedOrders", function (orders) {
	if (orders.length > 0) {
		BindOrdersToGrid(orders); 
	} else {
		console.log("No orders received.");
	}
});
function BindOrdersToGrid(orders) {
	console.log("Binding orders: ", orders);

	$('#tblOrders tbody').empty();
	var tr;
	$.each(orders, function (index, order) {
		tr = $('<tr/>');
		tr.append(`<td>${(index + 1)}</td>`);
		tr.append(`<td>${order.totalPrice}</td>`);
		tr.append(`<td>${order.status}</td>`);
		tr.append(`<td>${new Date(order.createdAt).toLocaleDateString()}</td>`);
		$('#tblOrders tbody').append(tr);
	});
}
connection.on("ReceivedOrdersForGraph", function (ordersForGraph) {
    console.log("Orders for Graph:", ordersForGraph);  
    BindOrdersToGraph(ordersForGraph);
});


function BindOrdersToGraph(ordersForGraph) {
	var labels = [];
	var data = [];

	// Kiểm tra xem ordersForGraph có đúng không
	console.log(ordersForGraph);

	$.each(ordersForGraph, function (index, item) {
		labels.push(new Date(item.orderTime).toLocaleDateString()); // Chuyển đổi thành ngày
		data.push(item.totalPrice);
	});

	DestroyCanvasIfExists('canvasOrders');

	const context = $('#canvasOrders');
	const myChart = new Chart(context, {
		type: 'line',
		data: {
			labels: labels,  // Nhãn là các ngày
			datasets: [{
				label: 'List of Orders',
				data: data,  // Dữ liệu là tổng giá trị đơn hàng
				backgroundColor: backgroundColors,
				borderColor: borderColors,
				borderWidth: 1
			}]
		},
		options: {
			scales: {
				y: {
					beginAtZero: true  // Bắt đầu từ 0
				}
			}
		}
	});
}
 

// supporting functions for Graphs
function DestroyCanvasIfExists(canvasId) {
	let chartStatus = Chart.getChart(canvasId); // Sử dụng đúng ID canvas
	if (chartStatus != undefined) {
		chartStatus.destroy(); // Hủy biểu đồ cũ trước khi vẽ biểu đồ mới
	}
}


var backgroundColors = [
	'rgba(255, 99, 132, 0.2)',
	'rgba(54, 162, 235, 0.2)',
	'rgba(255, 206, 86, 0.2)',
	'rgba(75, 192, 192, 0.2)',
	'rgba(153, 102, 255, 0.2)',
	'rgba(255, 159, 64, 0.2)'
];
var borderColors = [
	'rgba(255, 99, 132, 1)',
	'rgba(54, 162, 235, 1)',
	'rgba(255, 206, 86, 1)',
	'rgba(75, 192, 192, 1)',
	'rgba(153, 102, 255, 1)',
	'rgba(255, 159, 64, 1)'
];