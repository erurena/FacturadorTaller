/*    ==Scripting Parameters==

    Source Server Version : SQL Server 2016 (13.0.1601)
    Source Database Engine Edition : Microsoft SQL Server Enterprise Edition
    Source Database Engine Type : Standalone SQL Server

    Target Server Version : SQL Server 2017
    Target Database Engine Edition : Microsoft SQL Server Standard Edition
    Target Database Engine Type : Standalone SQL Server
*/
USE [erurena_Salon]
GO
ALTER TABLE [cotDor].[UserRoles] DROP CONSTRAINT [FK_cotDor.UserRoles_Salonalba.Users_IdentityUser_Id]
GO
ALTER TABLE [cotDor].[UserRoles] DROP CONSTRAINT [FK_cotDor.UserRoles_Salonalba.Roles_IdentityRole_Id]
GO
ALTER TABLE [cotDor].[UserClaims] DROP CONSTRAINT [FK_cotDor.UserClaims_Salonalba.Users_IdentityUser_Id]
GO
ALTER TABLE [cotDor].[Factura] DROP CONSTRAINT [FK_Factura_Cotizacion]
GO
ALTER TABLE [cotDor].[ExternalLogins] DROP CONSTRAINT [FK_cotDor.ExternalLogins_Salonalba.Users_IdentityUser_Id]
GO
ALTER TABLE [cotDor].[DetalleCot] DROP CONSTRAINT [FK_DetalleCot_Producto]
GO
ALTER TABLE [cotDor].[DetalleCot] DROP CONSTRAINT [FK_DetalleCot_Cotizacion]
GO
ALTER TABLE [cotDor].[Cotizacion] DROP CONSTRAINT [FK_Cotizacion_Clientes]
GO
/****** Object:  Table [cotDor].[Users]    Script Date: 16/10/2017 21:48:58 ******/
DROP TABLE [cotDor].[Users]
GO
/****** Object:  Table [cotDor].[UserRoles]    Script Date: 16/10/2017 21:48:58 ******/
DROP TABLE [cotDor].[UserRoles]
GO
/****** Object:  Table [cotDor].[UserClaims]    Script Date: 16/10/2017 21:48:58 ******/
DROP TABLE [cotDor].[UserClaims]
GO
/****** Object:  Table [cotDor].[Roles]    Script Date: 16/10/2017 21:48:58 ******/
DROP TABLE [cotDor].[Roles]
GO
/****** Object:  Table [cotDor].[Producto]    Script Date: 16/10/2017 21:48:58 ******/
DROP TABLE [cotDor].[Producto]
GO
/****** Object:  Table [cotDor].[Ncf]    Script Date: 16/10/2017 21:48:58 ******/
DROP TABLE [cotDor].[Ncf]
GO
/****** Object:  Table [cotDor].[FlujoCaja]    Script Date: 16/10/2017 21:48:58 ******/
DROP TABLE [cotDor].[FlujoCaja]
GO
/****** Object:  Table [cotDor].[Factura]    Script Date: 16/10/2017 21:48:58 ******/
DROP TABLE [cotDor].[Factura]
GO
/****** Object:  Table [cotDor].[ExternalLogins]    Script Date: 16/10/2017 21:48:58 ******/
DROP TABLE [cotDor].[ExternalLogins]
GO
/****** Object:  Table [cotDor].[DetalleCot]    Script Date: 16/10/2017 21:48:58 ******/
DROP TABLE [cotDor].[DetalleCot]
GO
/****** Object:  Table [cotDor].[Cotizacion]    Script Date: 16/10/2017 21:48:58 ******/
DROP TABLE [cotDor].[Cotizacion]
GO
/****** Object:  Table [cotDor].[Clientes]    Script Date: 16/10/2017 21:48:58 ******/
DROP TABLE [cotDor].[Clientes]
GO
/****** Object:  Table [cotDor].[Clientes]    Script Date: 16/10/2017 21:48:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [cotDor].[Clientes](
	[ClienteId] [int] IDENTITY(1,1) NOT NULL,
	[NombreCliente] [nvarchar](max) NULL,
	[RncCliente] [nvarchar](max) NULL,
	[DireccionCliente] [nvarchar](max) NULL,
	[ContactoCliente] [nvarchar](max) NULL,
	[TelefonoCliente] [nvarchar](max) NULL,
	[CorreoCliente] [nvarchar](max) NULL,
 CONSTRAINT [PK_cotDor.Clientes] PRIMARY KEY CLUSTERED 
(
	[ClienteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [cotDor].[Cotizacion]    Script Date: 16/10/2017 21:48:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [cotDor].[Cotizacion](
	[CotizacionId] [int] IDENTITY(1,1) NOT NULL,
	[Fecha] [datetime] NOT NULL,
	[ClienteId] [int] NOT NULL,
	[TotalFactura] [decimal](18, 0) NOT NULL,
	[Itbis] [decimal](18, 0) NOT NULL,
 CONSTRAINT [PK_cotDor.Cotizacion] PRIMARY KEY CLUSTERED 
(
	[CotizacionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [cotDor].[DetalleCot]    Script Date: 16/10/2017 21:48:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [cotDor].[DetalleCot](
	[DetalleCotId] [int] IDENTITY(1,1) NOT NULL,
	[CotizacionId] [int] NOT NULL,
	[ProductoId] [int] NOT NULL,
	[Cantidad] [int] NOT NULL,
	[FichaVehiculo] [nvarchar](max) NULL,
	[Valor] [decimal](18, 0) NOT NULL,
	[Comentario] [nvarchar](max) NULL,
 CONSTRAINT [PK_cotDor.DetalleCots] PRIMARY KEY CLUSTERED 
(
	[DetalleCotId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [cotDor].[ExternalLogins]    Script Date: 16/10/2017 21:48:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [cotDor].[ExternalLogins](
	[UserId] [nvarchar](128) NOT NULL,
	[LoginProvider] [nvarchar](max) NULL,
	[ProviderKey] [nvarchar](max) NULL,
	[IdentityUser_Id] [nvarchar](128) NULL,
 CONSTRAINT [PK_cotDor.ExternalLogins] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [cotDor].[Factura]    Script Date: 16/10/2017 21:48:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [cotDor].[Factura](
	[FacturaId] [int] IDENTITY(1,1) NOT NULL,
	[CotizacionId] [int] NOT NULL,
	[FechaFac] [datetime] NOT NULL,
	[FechaVen] [datetime] NOT NULL,
	[Ncf] [nvarchar](max) NULL,
	[OrdenCompraNu] [nvarchar](max) NOT NULL,
	[ImgOrdenCompra] [nvarchar](max) NULL,
 CONSTRAINT [PK_cotDor.Factura] PRIMARY KEY CLUSTERED 
(
	[FacturaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [cotDor].[FlujoCaja]    Script Date: 16/10/2017 21:48:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [cotDor].[FlujoCaja](
	[FlujoCajaId] [int] IDENTITY(1,1) NOT NULL,
	[Categoria] [nvarchar](max) NULL,
	[Descripcion] [nvarchar](max) NULL,
	[Valor] [int] NULL,
	[Fecha] [datetime] NOT NULL,
 CONSTRAINT [PK_cotDor.FlujoCaja] PRIMARY KEY CLUSTERED 
(
	[FlujoCajaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [cotDor].[Ncf]    Script Date: 16/10/2017 21:48:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [cotDor].[Ncf](
	[NcfId] [int] IDENTITY(1,1) NOT NULL,
	[Inicio] [nvarchar](max) NULL,
	[Contador] [int] NOT NULL,
	[NumInicio] [int] NOT NULL,
	[NumFin] [int] NOT NULL,
	[NumActual] [int] NOT NULL,
 CONSTRAINT [PK_cotDor.Ncf] PRIMARY KEY CLUSTERED 
(
	[NcfId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [cotDor].[Producto]    Script Date: 16/10/2017 21:48:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [cotDor].[Producto](
	[ProductoId] [int] IDENTITY(1,1) NOT NULL,
	[NombreProducto] [nvarchar](max) NULL,
	[Categoria] [nvarchar](max) NULL,
 CONSTRAINT [PK_cotDor.Productos] PRIMARY KEY CLUSTERED 
(
	[ProductoId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [cotDor].[Roles]    Script Date: 16/10/2017 21:48:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [cotDor].[Roles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](max) NULL,
 CONSTRAINT [PK_Salonalba.Roles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [cotDor].[UserClaims]    Script Date: 16/10/2017 21:48:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [cotDor].[UserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](max) NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
	[IdentityUser_Id] [nvarchar](128) NULL,
 CONSTRAINT [PK_cotDor.UserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [cotDor].[UserRoles]    Script Date: 16/10/2017 21:48:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [cotDor].[UserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](max) NULL,
	[IdentityRole_Id] [nvarchar](128) NULL,
	[IdentityUser_Id] [nvarchar](128) NULL,
 CONSTRAINT [PK_cotDor.UserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [cotDor].[Users]    Script Date: 16/10/2017 21:48:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [cotDor].[Users](
	[Id] [nvarchar](128) NOT NULL,
	[Email] [nvarchar](max) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](max) NULL,
	[Discriminator] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_cotDor.Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [cotDor].[Cotizacion]  WITH CHECK ADD  CONSTRAINT [FK_Cotizacion_Clientes] FOREIGN KEY([ClienteId])
REFERENCES [cotDor].[Clientes] ([ClienteId])
GO
ALTER TABLE [cotDor].[Cotizacion] CHECK CONSTRAINT [FK_Cotizacion_Clientes]
GO
ALTER TABLE [cotDor].[DetalleCot]  WITH CHECK ADD  CONSTRAINT [FK_DetalleCot_Cotizacion] FOREIGN KEY([CotizacionId])
REFERENCES [cotDor].[Cotizacion] ([CotizacionId])
GO
ALTER TABLE [cotDor].[DetalleCot] CHECK CONSTRAINT [FK_DetalleCot_Cotizacion]
GO
ALTER TABLE [cotDor].[DetalleCot]  WITH CHECK ADD  CONSTRAINT [FK_DetalleCot_Producto] FOREIGN KEY([ProductoId])
REFERENCES [cotDor].[Producto] ([ProductoId])
GO
ALTER TABLE [cotDor].[DetalleCot] CHECK CONSTRAINT [FK_DetalleCot_Producto]
GO
ALTER TABLE [cotDor].[ExternalLogins]  WITH CHECK ADD  CONSTRAINT [FK_cotDor.ExternalLogins_Salonalba.Users_IdentityUser_Id] FOREIGN KEY([IdentityUser_Id])
REFERENCES [cotDor].[Users] ([Id])
GO
ALTER TABLE [cotDor].[ExternalLogins] CHECK CONSTRAINT [FK_cotDor.ExternalLogins_Salonalba.Users_IdentityUser_Id]
GO
ALTER TABLE [cotDor].[Factura]  WITH CHECK ADD  CONSTRAINT [FK_Factura_Cotizacion] FOREIGN KEY([CotizacionId])
REFERENCES [cotDor].[Cotizacion] ([CotizacionId])
GO
ALTER TABLE [cotDor].[Factura] CHECK CONSTRAINT [FK_Factura_Cotizacion]
GO
ALTER TABLE [cotDor].[UserClaims]  WITH CHECK ADD  CONSTRAINT [FK_cotDor.UserClaims_Salonalba.Users_IdentityUser_Id] FOREIGN KEY([IdentityUser_Id])
REFERENCES [cotDor].[Users] ([Id])
GO
ALTER TABLE [cotDor].[UserClaims] CHECK CONSTRAINT [FK_cotDor.UserClaims_Salonalba.Users_IdentityUser_Id]
GO
ALTER TABLE [cotDor].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_cotDor.UserRoles_Salonalba.Roles_IdentityRole_Id] FOREIGN KEY([IdentityRole_Id])
REFERENCES [cotDor].[Roles] ([Id])
GO
ALTER TABLE [cotDor].[UserRoles] CHECK CONSTRAINT [FK_cotDor.UserRoles_Salonalba.Roles_IdentityRole_Id]
GO
ALTER TABLE [cotDor].[UserRoles]  WITH CHECK ADD  CONSTRAINT [FK_cotDor.UserRoles_Salonalba.Users_IdentityUser_Id] FOREIGN KEY([IdentityUser_Id])
REFERENCES [cotDor].[Users] ([Id])
GO
ALTER TABLE [cotDor].[UserRoles] CHECK CONSTRAINT [FK_cotDor.UserRoles_Salonalba.Users_IdentityUser_Id]
GO
