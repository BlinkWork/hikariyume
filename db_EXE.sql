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
    full_name VARCHAR(255),
    address TEXT,
    phone_number VARCHAR(15),
    role VARCHAR(10) CHECK (role IN ('admin', 'user')) NOT NULL,
    created_at DATETIME DEFAULT GETDATE()
);

CREATE TABLE Categories (
    category_id INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(100) NOT NULL,
    description TEXT
);
CREATE TABLE Products (
    product_id INT PRIMARY KEY IDENTITY(1,1),
    image VARCHAR(100) NOT NULL,
    name NVARCHAR(255) NOT NULL,
    description NVARCHAR(255),
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
    comment TEXT,
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
(N'Bát', 'Các loại bát gốm sứ cao cấp'),
(N'Đĩa', 'Các loại đĩa gốm sứ đa dạng'),
(N'Cốc', 'Các loại cốc đa dạng');

INSERT INTO Products (name, [image], description, price, stock_quantity, category_id)
VALUES 
(N'Bát sứ', 'batsu1.jpg', N'Kích thước 16 cm',65000,10, 1), 
(N'Bát sứ 2','batsu2.jpg',N'Kích thước 14 cm',35000,10,1),
(N'Đĩa thuyền hoa','diathuyenhoa.jpg',N'Kích thước 22x10cm',50000, 10,2),
(N'Đĩa gốm','diagom.jpg',N'Kích thước 20cm',80000,10,2),
(N'Đĩa họa tiết','diahoatiet.jpg',N'Kích thước 12cm',30000,10,2);

INSERT INTO Orders (user_id, total_price, status)
VALUES 
(2, 125000, N'Đang giao'),
(3, 80000, N'Hoàn thành'),
(2, 175000, N'Đang giao');

INSERT INTO Order_Items (order_id, product_id, quantity, price)
VALUES 
(1, 1, 2, 50000),
(1, 2, 1, 75000),
(2, 4, 1, 80000),
(3, 3, 1, 120000),
(3, 1, 1, 50000);

INSERT INTO Reviews (user_id, product_id, rating, comment)
VALUES 
(2, 1, 5, N'Bát sứ rất đẹp và chất lượng tốt!'),
(3, 2, 4, N'Đĩa có hoa văn rất tinh tế.'),
(2, 3, 5, N'Bình hoa thiết kế đẹp, mình rất thích.');
