# EspotifieDesktopClient
Cliente de escritorio para el API de EspotiFEI

Para utilizar EspofeiDesktop solo es necesario cambiar la varaible URIRestServer y URIGrpcServer por la direccón del servicio de EspotiFEI API
```
  public static string URIRestServer { get; } = "http://direccion-servicio:5000/";
  public static string URIGrpcServer { get; } = "direccion-servicio:5001";

```
