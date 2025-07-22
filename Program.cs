using ClosedXML.Excel;
using LeitorXmls.Models;
using LeitorXmls.Services;


namespace LeitorXmls
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(" LEITOR DE XMLs DE NOTAS FISCAIS DO COFRE  ");
            Console.WriteLine(" DIGITE AQUI O CAMINHO DA PASTA A SER LIDA: ");

            string caminhoPasta = Console.ReadLine()?.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(caminhoPasta) || !Directory.Exists(caminhoPasta))
            {
                Console.WriteLine("Caminho inválido. Encerrando o programa.");
                return;
            }

            string[] arquivosXml = Directory.GetFiles(caminhoPasta, "*.xml", SearchOption.AllDirectories);
            var notas = new List<NotaFiscal>();

            foreach (var arquivo in arquivosXml)
            {
                var nota = LeitorXmlsService.LerXml(arquivo);
                if (nota != null)
                {
                    notas.Add(nota);
                }
            }

            if (notas.Count == 0)
            {
                Console.WriteLine("Nenhuma nota válida foi encontrada.");
                return;
            }

            string mongoConnectionString = "mongodb://localhost:27017";
            string mongoDatabaseName = "LeitorNotas";

            var mongoService = new MongoDbService(mongoConnectionString, mongoDatabaseName);
            mongoService.InserirNotas(notas);

            Console.WriteLine("Notas inseridas no banco de dados MongoDB.");

            string pastaBase = Path.Combine(caminhoPasta, "NotasPorTipo");
            Directory.CreateDirectory(pastaBase);

            var notasPorTipo = notas.GroupBy(n => n.Tipo);

            foreach (var grupo in notasPorTipo)
            {
                string tipo = grupo.Key.ToString();
                string pastaTipo = Path.Combine(pastaBase, tipo);
                Directory.CreateDirectory(pastaTipo);

                string caminhoExcelTipo = Path.Combine(pastaTipo, $"{tipo}.xlsx");

                XLWorkbook workbook;
                IXLWorksheet planilha;

                if (File.Exists(caminhoExcelTipo))
                {
                    workbook = new XLWorkbook(caminhoExcelTipo);
                    planilha = workbook.Worksheet("Notas");
                }
                else
                {
                    workbook = new XLWorkbook();
                    planilha = workbook.Worksheets.Add("Notas");

                    planilha.Cell(1, 1).Value = "Tipo";
                    planilha.Cell(1, 2).Value = "Chave";
                    planilha.Cell(1, 3).Value = "Data Emissão";
                    planilha.Cell(1, 4).Value = "Número";
                    planilha.Cell(1, 5).Value = "CNPJ Emitente";
                    planilha.Cell(1, 6).Value = "Nome Emitente";
                    planilha.Cell(1, 7).Value = "CNPJ Destinatário";
                    planilha.Cell(1, 8).Value = "Nome Destinatário";
                    planilha.Cell(1, 9).Value = "Valor Total";
                }

                int linha = planilha.LastRowUsed()?.RowNumber() + 1 ?? 2;

                foreach (var nota in grupo)
                {
                    planilha.Cell(linha, 1).Value = nota.Tipo.ToString();
                    planilha.Cell(linha, 2).Value = nota.Chave;
                    planilha.Cell(linha, 3).Value = nota.DataEmissao.ToString("dd/MM/yyyy");
                    planilha.Cell(linha, 4).Value = nota.NumeroNota;
                    planilha.Cell(linha, 5).Value = nota.CnpjEmitente;
                    planilha.Cell(linha, 6).Value = nota.NomeEmitente;
                    planilha.Cell(linha, 7).Value = nota.CnpjDestinatario;
                    planilha.Cell(linha, 8).Value = nota.NomeDestinatario;
                    planilha.Cell(linha, 9).Value = nota.ValorTotal;
                    linha++;
                }

                workbook.SaveAs(caminhoExcelTipo);
                workbook.Dispose();

                foreach (var nota in grupo)
                {
                    try
                    {
                        if (!File.Exists(nota.CaminhoOriginal))
                        {
                            Console.WriteLine($"Arquivo já está movido ou não encontrado: {nota.CaminhoOriginal}");
                            continue;
                        }

                        string nomeArquivo = Path.GetFileName(nota.CaminhoOriginal);
                        string destinoFinal = Path.Combine(pastaTipo, nomeArquivo);

                        if (File.Exists(destinoFinal))
                            File.Delete(destinoFinal);

                        File.Move(nota.CaminhoOriginal, destinoFinal);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao mover {nota.CaminhoOriginal}: {ex.Message}");
                    }
                }
            }

            Console.WriteLine("Organização dos XMLs e criação das planilhas por tipo concluída.");
        }
    }
}
