IF EXISTS (SELECT name FROM sys.databases WHERE name = N'HikariYume')
Begin
USE master;
ALTER DATABASE HikariYume SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE HikariYume;
end
CREATE DATABASE HikariYume;
GO

USE HikariYume;


CREATE TABLE Users (
    user_id INT PRIMARY KEY IDENTITY(1,1),
    username VARCHAR(100) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    full_name NVARCHAR(255),
    address TEXT,
    phone_number VARCHAR(15),
    role VARCHAR(10) CHECK (role IN ('admin', 'user')) NOT NULL,
    created_at DATETIME DEFAULT GETDATE()
);

CREATE TABLE Categories (
    category_id INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(255)
);
CREATE TABLE Products (
    product_id INT PRIMARY KEY IDENTITY(1,1),
    image VARCHAR(100) NOT NULL,
    name NVARCHAR(255) NOT NULL,
    --description NVARCHAR(255),
	size NVARCHAR(255),
	origin NVARCHAR(255),
	color NVARCHAR(255),
	age NVARCHAR(255),
	material NVARCHAR(255),
    price DECIMAL(10, 2) NOT NULL,
    stock_quantity INT NOT NULL,
    category_id INT,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (category_id) REFERENCES Categories(category_id)
);

CREATE TABLE Orders (
    order_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT,
    total_price DECIMAL(10, 2) NOT NULL,
    status NVARCHAR(10) CHECK (status IN (N'Đang chờ', N'Đang giao', N'Hoàn thành', N'Bị hủy')) NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES Users(user_id)
);

CREATE TABLE Order_Items (
    order_item_id INT PRIMARY KEY IDENTITY(1,1),
    order_id INT,
    product_id INT,
    quantity INT NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (order_id) REFERENCES Orders(order_id),
    FOREIGN KEY (product_id) REFERENCES Products(product_id)
);

CREATE TABLE Reviews (
    review_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT,
    product_id INT,
    rating INT CHECK (rating BETWEEN 1 AND 5) NOT NULL,
    comment NVARCHAR(255),
    created_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    FOREIGN KEY (product_id) REFERENCES Products(product_id)
);


CREATE TABLE Payments (
    payment_id INT PRIMARY KEY IDENTITY(1,1),
    order_id INT,
    amount DECIMAL(10, 2) NOT NULL,
    payment_method VARCHAR(20) CHECK (payment_method IN ('credit_card', 'paypal', 'bank_transfer')) NOT NULL,
    payment_status VARCHAR(10) CHECK (payment_status IN ('pending', 'completed', 'failed')) NOT NULL,
    payment_date DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (order_id) REFERENCES Orders(order_id)
);

CREATE TABLE Cart (
    cart_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT,
    product_id INT,
    quantity INT NOT NULL,
    added_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    FOREIGN KEY (product_id) REFERENCES Products(product_id)
);

CREATE TABLE Wishlist (
    wishlist_id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT,
    product_id INT,
    added_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES Users(user_id),
    FOREIGN KEY (product_id) REFERENCES Products(product_id)
);

INSERT INTO Users (username, password, email, full_name, address, phone_number, role)
VALUES 
('admin', 'password1', 'admin@gom.vn', 'Admin', '123 Gốm St', '0901234567', 'admin'),
('user1', 'password2', 'user1@gom.vn', 'Nguyen Van A', '456 Gốm St', '0907654321', 'user'),
('user2', 'password3', 'user2@gom.vn', 'Tran Thi B', '789 Gốm St', '0912345678', 'user');

INSERT INTO Categories (name, description)
VALUES 
(N'Bát', N'Các loại bát gốm sứ cao cấp'),
(N'Đĩa', N'Các loại đĩa gốm sứ đa dạng'),
(N'Cốc', N'Các loại cốc đa dạng'),
(N'Bộ', N'Các bộ trang bị đa dạng');

INSERT INTO Products (image, name, size, origin, color, age, material, price, stock_quantity, category_id)
VALUES 
('product1.jpg', N'Đĩa gốm', N'20-25 cm, chiều cao 3-5 cm', N'Nhật bản', N'Màu xanh rêu nhạt', N'Sản phẩm mới, còn nguyên vẹn với độ bóng', NULL, 40000.00, 20, 2),
('product2.jpg', N'Cốc Gốm Thủ Công', N'Đường kính miệng cốc 8-10 cm, Chiều cao 8-10 cm', N'Nhật bản', N'Màu trắng sữa nhạt', N'Sản phẩm còn mới, bề mặt và mép cốc nhẵn mịn, không có vết xước hay khuyết điểm', N'Gốm thủ công', 60000.00, 20, 3),
('product3.jpg', N'Đĩa gốm trắng trơn', N'Đường kính đĩa 15-18cm, Chiều cao 2-3 cm', N'Nhật bản', N'Trắng', N'Sản phẩm mới, không trầy xước', N'Gốm', 60000.00, 20, 2),
('product4.jpg', N'Đĩa Gốm Nhật Bản - Hồng Nhạt', N'16.5x 12 cm', N'Nhật bản', N'Hồng nhạt', N'Sản phẩm còn mới', NULL, 70000.00, 20, 2),
('product5.jpg', N'Chén Gốm Thủ Công Họa Tiết Vân Nứt', N'12x5 cm', N'Nhật bản', N'Màu trắng sữa', N'Sản phẩm còn mới', NULL, 75000.00, 20, 3),
('product6.jpg', N'Chén Gốm Thủ Công Họa Tiết Vân Nứt Và Đáy Gỗ', N'12x5 cm', N'Nhật bản', N'Màu trắng sữa', N'Sản phẩm còn mới', NULL, 55000.00, 20, 3),
('product7.jpg', N'Cốc Gốm Thủ Công Họa Tiết Xanh Nâu', N'8x10 cm', N'Nhật bản', N'Sự kết hợp hài hòa giữa màu xanh nhạt và nâu sẫm', N'Sản phẩm còn mới', NULL, 55000.00, 20, 3),
('product8.jpg', N'Đĩa Gốm Vuông Họa Tiết Màu Xanh Lá', N'16.5x12 cm', N'Nhật bản', N'Màu xanh lá', N'Sản phẩm còn mới', NULL, 50000.00, 20, 2),
('product9.jpg', N'Đĩa Gốm Hình Ấm Trà', N'12x9', N'Nhật bản', N'Màu trắng ngà', N'Sản phẩm còn mới', NULL, 110000.00, 20, 4),
('product10.jpg', N'Bát Gốm Trắng Viền Xanh', N'15x6 cm', N'Nhật bản', N'Màu trắng ngà', N'Sản phẩm còn mới', NULL, 40000.00, 20, 1),
('product11.jpg', N'Đĩa Gốm thuyền Hoa Văn Hồng', N'22x10 cm', N'Nhật bản', N'Màu trắng ngà', N'Sản phẩm còn mới', NULL, 50000.00, 20, 2),
('product12.jpg', N'Đĩa Gốm Oval Viền Đen', N'25x15', N'Nhật bản', N'Màu trắng ngà', N'Sản phẩm còn mới', NULL, 60000.00, 20, 2),
('product13.jpg', N'Đĩa Gốm Oval Hoa Văn Cá', N'22x12 cm', N'Nhật bản', N'Nền trắng', N'Sản phẩm còn mới', NULL, 60000.00, 20, 2),
--('product14.jpg', N'Đĩa Gốm Tròn Hoa Văn', N'18 cm', N'Nhật bản', N'Màu xám nhạt', N'Sản phẩm còn mới', NULL, 65000.00, 20, 2),
('product14.jpg', N'Đĩa Gốm Tròn Màu Xanh Viền Chữ "DELICIOUS"', N'25 cm', N'Nhật bản', N'Màu xanh dương', N'Sản phẩm còn mới', NULL, 60000.00, 20, 2),
('product15.jpg', N'Bộ Bát Gốm Hoa Văn Trang Nhã', N'12x6 cm', N'Nhật bản', N'Màu xám nhạt', N'Sản phẩm còn mới', NULL, 35000.00, 20, 4);

INSERT INTO Orders (user_id, total_price, status)
VALUES 
(2, 125000, N'Đang giao'),
(3, 80000, N'Hoàn thành'),
(2, 175000, N'Đang giao');

INSERT INTO Reviews (user_id, product_id, rating, comment)
VALUES 
(2, 1, 5, N'Bát sứ rất đẹp và chất lượng tốt!'),
(3, 2, 4, N'Đĩa có hoa văn rất tinh tế.'),
(2, 3, 5, N'Bình hoa thiết kế đẹp, mình rất thích.');
