namespace PedidosApi.Models;
public class Pedido { public int Id {get;set;} public int ClienteId {get;set;} public string Descripcion {get;set;}=""; public decimal Total {get;set;} public DateTime Fecha {get;set;} public string Estado {get;set;}="Pendiente"; }
