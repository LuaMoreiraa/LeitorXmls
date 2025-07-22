using LeitorXmls.Models;
using MongoDB.Driver;

namespace LeitorXmls.Services
{
    public class MongoDbService
    {
        private readonly IMongoCollection<NotaFiscal> _notas;

        public MongoDbService(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _notas = database.GetCollection<NotaFiscal>("NotasFiscais");
        }

        public void InserirNotas(List<NotaFiscal> notas)
        {
            foreach (var nota in notas)
            {
                var existente = _notas.Find(n => n.Chave == nota.Chave).FirstOrDefault();
                if (existente == null)
                    _notas.InsertOne(nota);
            }
        }

        public List<NotaFiscal> ListarNotas()
        {
            return _notas.Find(_ => true).ToList();
        }
    }
}
