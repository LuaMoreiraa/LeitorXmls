using LeitorXmls.Models;
using System.Xml.Linq;

namespace LeitorXmls.Services
{
    public static class LeitorXmlsService
    {
        public static NotaFiscal LerXml(string caminho)
        {
            try
            {
                using var stream = File.OpenRead(caminho);
                var xml = XDocument.Load(stream);

                string chave = ObterChaveDeAcesso(xml);
                NotaFiscal nota = null;

                if (!string.IsNullOrEmpty(chave) && chave.Length >= 22)
                {
                    string modelo = chave.Substring(20, 2);

                    nota = modelo switch
                    {
                        "55" => LerXmlNfe(xml),
                        "65" => LerXmlNfce(xml),
                        "57" => LerXmlCte(xml),
                        "59" => LerXmlCfe(xml),
                        _ => null
                    };
                }

                if (nota == null)
                {
                    var raiz = xml.Root?.Name.LocalName;

                    if (raiz == "NFSe")
                        nota = LerXmlNfse(xml);
                    else
                        nota = null;
                }


                if (nota != null)
                {
                    nota.CaminhoOriginal = caminho;
                    nota.Chave ??= chave;
                }

                return nota;
            }
            catch
            {
                return null;
            }
        }

        private static string ObterChaveDeAcesso(XDocument xml)
        {
            var id = xml.Descendants()
                .FirstOrDefault(x =>
                    x.Name.LocalName == "infNFe" ||
                    x.Name.LocalName == "infCFe" ||
                    x.Name.LocalName == "infCte")
                ?.Attribute("Id")?.Value;

            if (string.IsNullOrEmpty(id))
                return null;

            return id.Replace("NFe", "")
                     .Replace("CFe", "")
                     .Replace("CTe", "");
        }

        private static NotaFiscal LerXmlNfe(XDocument xml)
        {
            var ide = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "ide");
            var emit = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "emit");
            var dest = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "dest");
            var total = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "ICMSTot");

            return new NotaFiscal
            {
                Tipo = TipoNota.NFe,
                Chave = ObterChaveDeAcesso(xml),
                DataEmissao = ParseData(ide, "dhEmi", "dEmi"),
                NumeroNota = int.Parse(ide?.Element(ide.Name.Namespace + "nNF")?.Value ?? "0"),
                CnpjEmitente = emit?.Element(emit.Name.Namespace + "CNPJ")?.Value,
                NomeEmitente = emit?.Element(emit.Name.Namespace + "xNome")?.Value,
                CnpjDestinatario = dest?.Element(dest.Name.Namespace + "CNPJ")?.Value ?? dest?.Element(dest.Name.Namespace + "CPF")?.Value,
                NomeDestinatario = dest?.Element(dest.Name.Namespace + "xNome")?.Value,
                ValorTotal = decimal.Parse(total?.Element(total.Name.Namespace + "vNF")?.Value ?? "0")
            };
        }

        private static NotaFiscal LerXmlNfce(XDocument xml)
        {
            var nota = LerXmlNfe(xml);
            if (nota != null)
                nota.Tipo = TipoNota.NFCe;

            return nota;
        }


        private static NotaFiscal LerXmlCte(XDocument xml)
        {
            var ide = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "ide");
            var emit = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "emit");
            var dest = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "dest");
            var vPrest = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "vPrest");

            return new NotaFiscal
            {
                Tipo = TipoNota.CTe,
                Chave = ObterChaveDeAcesso(xml),
                DataEmissao = ParseData(ide, "dhEmi", "dEmi"),
                NumeroNota = int.Parse(ide?.Element(ide.Name.Namespace + "nCT")?.Value ?? "0"),
                CnpjEmitente = emit?.Element(emit.Name.Namespace + "CNPJ")?.Value,
                NomeEmitente = emit?.Element(emit.Name.Namespace + "xNome")?.Value,
                CnpjDestinatario = dest?.Element(dest.Name.Namespace + "CNPJ")?.Value ?? dest?.Element(dest.Name.Namespace + "CPF")?.Value,
                NomeDestinatario = dest?.Element(dest.Name.Namespace + "xNome")?.Value,
                ValorTotal = decimal.Parse(vPrest?.Element(vPrest.Name.Namespace + "vTPrest")?.Value ?? "0")
            };
        }

        private static NotaFiscal LerXmlCfe(XDocument xml)
        {
            var ide = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "ide");
            var emit = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "emit");
            var dest = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "dest");
            var total = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "total");

            return new NotaFiscal
            {
                Tipo = TipoNota.CFe,
                Chave = ObterChaveDeAcesso(xml),
                DataEmissao = ParseData(ide, "dEmi"),
                NumeroNota = int.Parse(ide?.Element(ide.Name.Namespace + "nCFe")?.Value ?? "0"),
                CnpjEmitente = emit?.Element(emit.Name.Namespace + "CNPJ")?.Value,
                NomeEmitente = emit?.Element(emit.Name.Namespace + "xNome")?.Value,
                CnpjDestinatario = dest?.Element(dest.Name.Namespace + "CNPJ")?.Value ?? dest?.Element(dest.Name.Namespace + "CPF")?.Value,
                NomeDestinatario = dest?.Element(dest.Name.Namespace + "xNome")?.Value,
                ValorTotal = decimal.Parse(total?.Element(total.Name.Namespace + "vCFe")?.Value ?? "0")
            };
        }

        private static NotaFiscal LerXmlNfse(XDocument xml)
        {
            var infNfse = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "InfNfse");
            var prestador = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "IdentificacaoPrestador");
            var tomador = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "TomadorServico");
            var valores = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "ValoresNfse");

            return new NotaFiscal
            {
                Tipo = TipoNota.NFSe,
                Chave = infNfse?.Attribute("Id")?.Value,
                DataEmissao = DateTime.Parse(xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "DataEmissao")?.Value),
                NumeroNota = int.Parse(xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "Numero")?.Value ?? "0"),
                CnpjEmitente = prestador?.Element(prestador.Name.Namespace + "Cnpj")?.Value,
                NomeEmitente = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "RazaoSocialPrestador")?.Value,
                CnpjDestinatario = tomador?.Descendants().FirstOrDefault(x => x.Name.LocalName == "CpfCnpj")?.Element(tomador.Name.Namespace + "Cnpj")?.Value,
                NomeDestinatario = tomador?.Element(tomador.Name.Namespace + "RazaoSocial")?.Value,
                ValorTotal = decimal.Parse(valores?.Element(valores.Name.Namespace + "ValorServicos")?.Value ?? "0")
            };
        }

        private static DateTime ParseData(XElement elemento, params string[] nomes)
        {
            foreach (var nome in nomes)
            {
                var el = elemento?.Element(elemento.Name.Namespace + nome);
                if (el != null && DateTime.TryParse(el.Value, out var dt))
                    return dt;
            }

            return DateTime.MinValue;
        }
    }
}