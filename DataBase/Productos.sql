ALTER TABLE productos
ADD 
    stock_caja INT DEFAULT 0,
    stock_blister INT DEFAULT 0,
    stock_unidad INT DEFAULT 0,
    precio_unidad DECIMAL(10, 2) DEFAULT 0,
    precio_blister DECIMAL(10, 2) DEFAULT 0,
    precio_caja DECIMAL(10, 2) DEFAULT 0;


ALTER TABLE productos
ADD 
    Fecha_Vencimiento Date  DEFAULT 0


Select * from productos

TRUNCATE TABLE productos;

CREATE TABLE catalogo_proveedores (
    idproveedor INT IDENTITY PRIMARY KEY,
    nombre VARCHAR(255),
    contacto VARCHAR(255),
    direccion VARCHAR(500),
    correo VARCHAR(255),
    telefono VARCHAR(50),
    nit VARCHAR(50),
    condicion_pago VARCHAR(255),
    moneda VARCHAR(10),
    pais VARCHAR(100)
);


Select * from catalogo_proveedores

ALTER TABLE productos
ADD idproveedor INT;

ALTER TABLE productos
ADD CONSTRAINT fk_idproveedor_prod FOREIGN KEY (idproveedor) REFERENCES catalogo_proveedores(idproveedor);

SELECT 
    p.idproducto,
    p.nombre,
    p.Descripcion,
    p.rutaimg,
    p.nombreimg,
    p.estado,
    p.fecharegistro,
    p.stock_caja,
    p.stock_blister,
    p.stock_unidad,
    p.precio_unidad,
    p.precio_blister,
    p.precio_caja,
    cp.nombre AS nombre_proveedor
FROM 
    dbo.productos p
JOIN 
    dbo.catalogo_proveedores cp
ON 
    p.idproveedor = cp.idproveedor;


CREATE TABLE laboratorio (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(255) NOT NULL,
    activo CHAR(1) NOT NULL CHECK (activo IN ('S', 'N'))
);

Select * from laboratorio

ALTER TABLE productos
ADD idlaboratorio INT;

ALTER TABLE productos
ADD CONSTRAINT fk_idlaboratorio_prod FOREIGN KEY (idlaboratorio) REFERENCES laboratorio(id);

ALTER TABLE productos
ADD 
    credito_fiscal BIT DEFAULT 0,
    vitrina BIT DEFAULT 0,
    presentacion VARCHAR(255) NULL,
    codigo_barras VARCHAR(50) NULL,
    absorcion VARCHAR(255) NULL,
    compuesto VARCHAR(255) NULL,
    fecha_vencimiento DATE NULL;


ALTER TABLE catalogo_proveedores
ADD estado BIT DEFAULT 1;


INSERT INTO catalogo_proveedores (nombre, contacto, direccion, correo, telefono, nit, condicion_pago, moneda, pais, estado)
VALUES 
('Drogueria San José', 'Miguel Angel Leiva', 'by pass km 76 1/2, #7, Chalchuapa, Sta', 'drogueriasanjosechalchuapa@gmail.com', '6420-0817', '00969581-0', 'contado', 'dolar', 'El Salvador', 1),
('C Imberton', 'Denys Flores C', 'Km 11 ½ Carretera al Puerto de La Libert', 'www.cimberton.com.sv', '2228-5666', '0614081260145', 'contado', 'dolar', 'El Salvador', 1),
('Vijosa', 'Roxana Martínez', 'Calle Primavera y 23 Av sur, carretera panam', 'info@vijosa.com', '2251-9797', '06142047750010', 'contado', 'dolar', 'El Salvador', 1),
('Drogueria Santa Lucia', 'Herbert Cortez', 'Calle y Colonia Roma, #328 San Salvador', 'ventas@drogueriasantalucia.com', '2250-6200', '06142801420027', 'contado', 'dolar', 'El Salvador', 1),
('Corporacion Cefa', 'Frederick Navas', 'Calle Siemens y Av Lamatepec 55-56 Antiguo Cuscatlan', 'adtrecepcion_dte@cefaelsalvad', '2592-1100', '06142704001062', 'contado', 'dolar', 'El Salvador', 1),
('Drogueria Droiphar', 'Patty de Paz', 'Av Maquilishuat #270, Col Vista Hermosa, San Salvador', 'droiphar@aciselsa.com.sv', '7850-3441', '0614140909145', 'contado', 'dolar', 'El Salvador', 1),
('Mundo Farma', 'Mauricio Alvarado', 'la AV Nte, Barrio San Miguelito, #838, San Salvador', '', '2327-5505', '06141802781118', 'contado', 'dolar', 'El Salvador', 1),
('Pragma', 'Iris Lopez', '29 calle pte y 10ª AV sur #136, Santa Ana', 'josuegalan@hotmail.com', '7822-0667', '02101801791015', 'contado', 'dolar', 'El Salvador', 1),
('Drogueria Pajoca', 'Omar Campos', 'Finca El Molino, Canton Natividad, Carret antigua a San Salvador, San Salvador', '', '2283-9682', '02140241071207', 'contado', 'dolar', 'El Salvador', 1),
('Drogueria Insaya', 'Ernesto Figueroa', 'Calle Teolt, Pólig Z-1, pie A, Urb Cumbres de Cuscatlan, #1, Antiguo', '', '2273-424', '061420202161019', 'contado', 'dolar', 'El Salvador', 1);

CREATE PROCEDURE spu_registrar_productos(
    @idcategoria INT,
    @nombre VARCHAR(100),
    @Descripcion VARCHAR(500),
    @stock_caja INT,
    @stock_blister INT,
    @stock_unidad INT,
    @precio_caja DECIMAL(10,2),
    @precio_blister DECIMAL(10,2),
    @precio_unidad DECIMAL(10,2),
    @idproveedor INT,
    @idlaboratorio INT,
    @credito_fiscal BIT,
    @vitrina BIT,
    @presentacion VARCHAR(255),
    @codigo_barras VARCHAR(50),
    @absorcion VARCHAR(255),
    @compuesto VARCHAR(255),
    @fecha_vencimiento DATE,
    @estado BIT,
    @mensaje VARCHAR(60) OUTPUT,
    @resultado INT OUTPUT
)
AS
BEGIN
    SET @resultado = 0
    IF NOT EXISTS (SELECT * FROM productos WHERE nombre = @nombre)
    BEGIN
        INSERT INTO productos(idcategoria, nombre, Descripcion, stock_caja, stock_blister, stock_unidad, precio_caja, precio_blister, precio_unidad, idproveedor, idlaboratorio, credito_fiscal, vitrina, presentacion, codigo_barras, absorcion, compuesto, fecha_vencimiento, estado)
        VALUES (@idcategoria, @nombre, @Descripcion, @stock_caja, @stock_blister, @stock_unidad, @precio_caja, @precio_blister, @precio_unidad, @idproveedor, @idlaboratorio, @credito_fiscal, @vitrina, @presentacion, @codigo_barras, @absorcion, @compuesto, @fecha_vencimiento, @estado)
        SET @resultado = SCOPE_IDENTITY()
    END
    ELSE
        SET @mensaje = 'El producto ya existe'
END
GO

CREATE PROCEDURE spu_editar_productos(
    @idproducto INT,
    @idcategoria INT,
    @nombre VARCHAR(100),
    @Descripcion VARCHAR(500),
    @stock_caja INT,
    @stock_blister INT,
    @stock_unidad INT,
    @precio_caja DECIMAL(10,2),
    @precio_blister DECIMAL(10,2),
    @precio_unidad DECIMAL(10,2),
    @idproveedor INT,
    @idlaboratorio INT,
    @credito_fiscal BIT,
    @vitrina BIT,
    @presentacion VARCHAR(255),
    @codigo_barras VARCHAR(50),
    @absorcion VARCHAR(255),
    @compuesto VARCHAR(255),
    @fecha_vencimiento DATE,
    @estado BIT,
    @mensaje VARCHAR(60) OUTPUT,
    @resultado BIT OUTPUT
)
AS
BEGIN
    SET @resultado = 0
    IF NOT EXISTS (SELECT * FROM productos WHERE nombre = @nombre AND idproducto != @idproducto)
    BEGIN
        UPDATE productos SET
            idcategoria = @idcategoria,
            nombre = @nombre,
            Descripcion = @Descripcion,
            stock_caja = @stock_caja,
            stock_blister = @stock_blister,
            stock_unidad = @stock_unidad,
            precio_caja = @precio_caja,
            precio_blister = @precio_blister,
            precio_unidad = @precio_unidad,
            idproveedor = @idproveedor,
            idlaboratorio = @idlaboratorio,
            credito_fiscal = @credito_fiscal,
            vitrina = @vitrina,
            presentacion = @presentacion,
            codigo_barras = @codigo_barras,
            absorcion = @absorcion,
            compuesto = @compuesto,
            fecha_vencimiento = @fecha_vencimiento,
            estado = @estado
        WHERE idproducto = @idproducto
        SET @resultado = 1
    END
    ELSE
        SET @mensaje = 'El producto ya existe'
END
GO

CREATE PROCEDURE spu_eliminar_producto(
    @idproducto INT,
    @mensaje VARCHAR(60) OUTPUT,
    @resultado BIT OUTPUT
)
AS
BEGIN
    SET @resultado = 0
    IF NOT EXISTS (SELECT * FROM detallesVentas dv INNER JOIN productos p ON p.idproducto = dv.idproducto WHERE p.idproducto = @idproducto)
    BEGIN
        DELETE FROM productos WHERE idproducto = @idproducto
        SET @resultado = 1
    END
    ELSE
        SET @mensaje = 'El producto se encuentra relacionado a una venta'
END
GO

CREATE PROCEDURE sp_ListarLaboratorios
AS
BEGIN
    SELECT id, nombre, estado
    FROM Laboratorio
    ORDER BY nombre;
END;

CREATE PROCEDURE sp_GuardarLaboratorio
    @id INT = 0,  -- Default to 0 if not provided
    @nombre VARCHAR(100),
    @estado BIT
AS
BEGIN
    IF @id = 0
    BEGIN
        INSERT INTO Laboratorio (nombre, estado)
        VALUES (@nombre, @estado);
    END
    ELSE
    BEGIN
        UPDATE Laboratorio
        SET nombre = @nombre,
            estado = @estado
        WHERE id = @id;
    END
END;


CREATE PROCEDURE sp_EliminarLaboratorio
    @id INT
AS
BEGIN
    DELETE FROM Laboratorio
    WHERE id = @id;
END;

SELECT * FROM dbo.laboratorio

CREATE PROCEDURE sp_ListarLaboratorios
AS
BEGIN
    SELECT id, nombre, estado
    FROM laboratorio
END

SELECT *
FROM sys.tables
WHERE name = 'laboratorio';

DROP TABLE IF EXISTS [dbo].[laboratorio];

DROP TABLE [dbo].[laboratorio];

SELECT 
    fk.name AS ForeignKeyName,
    tp.name AS ParentTable,
    tr.name AS ReferencedTable
FROM 
    sys.foreign_keys AS fk
INNER JOIN 
    sys.tables AS tp ON fk.parent_object_id = tp.object_id
INNER JOIN 
    sys.tables AS tr ON fk.referenced_object_id = tr.object_id
WHERE 
    tr.name = 'laboratorio';


ALTER TABLE productos DROP CONSTRAINT fk_idlaboratorio_prod;

CREATE TABLE [dbo].[laboratorio](
    [id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Nombre] [Varchar] (50) NOT NULL,
    [estado] [bit] NOT NULL
);


ALTER TABLE [dbo].[productos]
ADD CONSTRAINT FK_Productos_Laboratorio
FOREIGN KEY (idlaboratorio) REFERENCES [dbo].[laboratorio](id);

SELECT * FROM [dbo].[laboratorio];

EXEC sp_GuardarLaboratorio @id = 0, @nombre = 'MK', @estado = 1;


ALTER PROCEDURE [dbo].[sp_ListarLaboratorios]
AS
BEGIN
    SELECT id, nombre, estado
    FROM laboratorio
END


-- Prueba de inserción
EXEC sp_GuardarLaboratorio @id = 0, @nombre = 'Laboratorio Prueba', @estado = 1;

SELECT * FROM dbo.laboratorio

-- Prueba de actualización
EXEC sp_GuardarLaboratorio @id = 1, @nombre = 'Laboratorio Actualizado', @estado = 0;
