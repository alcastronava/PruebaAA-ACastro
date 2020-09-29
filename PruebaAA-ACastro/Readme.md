# Prueba AA - Alfredo Castro

Descarga de fichero .csv almacenado en una cuenta de almacenamiento e insertar su contenido en una BD SQL Server local

El fichero contiene las columnas 
PointOfSale, Product, Date, Stock
Como ejemplo:
121017;17240503103734;2019-08-17;2

La primera linea contiene "PointOfSale;Product;Date;Stock", por lo cual es descartada.

Para ejecutar este programa: 
  - Abra el código de la aplicación de consola desde Visual Studio 2019.
  - Compile.
  - En PruebaAAContext.cs Reemplace los valores para "{SQLServerInstance}" y "{DBName}" por algun valor valido para su BD SQL Server local.
  - En Visual Studio2019 Abra el Package Manager Console y ejecute el comando "update-database".
  - Ejecute el programa.

```sh
update-database
```
En Program.cs puede reemplazar los valores para el URL del fichero (url)

# Mejoras que se pueden implementar

  - La URL del fichero a descargar y el fichero destino local pueden ser argumentos del aplicación de consola.
  - Manejar errores de desconexión y descarga.
  - Agregar funcionalidad para cancelar descarga o procesamiento del fichero.
  - El procesamiento del fichero puede hacerse en multithread, dividiendo el archivo en varios lotes o usando alguna función como Parallel.For.
  - El procesamiento del fichero podría hacerse al mismo tiempo que se va descargando.

Quedan pendiente algunas validaciones sobre el archivo.

