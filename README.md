# LeitorXML

LeitorXML é um projeto em C# que faz a leitura e interpretação de arquivos XML de Notas Fiscais Eletrônicas (NFe, NFCe, CTe, NFSe, CFe). O projeto puxa as principais informações de cada tipo de nota e gera objetos para fácil manipulação e integração. O projeto separa as notas por tipo e cria planilhas em Excel para cada tipo de nota.

## Funcionalidades

- Leitura de uma pasta contendo arquivos XML de notas fiscais. 
- Identificação automática do tipo da nota fiscal (Enum: NFe, CTe, NFCe, NFSe, CFe). 
- Extração das principais informações do XML:
- Chave do XML (string)
- Data de Emissão (DateTime)
- Número da Nota (int)
- CNPJ do Emitente (string)
- Nome do Emitente (string)
- CNPJ do Destinatário (string)
- Nome do Destinatário (string)
- Valor Total da Nota (decimal)
- Exportação da lista de notas para planilha Excel. 

---

## Tecnologias

- .NET (C#)
- ClosedXML (Para a exportação dos dados para planilhas)
- MongoDB Compass

---


## Proximos passos

- Criar uma interface Web para facilitar o usuario a baixar e visualizar.


## Como usar

1. Clone este repositório:

```bash
git clone https://github.com/seuusuario/LeitorXML.git
cd LeitorXML

