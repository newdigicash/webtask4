CREATE TABLE Clientes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Identificacion VARCHAR(20) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Apellido VARCHAR(100) NOT NULL,
    Telefono VARCHAR(20) NOT NULL
);

CREATE TABLE Categorias (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(255) NOT NULL,
    Activa TINYINT(1) NOT NULL DEFAULT 1
);

CREATE TABLE Productos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(120) NOT NULL,
    Precio DECIMAL(10, 2) NOT NULL,
    Stock INT NOT NULL DEFAULT 0,
    CategoriaId INT NOT NULL,
    CONSTRAINT FK_Productos_Categorias_CategoriaId
        FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id)
        ON DELETE RESTRICT
);

CREATE TABLE Pedidos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Fecha DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Cantidad INT NOT NULL DEFAULT 1,
    Total DECIMAL(10, 2) NOT NULL,
    ClienteId INT NOT NULL,
    ProductoId INT NOT NULL,
    CONSTRAINT FK_Pedidos_Clientes_ClienteId
        FOREIGN KEY (ClienteId) REFERENCES Clientes(Id)
        ON DELETE RESTRICT,
    CONSTRAINT FK_Pedidos_Productos_ProductoId
        FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
        ON DELETE RESTRICT
);
