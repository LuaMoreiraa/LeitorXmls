using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LeitorXmls.Models
{
    public enum TipoNota
    {
        NFe,
        CTe,
        NFCe,
        NFSe,
        CFe
    }

    public class NotaFiscal
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public TipoNota Tipo { get; set; }
        public string Chave { get; set; }
        public DateTime DataEmissao { get; set; }
        public int NumeroNota { get; set; }
        public string CnpjEmitente { get; set; }
        public string NomeEmitente { get; set; }
        public string CnpjDestinatario { get; set; }
        public string NomeDestinatario { get; set; }
        public decimal ValorTotal { get; set; }
        public string CaminhoOriginal { get; set; }
    }
}
